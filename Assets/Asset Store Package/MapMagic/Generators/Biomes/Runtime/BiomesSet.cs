using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Products;
using Den.Tools.GUI;

namespace MapMagic.Nodes.Biomes
{
	public class BiomeLayer : IBiome, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
	{
		public float Opacity { get; set; }

		public Generator Gen { get { return gen; } private set { gen = value;} }
		public Generator gen; //property is not serialized
		public void SetGen (Generator gen) => this.gen=gen;

		public ulong id; //properties not serialized
		public ulong Id { get{return id;} set{id=value;} } 
		public ulong LinkedOutletId { get; set; }  //if it's inlet. Assigned every before each clear or generate
		public ulong LinkedGenId { get; set; } 

		public IUnit ShallowCopy() => (BiomeLayer)this.MemberwiseClone();

		public Expose.Override Override { get{return null;} set{} }

		public Graph graph;
		public Graph SubGraph => graph;
		public Graph AssignedGraph => graph;
		public TileData SubData (TileData parent) => parent.GetSubData(id);

		public BiomeLayer () => Opacity=1;
	}


	[Serializable]
	[GeneratorMenu (menu="Biomes", name ="Biomes Set", iconName="GeneratorIcons/Biome", priority = 1, colorType = typeof(IBiome))]
	public class BiomesSet200 : Generator, IMultiInlet, IMultiLayer, ICustomComplexity, ICustomClear, IPrepare
	{
		public BiomeLayer[] layers = new BiomeLayer[0];
		public IList<IUnit> Layers { get => layers; set => layers=ArrayTools.Convert<BiomeLayer,IUnit>(value); }
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(BiomeLayer)i);
		public bool Inversed => true;
		public bool HideFirst => true;

		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			foreach (BiomeLayer layer in layers)
				yield return layer;
			//TODO: return layers
		}

		public IEnumerable<IBiome> Biomes() 
		{ 
			foreach (BiomeLayer layer in layers)
				yield return layer;
		}

		public IEnumerable<(IInlet<MatrixWorld>,Graph,TileData)> InletsSubgraphsDatas(TileData data) 
		/// Iterates in active generated layers (ones that have subgraph assigned and data generated) and returns their subgraph and data
		{ 
			foreach (BiomeLayer layer in layers)
			{
				Graph subGraph = layer.SubGraph;
				if (subGraph == null) continue;

				TileData subData = layer.SubData(data);
				if (subData == null) continue; //in case biome has not been generated ever yet, but dragging field 

				yield return (layer, subGraph, subData);
			}
				
		}

		public float Complexity
		{get{
			float sum = 0;
			foreach (BiomeLayer layer in layers)
				if (layer.graph != null)
					sum += layer.graph.GetGenerateComplexity();
			return sum;
		}}

		public float Progress (TileData data)
		{
			float sum = 0;
			foreach (BiomeLayer layer in layers)
			{
				if (layer.graph == null) continue;

				TileData subData = layer.SubData(data);
				if (subData == null) continue;

				sum += layer.graph.GetGenerateProgress(subData);
			}
			return sum;
		}


		public void Prepare (TileData data, Terrain terrain)
		{
			foreach (BiomeLayer layer in layers)
			{
				if (layer.graph == null) continue;

				TileData subData = data.CreateLoadSubData(layer.Id);

				layer.graph.Prepare(subData, terrain);
			}
		}


		public override void Generate (TileData data, StopToken stop) 
		{
			#if MM_DEBUG
			Log.Add("Biome start (draft:" + data.isDraft + " gen:" + id);
			#endif

			if (layers.Length == 0) return;
			BiomeLayer[] layersCopy = layers.Copy(); //layers count can be changed during generate

			//reading/copying products
			MatrixWorld[] dstMatrices = new MatrixWorld[layersCopy.Length];
			float[] opacities = new float[layersCopy.Length];

			if (stop!=null && stop.stop) return;
			for (int i=0; i<layersCopy.Length; i++)
			{
				if (stop!=null && stop.stop) return;

				MatrixWorld srcMatrix = data.ReadInletProduct(layersCopy[i]);
				if (srcMatrix != null) dstMatrices[i] = new MatrixWorld(srcMatrix);
				else dstMatrices[i] = new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize);

				opacities[i] = layersCopy[i].Opacity;
			}

			//normalizing
			if (stop!=null && stop.stop) return;
			dstMatrices.FillNulls(() => new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize));
			dstMatrices[0].Fill(1);
			Matrix.BlendLayers(dstMatrices, opacities);

			//saving products
			if (stop!=null && stop.stop) return;
			for (int i=0; i<layersCopy.Length; i++)
				data.StoreProduct(layersCopy[i], dstMatrices[i]);

			//generating biomes
			for (int i=0; i<layersCopy.Length; i++)
			{
				if (stop!=null && stop.stop) return;

				BiomeLayer layer = layersCopy[i];

				MatrixWorld mask;
				if (data.biomeMask == null)
					mask = dstMatrices[i]; //no need to copy for first-level biome
				else
				{
					mask = new MatrixWorld(dstMatrices[i]);
					mask.Multiply(data.biomeMask);
				}

				Graph subGraph = layer.SubGraph;
				if (subGraph == null) continue;

				//TileData subData = data.GetSubData(layer.Id);
				//if (subData == null) subData = data.CreateSubData(layer.Id, mask);
				//subData.mask = mask;
				//SubData could be created at prepare stage (Whittaker has prepare), but I have not tested re-assigning existing data yet
				TileData subData = data.CreateLoadSubData(layer.Id, mask);

				layer.graph.Generate(subData, stop:stop, ovd:layer.graph.defaults);
			}

			#if MM_DEBUG
			Log.Add("Biome generated (draft:" + data.isDraft + " gen:" + id);
			#endif
		}


		public void OnClearing (Graph graph, TileData data, ref bool isReady, bool totalRebuild=false) 
		/// Will be called at least once when ClearChanged graph (no matter ready or not)
		/// Returns true if changed
		/// Inlets are already cleared to this moment, but not this node itself
		/// Iterating in sub-graph made with this
		{
			// What should be cleared and when:
			// - On this graph modification (inlet change):			this node (done by default), all subgraph outputs (for biomes)
			// - Subgraph modification (any subgraph node change):	this node, all subgraph relevants
			// - Exposed values change (this node change):			this node (done by default), exp related subgraph node, subgraph relevants

			//clarifying whether this generator changed directly or recursively
			bool versionChanged = data.VersionChanged(this);
			bool thisChanged = !isReady && versionChanged;
			bool inletChanged = !isReady && !versionChanged;

			//iterating subgraphs/subdatas
			foreach (BiomeLayer layer in this.layers)
			{
				Graph subGraph = layer.SubGraph;
				if (subGraph == null) continue;

				TileData subData = layer.SubData(data);
				if (subData == null) continue; //in case biome has not been generated ever yet, but dragging field 

				//resetting exposed related nodes on this node change
				if (thisChanged)
				{
					//TODO: reset only changed generators
					foreach (IUnit expUnit in subGraph.exposed.AllUnits(subGraph))
						subData.ClearReady((Generator)expUnit);
				}

				//resetting outputs/relevants on inlet or this changed
				if (inletChanged || thisChanged)
				{
					foreach (Generator relGen in subGraph.RelevantGenerators(data.isDraft)) 
						subData.ClearReady(relGen);
				}

				//iterating in sub-graph after
				subGraph.ClearChanged(subData);

				//at the end clearing this if any subgraph relevant changed
				if (isReady)
				{
					foreach (Generator relGen in subGraph.RelevantGenerators(data.isDraft)) 
						if (!subData.IsReady(relGen))
							isReady = false;
				}
			}
		}
	}
}
