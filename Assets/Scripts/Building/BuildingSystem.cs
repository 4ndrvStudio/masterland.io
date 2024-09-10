using UnityEngine;
using System.Collections.Generic;
using masterland.UI;

namespace masterland.Building
{
    [System.Serializable]
    public class ElementConnections
    {
        public GameObject Element;
        public BuildingComponent BuildingComponentConfig;
        public GameObject Preview;
        public List<GameObject> Connections;
    }

    public class BuildingSystem : Singleton<BuildingSystem>
    {
        [Header("GENERAL")]
        [SerializeField] private bool _allowSnapping;
        [SerializeField] private float _snappingDistance;
        [SerializeField] private LayerMask _buildableLayers;
        [SerializeField] private LayerMask _floorLayerMask;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _cameraHolder;
        public GameObject HouseHolder;
        [SerializeField] private GameObject _floorOb;

        [Header("BUILDING COMPONENT CATALOGUE")]
        [SerializeField] private List<BuildingComponent> _buildingComponents = new List<BuildingComponent>();
        public List<BuildingComponent> BuildingComponents => _buildingComponents;

        [Header("SNAPPING")]
        [SerializeField] private float _gridSize = 2;
        [SerializeField] private float _gridOffset = 0.5f;

        [SerializeField] private GameObject _currentPreviewGameObject;
        [SerializeField] private BuildingComponent _currentlyPreviewedComponent;
        [SerializeField] private Vector3 _currentPosition;
        [SerializeField] private RaycastHit _raycastHit;


        [SerializeField] private Preview _currentPreview;
        [SerializeField] private GameObject _currentElement;
        public List<ElementConnections> ElementConnectionsList = new();
        [SerializeField] private float _connectedThreshold = 0.5f;
        [SerializeField] private float _overlapBoxThreshHold = 1f;


        [HideInInspector] public bool Snapping;
        [HideInInspector] public GameObject ObjectToSnap;
        [HideInInspector] public Vector3 SnappingOffset;
        [HideInInspector] public float HightOffset;

        [Header("Rotate")]
        [SerializeField] private UITouchField _uiTouchField;
        [Range(1f, 20f)]
        [SerializeField] private float _rotationSpeed;

        [Header("MODE")]
        public bool EditMode;
        public bool PreviewMode;

        [Header("Prices")]
        public int Wood;
        public int Stone;

        private void Start()
        {
            if(BuildingManager.Instance)
                _buildingComponents = BuildingManager.Instance.BuildingComponents;

            for (int i = 0; i < _buildingComponents.Count; i++)
            {
                UserInterface.Instance.MenuElements.Add(_buildingComponents[i].MenuElement);
            }

            UserInterface.Instance.Initialize();
        }

        public void CheckMaterials()
        {
            Wood = 0;
            Stone = 0;
            ElementConnectionsList.ForEach(item =>
            {
                var element = item.Element.GetComponentInChildren<Element>();
                Wood += element.BuildingComponent.Wood;
                Stone += element.BuildingComponent.Stone;
            });

            UserInterface.Instance.UpdateMinerals(Wood, Stone);
        }

        private void Update()
        {
            if (!EditMode)
            {
                if (Input.GetKeyDown(KeyCode.B))
                {
                   ShowBuildingUI();
                }
                
                if(!UserInterface.Instance.Active)
                {
                    if (Input.GetMouseButton(0))
                    {
                        _cameraHolder.eulerAngles += new Vector3(0, _uiTouchField.TouchDist.x * _rotationSpeed * Time.deltaTime, 0);
                    }
                }
            }

            EditMode = Input.GetKey(KeyCode.C) && !PreviewMode;

            if (PreviewMode && !UserInterface.Instance.Active)
            {
                ShowPreview();
                if (Input.GetMouseButtonDown(1))
                {
                    if (_currentPreview != null && _currentPreview.Buildable)
                    {
                        Place();
                    }
                }

                if (Input.mouseScrollDelta.y != 0)
                {
                    Rotate();
                }
            }

            if (EditMode && !UserInterface.Instance.Active)
            {
                ShowDelete();
                Delete();
            }
            else
            {
                ResetDelete();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                MoveHouseToCenter();
            }
        }

        public void ShowBuildingUI() {
            if (UserInterface.Instance.Active)
            {
                UserInterface.Instance.Deactivate();
            }
            else
            {
                UserInterface.Instance.Activate();
            }
        }

        private void ShowDelete()
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _raycastHit, float.MaxValue, _buildableLayers,
            QueryTriggerInteraction.Ignore))
            {
                foreach (Transform element in HouseHolder.transform)
                {
                    var elementScript = element.GetComponentInChildren<Element>();
                    if(elementScript == null) continue;
                    
                    if (element.GetChild(0) == _raycastHit.transform)
                    {
                        elementScript.CanDelete(true);
                        _currentElement = element.gameObject;
                    }
                    else
                        elementScript.CanDelete(false);
                }

                if (_raycastHit.transform.gameObject != _floorOb)
                {
                    bool isConnectedOther = IsConnected(_raycastHit.transform.parent.gameObject, _floorOb);
                }
            }
            else
            {
                ResetDelete();
            }
        }

        public void ResetDelete()
        {
            foreach (Transform element in HouseHolder.transform)
            {
                var elementScript = element.GetComponentInChildren<Element>();
                if(elementScript == null) continue;

                elementScript.CanDelete(false);
            }
            _currentElement = null;
        }

        public void Delete()
        {
            if (Input.GetMouseButtonDown(1) && _currentElement != null)
            {
                var el = ElementConnectionsList.Find(item => item.Element == _currentElement);
                Destroy(el.Preview);
                ElementConnectionsList.Remove(el);
                Destroy(_currentElement);
            
                foreach (var element in ElementConnectionsList)
                {
                    element.Connections.RemoveAll(connection => connection == null);
                }

                ElementConnectionsList.RemoveAll(elementConnections =>
                {
                    if (!IsConnected(elementConnections.Element, _floorOb) && elementConnections.Element.transform.localPosition.y > 0.2f)
                    {
                        Destroy(elementConnections.Element);
                        Destroy(elementConnections.Preview);
                        return true;
                    }
                    return false;
                });

                foreach (var element in ElementConnectionsList)
                {
                    element.Connections.RemoveAll(connection => connection == null);
                }
            }
            CheckMaterials();
        }



        private void InstantiatePreview()
        {
            if (_currentPreviewGameObject != null)
            {
                GameObject temp = _currentPreviewGameObject;
                Destroy(temp);
            }

            _currentPreviewGameObject = Instantiate(_currentlyPreviewedComponent.PreviewPrefab);
            _currentPreview = _currentPreviewGameObject.GetComponentInChildren<Preview>();
            PreviewMode = true;
        }

        public void SwitchToIndex(int _index)
        {
            _currentlyPreviewedComponent = _buildingComponents[_index];
            InstantiatePreview();
        }

        private void ShowPreview()
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _raycastHit, float.MaxValue, _buildableLayers,
            QueryTriggerInteraction.Ignore))
            {
                _currentPosition = _raycastHit.point;
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            if (!_allowSnapping || !Snapping || ObjectToSnap == null)
            {
                _currentPosition -= Vector3.one * _gridOffset;
                _currentPosition /= _gridSize;
                _currentPosition = new Vector3(Mathf.Round(_currentPosition.x), Mathf.Round(_currentPosition.y), Mathf.Round(_currentPosition.z));
                _currentPosition *= _gridSize;
                _currentPosition += Vector3.one * _gridOffset;
            }
            else
            {
                MeshCollider objectCollider = ObjectToSnap.GetComponent<MeshCollider>();
                Vector3[] position = new Vector3[4];
                position[0] = new Vector3((objectCollider.bounds.center.x + objectCollider.bounds.size.x / 2f),
                                objectCollider.bounds.center.y, objectCollider.bounds.center.z) + SnappingOffset;

                position[1] = new Vector3((objectCollider.bounds.center.x - objectCollider.bounds.size.x / 2f),
                                objectCollider.bounds.center.y, objectCollider.bounds.center.z) + SnappingOffset;

                position[2] = new Vector3(objectCollider.bounds.center.x, objectCollider.bounds.center.y,
                                (objectCollider.bounds.center.z + objectCollider.bounds.size.z / 2f)) + SnappingOffset;

                position[3] = new Vector3(objectCollider.bounds.center.x, objectCollider.bounds.center.y,
                                (objectCollider.bounds.center.z - objectCollider.bounds.size.z / 2f)) + SnappingOffset;

                Vector3 updatedPosition = _currentPosition;
                float minDistance = float.MaxValue;
                int index = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (Vector3.Distance(_currentPosition, position[i]) < minDistance)
                    {
                        minDistance = Vector3.Distance(_currentPosition, position[i]);
                        updatedPosition = position[i];
                        index = i;
                    }
                }

                switch (index)
                {
                    case 0:
                        _currentPreviewGameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        break;

                    case 1:
                        _currentPreviewGameObject.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
                        break;

                    case 2:
                        _currentPreviewGameObject.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
                        break;

                    case 3:
                        _currentPreviewGameObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                        break;
                }

                updatedPosition += new Vector3(0f, HightOffset, 0f);
                _currentPosition = (Vector3.Distance(_currentPosition, updatedPosition) <= _snappingDistance) ? updatedPosition : _currentPosition;
            }
            _currentPreviewGameObject.transform.position = _currentPosition;
        }

        private void Place()
        {
            PreviewMode = false;
            
            GameObject elementOb = Instantiate(_currentlyPreviewedComponent.BuildingElementPrefab, _currentPosition, _currentPreviewGameObject.transform.rotation, HouseHolder.transform);
            elementOb.GetComponentInChildren<Rigidbody>().isKinematic = true;
            GameObject previewOb =  Instantiate(_currentlyPreviewedComponent.PreviewPrefab, _currentPosition, _currentPreviewGameObject.transform.rotation, HouseHolder.transform);
            previewOb.SetActive(false);
            
            ObjectToSnap = null;
            Snapping = false;
            ElementConnections elementConnections = new ElementConnections
            {
                Element = elementOb,
                Preview =  previewOb,
                BuildingComponentConfig = _currentlyPreviewedComponent,
                Connections = GetConnectedObject(elementOb.transform.GetChild(0)),
            };


            if (_currentPreviewGameObject != null)
            {
                GameObject temp = _currentPreviewGameObject;
                Destroy(temp);
            }

            ElementConnectionsList.Add(elementConnections);
            //update connecition
            ElementConnectionsList.ForEach(item =>
            {
                int index = elementConnections.Connections.FindIndex(i => i == item.Element);
                if (index != -1)
                {
                    item.Connections = GetConnectedObject(item.Element.transform.GetChild(0));
                }
            });

            foreach (var element in ElementConnectionsList)
            {
                element.Connections.RemoveAll(connection => connection == null);
            }

            CheckMaterials();

        }

        private void Rotate()
        {
            if (_currentPreviewGameObject == null)
            {
                return;
            }

            _currentPreviewGameObject.transform.Rotate(Vector3.up, 90f * Input.mouseScrollDelta.y);
        }


        private List<GameObject> GetConnectedObject(Transform obj)
        {
            List<GameObject> connectedList = new();

            MeshCollider objCollider = obj.GetComponent<MeshCollider>();
            Vector3 objCenter = objCollider.bounds.center;
            Vector3 objSize = objCollider.bounds.size;

            Vector3 adjustedSize = objSize * _overlapBoxThreshHold;
            Collider[] nearbyColliders = Physics.OverlapBox(objCenter, adjustedSize, Quaternion.identity);

            foreach (Collider col in nearbyColliders)
            {
                Transform otherTransform = col.transform;
                if (otherTransform == obj) continue;

                MeshCollider otherCollider = col.GetComponent<MeshCollider>();
                if (otherCollider == null) continue;

                if (Vector3.Distance(objCenter, otherCollider.bounds.center) > _connectedThreshold)
                    continue;

                Vector3[] otherVertices = otherCollider.sharedMesh.vertices;

                foreach (Vector3 vertex in otherVertices)
                {
                    Vector3 worldVertex = otherTransform.TransformPoint(vertex);

                    foreach (Vector3 objVertex in objCollider.sharedMesh.vertices)
                    {
                        Vector3 worldObjVertex = obj.TransformPoint(objVertex);
                        if (Vector3.Distance(worldObjVertex, worldVertex) <= _connectedThreshold)
                        {
                            GameObject otherObject = col.gameObject;
                            if (otherObject != _floorOb && !connectedList.Contains(otherObject.transform.parent.gameObject))
                            {
                                connectedList.Add(otherObject.transform.parent.gameObject);
                            }
                        }
                    }
                }
            }

            if (Physics.OverlapBox(objCenter, adjustedSize, Quaternion.identity, _floorLayerMask).Length > 0)
            {
                if (!connectedList.Contains(_floorOb))
                    connectedList.Add(_floorOb);
            }

            return connectedList;
        }

        public bool IsConnected(GameObject objectStart, GameObject objectTarget)
        {
            HashSet<GameObject> visited = new HashSet<GameObject>();
            return DFS(objectStart, objectTarget, visited);
        }

        private bool DFS(GameObject current, GameObject target, HashSet<GameObject> visited)
        {
            if (current == target)
            {
                return true;
            }

            visited.Add(current);

            ElementConnections currentConnections = ElementConnectionsList.Find(ec => ec.Element == current);

            if (currentConnections != null)
            {
                foreach (GameObject neighbor in currentConnections.Connections)
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (DFS(neighbor, target, visited))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public void MoveHouseToCenter()
        {
            Vector3 center = CalculateCenter();
            Vector3 offset = Vector3.one * 1.25f;
            offset.y =0;
            foreach (ElementConnections elementConnection in ElementConnectionsList)
            {
                elementConnection.Element.transform.position -= center;
                elementConnection.Element.transform.position += offset;
            }

        }

        Vector3 CalculateCenter()
        {
            float sumX = 0f;
            float sumZ = 0f;

            foreach (ElementConnections obj in ElementConnectionsList)
            {
                sumX += obj.Element.transform.position.x;
                sumZ +=  obj.Element.transform.position.z;
            }

            float averageX = sumX / ElementConnectionsList.Count;
            float averageZ = sumZ / ElementConnectionsList.Count;

            return new Vector3(averageX, 0f, averageZ);
        }

    }
}