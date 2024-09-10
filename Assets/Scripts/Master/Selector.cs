using UnityEngine;

namespace masterland.Interact
{
    using m = Master;
    using Building;
    using System.Collections.Generic;
    using System.Linq;

    public enum InteractType
    {
        Mint_Stone = 1,
        Mint_Wood = 2,
    }

    public enum SelectorState
    {
        Mineral = 1,
        Building = 2,
    }

    public class Selector : MonoBehaviour
    {
        [HideInInspector]
        public SelectedObject CurrentSelectedObject;
        public LayerMask layerMask;
        public float checkInterval = 0.1f;
        private float timer = 0f;
        public SelectorState SelectorState;

        [Header("HOUSE BUILDING")]
        [SerializeField] private LayerMask _terrainLayer;
        [SerializeField] LayerMask _ignoreLayer;
        [SerializeField] LayerMask _builableLayer;

        [SerializeField] private List<ElementConnections> _elementConnectionsList;
        [SerializeField] private GameObject _houseHolder;


        [SerializeField] private Vector3 _selectedHousePosition;
        private bool IsSetupHouse = false;

        void Start()
        {
            SelectorState = SelectorState.Mineral;
        }

        void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;

                if (SelectorState == SelectorState.Mineral)
                    SelectMineral();


            }

            if (SelectorState == SelectorState.Building)
            {
                if (IsSetupHouse)
                    MoveBuilding();
            }
        }

        public void SelectMineral()
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f, layerMask))
            {
                SelectedObject newOutlineEffect = hit.collider.GetComponent<SelectedObject>();

                if (newOutlineEffect != null)
                {
                    if (CurrentSelectedObject != newOutlineEffect)
                    {
                        if (CurrentSelectedObject != null)
                        {
                            CurrentSelectedObject.DisableOutline();
                        }

                        newOutlineEffect.EnableOutline();
                        CurrentSelectedObject = newOutlineEffect;
                        UIController.Instance.ShowInteractTutorial(CurrentSelectedObject.InteractType.ConvertEnumToStringWithSpace());
                    }
                }
                else
                {
                    if (CurrentSelectedObject != null)
                    {
                        CurrentSelectedObject.DisableOutline();
                        CurrentSelectedObject = null;
                        UIController.Instance.HideInteractTutorial();
                    }
                }
            }
            else
            {
                if (CurrentSelectedObject != null)
                {
                    CurrentSelectedObject.DisableOutline();
                    UIController.Instance.HideInteractTutorial();
                    CurrentSelectedObject = null;
                }
            }
        }

        public void SetupBuilding(List<ElementConnections> elementConnectionsList, GameObject houseHolder)
        {
            m.Master.Local.Input.PlayBuild = true;
            _elementConnectionsList = elementConnectionsList;
            _houseHolder = houseHolder;
            _elementConnectionsList.ForEach(item =>
            {
                item.Connections = item.Connections.Where(i => i != null).ToList();
            });

            _houseHolder.transform.position = transform.position + transform.forward * 2f;

            foreach (var element in _elementConnectionsList)
            {
                element.Element.SetActive(false);
                element.Preview.SetActive(true);
                if (element.BuildingComponentConfig.Type == BuildingComponentType.Floor)
                {
                    var foundation = element.Preview.transform.GetChild(0).GetChild(0).GetComponent<Foundation>();
                    Debug.Log("Found" + foundation.name);
                    if (foundation)
                        foundation.gameObject.SetActive(true);
                }

                if (element.BuildingComponentConfig.Type == BuildingComponentType.Door)
                {
                    if (IsExteriorDoor(element.Preview, out bool isStairForward)&& element.Preview.transform.localPosition.y <= 0.2f)
                    {
                        Stair stair = element.Preview.transform.GetChild(0).GetChild(0).GetComponent<Stair>();
                        if (stair)
                        {
                            stair.gameObject.SetActive(true);
                            stair.transform.localEulerAngles = new Vector3(0, isStairForward ? -90f : 90f, 0);
                        }
                    }

                }
            }

            IsSetupHouse = true;
            SelectorState = SelectorState.Building;
        }

        public void MoveBuilding()
        {
            Vector3 mousePosition = Input.mousePosition;

            Ray rayFromScreen = Camera.main.ScreenPointToRay(mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(rayFromScreen, 100f);

            foreach (RaycastHit hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & _terrainLayer) != 0)
                {
                    Vector3 rayStartPosition = new Vector3(hit.point.x, hit.point.y + 10f, hit.point.z);
                    Ray overheadRay = new Ray(rayStartPosition, Vector3.down);

                    if (Physics.Raycast(overheadRay, out RaycastHit terrainHit, 50f, _terrainLayer))
                    {
                        _selectedHousePosition =  Vector3.Lerp(_selectedHousePosition,terrainHit.point + Vector3.up * 0.2f, 5f* Time.fixedDeltaTime);
                        _houseHolder.transform.position = _selectedHousePosition;
                        break;
                    }
                }
            }

            if (m.Master.Local.Input.HouseRotationLeft)
            {
                _houseHolder.transform.eulerAngles = new Vector3(0, _houseHolder.transform.eulerAngles.y + 200f * Time.fixedDeltaTime, 0);
            }
            if (m.Master.Local.Input.HouseRotationRight)
            {
                _houseHolder.transform.eulerAngles = new Vector3(0, _houseHolder.transform.eulerAngles.y - 200f * Time.fixedDeltaTime, 0);
            }
        }

        public void RequireBuild()
        {
            m.Master.Local.Input.PlayBuild = false;
            SelectorState = SelectorState.Mineral;

            IsSetupHouse = false;
            
            // foreach (var element in _elementConnectionsList)
            // {
            //     element.Element.SetActive(true);
            //     element.Preview.SetActive(false);
            //     if (element.BuildingComponentConfig.Type == BuildingComponentType.Floor)
            //     {
            //         var foundation = element.Element.transform.GetChild(0).GetChild(0).GetComponent<Foundation>();
            //         if (foundation)
            //             foundation.gameObject.SetActive(true);
            //     }

            //     if (element.BuildingComponentConfig.Type == BuildingComponentType.Door)
            //     {

            //         if (IsExteriorDoor(element.Element, out bool isStairForward) && element.Element.transform.localPosition.y <= 0.2f)
            //         {
            //             Stair stair = element.Element.transform.GetChild(0).GetChild(0).GetComponent<Stair>();
            //             if (stair)
            //             {
            //                 stair.gameObject.SetActive(true);
            //                 stair.transform.localEulerAngles = new Vector3(0, isStairForward ? -90f : 90f, 0);
            //             }

            //         }

            //     }
            // }
            
            if(BuildingManager.Instance) 
            {
                Debug.Log(_selectedHousePosition);
                string jsonData = BuildingManager.Instance.ConvertBuildingToJsonData(_selectedHousePosition, _houseHolder.transform.eulerAngles,_elementConnectionsList);
                BuildingManager.Instance.RequireBuildHouse(jsonData);
            }

            Destroy(_houseHolder);
            _elementConnectionsList.Clear();

        }

        public bool IsExteriorDoor(GameObject door, out bool isStairForward)
        {
            Vector3 doorPosition = door.transform.position + door.transform.right * -1.25f + door.transform.up * 1.25f;
            Vector3 forwardDirection = door.transform.forward;
            float maxDistance = 7f;

            RaycastHit hit;
            bool forwardHit = Physics.Raycast(doorPosition, forwardDirection, out hit, maxDistance, _builableLayer);
            bool forwardClear = !forwardHit || (hit.collider != null && !hit.collider.gameObject.transform.IsChildOf(_houseHolder.transform));

            bool backwardHit = Physics.Raycast(doorPosition, -forwardDirection, out hit, maxDistance, _builableLayer);
            bool backwardClear = !backwardHit || (hit.collider != null && !hit.collider.gameObject.transform.IsChildOf(_houseHolder.transform));

            isStairForward = Vector3.Dot(door.transform.forward, -door.transform.localPosition.normalized) <= 0;

            return forwardClear || backwardClear;
        }


    }
}
