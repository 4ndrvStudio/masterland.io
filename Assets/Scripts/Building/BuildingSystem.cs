using UnityEngine;
using System.Collections.Generic;
using masterland.UI;

namespace masterland.Building
{
    [System.Serializable]
    public class ElementConnections
    {
        public GameObject Element;
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
        [SerializeField] private GameObject _houseHolder;
        [SerializeField] private GameObject _floorOb;

        [Header("BUILDING COMPONENT CATALOGUE")]
        [SerializeField] private List<BuildingComponent> _buildingComponents = new List<BuildingComponent>();

        [Header("SNAPPING")]
        [SerializeField] private float _gridSize = 2;
        [SerializeField] private float _gridOffset = 0.5f;

        [SerializeField] private GameObject _currentPreviewGameObject;
        [SerializeField] private BuildingComponent _currentlyPreviewedComponent;
        [SerializeField] private Vector3 _currentPosition;
        [SerializeField] private RaycastHit _raycastHit;


        [SerializeField] private Preview _currentPreview;
        [SerializeField] private GameObject _currentElement;
        [SerializeField] private List<ElementConnections> _elementConnectionsList = new();
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

        private void Start()
        {
            for (int i = 0; i < _buildingComponents.Count; i++)
            {
                UserInterface.Instance.MenuElements.Add(_buildingComponents[i].MenuElement);
            }
            UserInterface.Instance.Initialize();
        }

        private void Update()
        {
            if (!EditMode)
            {
                if (Input.GetKeyDown(KeyCode.B))
                {
                    if (UserInterface.Instance.Active)
                    {
                        UserInterface.Instance.Deactivate();
                    }
                    else
                    {
                        UserInterface.Instance.Activate();
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0))
                    {
                        float mouseX = Input.GetAxis("Mouse X");

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
        }

        private void ShowDelete()
        {
            if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out _raycastHit, float.MaxValue, _buildableLayers,
            QueryTriggerInteraction.Ignore))
            {
                foreach (Transform element in _houseHolder.transform)
                {
                    var elementScript = element.GetComponentInChildren<Element>();
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
                    Debug.Log(isConnectedOther);
                }
            }
            else
            {
                ResetDelete();
            }
        }

        public void ResetDelete()
        {
            foreach (Transform element in _houseHolder.transform)
            {
                var elementScript = element.GetComponentInChildren<Element>();
                elementScript.CanDelete(false);
            }
            _currentElement = null;
        }

        public void Delete()
        {
            if (Input.GetMouseButtonDown(1) && _currentElement != null)
            {
                var el = _elementConnectionsList.Find(item => item.Element == _currentElement);
                _elementConnectionsList.Remove(el);
                Destroy(_currentElement);

                foreach (var element in _elementConnectionsList)
                {
                    element.Connections.RemoveAll(connection => connection == null);
                }

                _elementConnectionsList.RemoveAll(elementConnections =>
                {
                    if (!IsConnected(elementConnections.Element, _floorOb))
                    {
                        Destroy(elementConnections.Element);
                        return true;
                    }
                    return false;
                });

                foreach (var element in _elementConnectionsList)
                {
                    element.Connections.RemoveAll(connection => connection == null);
                }
            }
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
            GameObject instanctiated = Instantiate(_currentlyPreviewedComponent.BuildingElementPrefab, _currentPosition, _currentPreviewGameObject.transform.rotation, _houseHolder.transform);
            instanctiated.GetComponentInChildren<Rigidbody>().isKinematic = true;
            if (_currentPreviewGameObject != null)
            {
                GameObject temp = _currentPreviewGameObject;
                Destroy(temp);
            }
            ObjectToSnap = null;
            Snapping = false;

            ElementConnections elementConnections = new ElementConnections
            {
                Element = instanctiated,
                Connections = GetConnectedObject(instanctiated.transform.GetChild(0)),
            };

            _elementConnectionsList.Add(elementConnections);
            //update connecition
            _elementConnectionsList.ForEach(item =>
            {
                int index = elementConnections.Connections.FindIndex(i => i == item.Element);
                if (index != -1)
                {
                    item.Connections = GetConnectedObject(item.Element.transform.GetChild(0));
                }
            });

            foreach (var element in _elementConnectionsList)
            {
                    element.Connections.RemoveAll(connection => connection == null);
            }

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

            ElementConnections currentConnections = _elementConnectionsList.Find(ec => ec.Element == current);

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

     


    }
}