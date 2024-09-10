using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace masterland.Building
{
    using M = Master;
    using Manager;
    using masterland.UI;
    using UnityEngine.SceneManagement;

    public class UserInterface : Singleton<UserInterface>
    {
        [Header("GENERAL")]
        [SerializeField] private GameObject _backgroundPanel;
        [SerializeField] private GameObject _circleMenuElementPrefab;
        [SerializeField] private bool _useGradient;

        [Header("BUTTONS")]
        [SerializeField] private Button _backBtn;
        [SerializeField] private Button _confirmBtn;
        [SerializeField] private Button _buildingsBtn;

        [Header("COLORS")]
        [SerializeField] private Color _normalButtonColor;
        [SerializeField] private Color _highlightedButtonColor;
        [SerializeField] private Gradient _highlightedButtonGradient = new Gradient();

   

        [Header("INFORMAL CENTER")]
        [SerializeField] private Image _informalCenterBackground;
        [SerializeField] private Text _itemName;
        [SerializeField] private Text _itemDescription;
        [SerializeField] private Image _itemIcon;

        [Header("INFORMAL MINERALS")]
        [SerializeField] private TextMeshProUGUI _woodText;
        [SerializeField] private TextMeshProUGUI _stoneText;


        private int _currentMenuItemIndex;
        private int _previousMenuItemIndex;
        private float _calculatedMenuIndex;
        private float _currentSelectionAngle;
        private Vector3 _currentMousePosition;
        private List<CircularMenuElement> _menuElements = new List<CircularMenuElement>();

        public bool Active { get { return _backgroundPanel.activeSelf; } }


        public List<CircularMenuElement> MenuElements
        {
            get { return _menuElements; }
            set { _menuElements = value; }
        }

        private void Start() 
        {
            _backBtn.onClick.AddListener(() => {
                if(!SceneController.Instance) return;
                SceneController.Instance.UnloadSceneAsync(SceneName.Scene_Building);
                UIManager.Instance.ToggleView(ViewName.Gameplay);
            });

            _buildingsBtn.onClick.AddListener(() => {
                BuildingSystem.Instance.ShowBuildingUI();
            });

            _confirmBtn.onClick.AddListener(SetupBuildingToPlace);
        }


        public void Initialize()
        {

            float rotationalIncrementalValue = 360f / MenuElements.Count;
            float currentRotationValue = 0;
            float fillPercentageValue = 1f / MenuElements.Count;

            for (int i = 0; i < MenuElements.Count; i++)
            {
                GameObject menuElementGameObject = Instantiate(_circleMenuElementPrefab);
                menuElementGameObject.name = i + ": " + currentRotationValue;
                menuElementGameObject.transform.SetParent(_backgroundPanel.transform);

                MenuButton menuButton = menuElementGameObject.GetComponent<MenuButton>();

                menuButton.Recttransform.localScale = Vector3.one;
                menuButton.Recttransform.localPosition = Vector3.zero;
                menuButton.Recttransform.rotation = Quaternion.Euler(0f, 0f, currentRotationValue);
                currentRotationValue += rotationalIncrementalValue;

                menuButton.Background.fillAmount = fillPercentageValue + 0.001f;
                MenuElements[i].ButtonBackground = menuButton.Background;

                menuButton.Icon.sprite = MenuElements[i].ButtonIcon;
                menuButton.IconRecttransform.rotation = Quaternion.identity;
            }

            _backgroundPanel.SetActive(false);
        }

        private void Update()
        {
            if (!Active)
            {
                return;
            }

            GetCurrentMenuElement();
            if (Input.GetMouseButton(0))
            {
                Select();
            }
        }

        public void UpdateMinerals(int wood, int stone) 
        {
            _woodText.text = wood.ToString();
            _stoneText.text = stone.ToString();
        }


        private void GetCurrentMenuElement()
        {
            float rotationalIncrementalValue = 360f / MenuElements.Count;
            _currentMousePosition = new Vector2(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2);
            _currentSelectionAngle = 90 + rotationalIncrementalValue + Mathf.Atan2(_currentMousePosition.y, _currentMousePosition.x) * Mathf.Rad2Deg;
            _currentSelectionAngle = (_currentSelectionAngle + 360f) % 360f;

            _currentMenuItemIndex = (int)(_currentSelectionAngle / rotationalIncrementalValue);

            if (_currentMenuItemIndex != _previousMenuItemIndex)
            {
                MenuElements[_previousMenuItemIndex].ButtonBackground.color = _normalButtonColor;

                _previousMenuItemIndex = _currentMenuItemIndex;
                MenuElements[_currentMenuItemIndex].ButtonBackground.color = _useGradient ? _highlightedButtonGradient.Evaluate(1f / MenuElements.Count
                * _currentMenuItemIndex) : _highlightedButtonColor;
                _informalCenterBackground.color = _useGradient ? _highlightedButtonGradient.Evaluate(1f / MenuElements.Count * _currentMenuItemIndex) :
                _highlightedButtonColor;
                RefreshInformalCenter();
            }
        }

        private void RefreshInformalCenter()
        {
            _itemName.text = MenuElements[_currentMenuItemIndex].Name;
            _itemIcon.sprite = MenuElements[_currentMenuItemIndex].ButtonIcon;
            _itemDescription.text = MenuElements[_currentMenuItemIndex].Description;
            _itemDescription.text = $"Wood: {BuildingSystem.Instance.BuildingComponents[_currentMenuItemIndex].Wood} \n STONE: {BuildingSystem.Instance.BuildingComponents[_currentMenuItemIndex].Stone}";
        }

        private void Select()
        {
            BuildingSystem.Instance.SwitchToIndex(_currentMenuItemIndex);
            Deactivate();
        }

        public void Activate()
        {
            if (Active)
            {
                return;
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _backgroundPanel.SetActive(true);
            RefreshInformalCenter();
        }

        public void Deactivate()
        {
            _backgroundPanel.SetActive(false);
        }

        public void SetupBuildingToPlace() 
        {
            if(M.Master.Local)
            {
                BuildingSystem.Instance.MoveHouseToCenter();
                M.Master.Local.Selector.SetupBuilding(BuildingSystem.Instance.ElementConnectionsList,BuildingSystem.Instance.HouseHolder);
                Scene targetScene = SceneManager.GetSceneByName(SceneName.Scene_Boostrap); 
                
                SceneManager.MoveGameObjectToScene(BuildingSystem.Instance.HouseHolder, targetScene);

                SceneController.Instance.UnloadSceneAsync(SceneName.Scene_Building);
                UIManager.Instance.ToggleView(ViewName.Gameplay);
            }
        }

    }
}