#region Includes
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#endregion

namespace TS.DoubleSlider
{
    [RequireComponent(typeof(RectTransform))]
    public class DoubleSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private SingleSlider _sliderMin;
        [SerializeField] private SingleSlider _sliderMax;

        [SerializeField] private SingleSlider _sliderMinHoc;
        [SerializeField] private SingleSlider _sliderMaxHoc;


        [SerializeField] public Slider _currePostion;



         [SerializeField] private Slider _currePostionHoc;
        

        

        [SerializeField] private Text warningText;

     [SerializeField] private RectTransform _fillArea;
     [SerializeField] private RectTransform _fillAreahoc;

     [SerializeField] private RectTransform _fillAreahocleft;
        [SerializeField] private RectTransform oldROMArea;

        [SerializeField] private RectTransform oldROMAreaHoc;
        

        //[SerializeField] private RectTransform oldROMArea2;

        [Header("Configuration")]
        [SerializeField] private bool _setupOnStart;
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue;

        [SerializeField] private float _minValueHoc;
        [SerializeField] private float _maxValueHoc;
        [SerializeField] private float _minDistance;
        [SerializeField] private bool _wholeNumbers;
        [SerializeField] private float _initialMinValue;
        [SerializeField] private float _initialMaxValue;

        [Header("Events")]
        public UnityEvent<float, float> OnValueChanged;

        public float minAng, maxAng;
        public bool UpdateMinMaxvalues;

        public PromWF_Scn_Hndlr_newUI promSlider;
        //public AromWF_Scn_Hndlr_newUI aromSlider;

        public bool IsEnabled
        {
            get { return _sliderMax.IsEnabled && _sliderMin.IsEnabled; }
            set
            {
                _sliderMin.IsEnabled = value;
                _sliderMax.IsEnabled = value;
                _sliderMinHoc.IsEnabled = value;
                _sliderMaxHoc.IsEnabled = value;
            }
        }

        public float MinValue => _sliderMin.Value;
        public float MaxValue => _sliderMax.Value;

        public float MinValueHoc => _sliderMinHoc.Value;
        public float MaxValueHoc => _sliderMaxHoc.Value;

        public bool WholeNumbers
        {
            get => _wholeNumbers;
            set
            {
                _wholeNumbers = value;
                _sliderMin.WholeNumbers = _wholeNumbers;
                _sliderMax.WholeNumbers = _wholeNumbers;
            }
        }

        public bool IsDisabled { get; internal set; }

        private RectTransform _fillRect;

        private RectTransform _fillRectHoc;

        private RectTransform _fillRectHocleft;
        private RectTransform _oldROMRect;
        private RectTransform _oldROMRectHoc;
        private bool _isAromActive;
        private bool _isPromActive;
        

        //private RectTransform _oldROMRect2;

        #endregion

        private void Awake()
        {
            if (_sliderMin == null || _sliderMax == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing slider min: " + _sliderMin + ", max: " + _sliderMax);
#endif
                return;
            }

            if (_fillAreahoc == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing fill area");
#endif
                return;
            }


            // _fillRectHocleft = _fillAreahocleft.transform.GetChild(0).transform as RectTransform;
            _fillRectHoc = _fillAreahoc.transform.GetChild(0).transform as RectTransform;
            _fillRect = _fillArea.transform.GetChild(0).transform as RectTransform;
            _oldROMRect = oldROMArea.transform.GetChild(0).transform as RectTransform;
            _oldROMRectHoc = oldROMAreaHoc.transform.GetChild(0).transform as RectTransform;
            Debug.Log("Selected Mechanism :"+ AppData.selectedMechanism);
            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
          {

            _sliderMaxHoc.gameObject.SetActive(true);
           _sliderMinHoc.gameObject.SetActive(true);
 
            _maxValue = 0f;
             _minValue = -120f;
             _minValueHoc = 0f;
             _maxValueHoc = 120f;
          
           _currePostion.gameObject.SetActive(true);
            _currePostionHoc.gameObject.SetActive(true);
            _fillArea.gameObject.SetActive(true);
            _fillAreahoc.gameObject.SetActive(true);
         }
         else{

            _currePostionHoc.gameObject.SetActive(false);
            _sliderMaxHoc.gameObject.SetActive(false);
           _sliderMinHoc.gameObject.SetActive(false);
           _fillAreahoc.gameObject.SetActive(false);

         }
         if (warningText != null)
      {
        warningText.gameObject.SetActive(false);
       }
        }

        private void Start()
        {

           
            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
          {
              _currePostion.gameObject.SetActive(true);
              _currePostionHoc.gameObject.SetActive(true);
              Setup(_minValue, _maxValue, _initialMinValue, _initialMaxValue);
            }
            else{
               _currePostionHoc.gameObject.SetActive(false); 
               _currePostion.gameObject.SetActive(true);
            }
            // if (!_setupOnStart) return;
            // Setup(_minValue, _maxValue, _initialMinValue, _initialMaxValue);
        }

        private void Update()
        {
            currentPositonUpdate();
            //currentPositonUpdate1();

            if (UpdateMinMaxvalues)
            {
                updateMinMaxVal();
            }
            
 
        }

        public void currentPositonUpdate()
   {        
         
            _currePostion.value = PlutoComm.angle;
           _currePostionHoc.value = -PlutoComm.angle;//-_currePostion.value;
            
            //_currePostion.value = AppData.plutoData.angle;

            float Currevalue = _currePostion.value;

        }

        public void updateMinMaxVal()
        {

             if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
        {
        // Get the current position value
           float currentValue = -_currePostionHoc.value;

        //minAng = Mathf.Clamp(minAng, -90f, 0f);
 

        // Set the minSlider at minAng and allow it to move towards maxAng
           if (currentValue < minAng)
        {
           
            minAng = Mathf.Clamp(currentValue, _minValue, 0f);
            minAng = Mathf.Clamp(currentValue,_minValue,0f);
            _sliderMin.setSliderVal(minAng);

            _sliderMinHoc.setSliderVal(-minAng);
        }

        // Allow the maxSlider to move within the range starting from minAng
        if (currentValue > maxAng)
        {
            //maxAng = Mathf.Clamp(currentValue,  0f,-90f);
            maxAng = Mathf.Clamp(currentValue, _minValue, 0f);
            maxAng = Mathf.Clamp(currentValue, _minValue,0f);
            _sliderMax.setSliderVal(0f);

            _sliderMaxHoc.setSliderVal(-maxAng);

            
        }

        // Ensure that the current position starts at minAng and goes to maxAng
        if (currentValue >= minAng && currentValue <= maxAng)
        {
            _sliderMin.setSliderVal(minAng);
            _sliderMax.setSliderVal(maxAng);

            _sliderMinHoc.setSliderVal(-minAng);
            _sliderMaxHoc.setSliderVal(-maxAng);

            //_sliderMinHoc.setSliderVal(minAng);
            //_sliderMaxHoc.setSliderVal(maxAng);
        }
       }
           
            else
            {
                // Normal operation if PlutoMech != 3
                if (_currePostion.value < minAng)
                {
                    minAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMin.setSliderVal(minAng);
                }
                if (_currePostion.value > maxAng)
                {
                    maxAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMax.setSliderVal(maxAng);
                }
                //OldROMRECT();
            }
        }

        public void Setup(float minValue, float maxValue, float initialMinValue, float initialMaxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _initialMinValue = initialMinValue;
            _initialMaxValue = initialMaxValue;

        if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
          {
            _sliderMinHoc.Setup(-_initialMinValue, minValue, maxValue, MinValueChanged);
            _sliderMaxHoc.Setup(-_initialMaxValue, minValue, maxValue, MaxValueChanged);

            _sliderMin.Setup(_initialMinValue, -maxValue, -minValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, -maxValue, -minValue, MaxValueChanged);

            _currePostion.minValue = minValue;
            _currePostion.maxValue = maxValue;
            _currePostionHoc.minValue = minValue;
            _currePostionHoc.maxValue = maxValue;
                
          }
        else{
            
            _sliderMin.Setup(_initialMinValue, minValue, maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, minValue, maxValue, MaxValueChanged);


            MinValueChanged(_initialMinValue);
            MaxValueChanged(_initialMaxValue);

           
            _currePostion.minValue = minValue;
            _currePostion.maxValue = maxValue;
            

            OldROMRECT();
        }
        }

        public void startAssessment(float val)
        {
            minAng = val;
            maxAng = val;
            _initialMinValue = val;
            _initialMaxValue = val;

        
          
            _sliderMin.Setup(_initialMinValue, _minValue, _maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, _minValue, _maxValue, MaxValueChanged);
       
            


            MinValueChanged(val);
            MaxValueChanged(val);

            
            _currePostion.minValue = _minValue;
            _currePostion.maxValue = _maxValue;
       
            _oldROMRect.localScale = new Vector3(1, 5f, 1);

            //}

        }

        private void OldROMRECT()
        {

            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) == 3)
          {
            // Calculate and set the position for OldROMRect1
            
                float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillAreahoc.rect.width;

                _oldROMRectHoc.offsetMin = new Vector2(offset, _fillRectHoc.offsetMin.y);
                offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillAreahoc.rect.width;

                _oldROMRectHoc.offsetMax = new Vector2(-offset, _fillRectHoc.offsetMax.y);

            // Calculate and set the position for OldROMRect2 (mirroring behavior)
                float offsetMin = ((-MaxValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
                _oldROMRect.offsetMin = new Vector2(offsetMin, _fillRect.offsetMin.y);

                float offsetMax = (1 - ((-MinValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
                _oldROMRect.offsetMax = new Vector2(-offsetMax, _fillRect.offsetMax.y);
        }else{
        
            
            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;

            _oldROMRect.offsetMin = new Vector2(offset, _fillRectHoc.offsetMin.y);
            offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;

            _oldROMRect.offsetMax = new Vector2(-offset, _fillRectHoc.offsetMax.y);
        }
            
        }

        private void MinValueChanged(float value)
        {

            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism)     ==3)
            {
            
            {
                float offsetHoc = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillAreahoc.rect.width;
                _fillRectHoc.offsetMin = new Vector2(offsetHoc, _fillRectHoc.offsetMin.y);

                

                if ((MaxValue - value) < _minDistance)
                {
                    _sliderMin.Value = MaxValue - _minDistance;
                }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMin.transform.SetAsLastSibling();
            }
            {

             float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
            _fillRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);

            if ((MaxValue - value) < _minDistance)
            {
                _sliderMin.Value = MaxValue - _minDistance;
            }

            OnValueChanged.Invoke(MinValue, -MaxValue);
            _sliderMin.transform.SetAsLastSibling();
            }

        }
            else
            {

            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
            _fillRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);

            if ((MaxValue - value) < _minDistance)
            {
                _sliderMin.Value = MaxValue - _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMin.transform.SetAsLastSibling();
            }
        }

        private void MaxValueChanged(float value)
        {

            if (PlutoComm.GetPlutoCodeFromLabel(PlutoComm.MECHANISMS, AppData.selectedMechanism) ==3)
            {
            {
             
                float offsetHoc = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillAreahoc.rect.width;
                _fillRectHoc.offsetMax = new Vector2(-offsetHoc, _fillRectHoc.offsetMax.y);

                if ((value - MinValue) < _minDistance)
                {
                    _sliderMax.Value = MinValue + _minDistance;
                }

                OnValueChanged.Invoke(MinValue, MaxValue);
                _sliderMax.transform.SetAsLastSibling();
             }
              float offset = ( 1- ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
            _fillRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);

            if ((value - MinValue) < _minDistance)
            {
                _sliderMax.Value = MinValue + _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMax.transform.SetAsLastSibling();
             
            }
             else{
            float offset = ( 1- ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
            _fillRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);

            if ((value - MinValue) < _minDistance)
            {
                _sliderMax.Value = MinValue + _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMax.transform.SetAsLastSibling();

             }
        }

    
    }

    
}







































/*#region Includes
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#endregion

namespace TS.DoubleSlider
{
    [RequireComponent(typeof(RectTransform))]
    public class DoubleSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private SingleSlider _sliderMin;
        [SerializeField] private SingleSlider _sliderMax;
        [SerializeField] private Slider _currePostion;

        [SerializeField] private SingleSlider _sliderMinHoc;
        [SerializeField] private SingleSlider _sliderMaxHoc;

        [SerializeField] private Slider _currePostionHoc;

        [SerializeField] private Text warningText;

        [SerializeField] private RectTransform _fillArea;
       // [SerializeField] private RectTransform _fillAreahoc;
        [SerializeField] private RectTransform oldROMArea;
        

        //[SerializeField] private RectTransform oldROMArea2;

        [Header("Configuration")]
        [SerializeField] private bool _setupOnStart;
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _minDistance;
        [SerializeField] private bool _wholeNumbers;
        [SerializeField] private float _initialMinValue;
        [SerializeField] private float _initialMaxValue;

        [Header("Events")]
        public UnityEvent<float, float> OnValueChanged;

        public float minAng, maxAng;
        public bool UpdateMinMaxvalues;

        public PromWF_Scn_Hndlr_newUI promSlider;
        public AromWF_Scn_Hndlr_newUI aromSlider;

        public bool IsEnabled
        {
            get { return _sliderMax.IsEnabled && _sliderMin.IsEnabled; }
            set
            {
                _sliderMin.IsEnabled = value;
                _sliderMax.IsEnabled = value;
            }
        }

        public float MinValue => _sliderMin.Value;
        public float MaxValue => _sliderMax.Value;

        public bool WholeNumbers
        {
            get => _wholeNumbers;
            set
            {
                _wholeNumbers = value;
                _sliderMin.WholeNumbers = _wholeNumbers;
                _sliderMax.WholeNumbers = _wholeNumbers;
            }
        }

        public bool IsDisabled { get; internal set; }

        private RectTransform _fillRect;
        private RectTransform _oldROMRect;
        private bool _isAromActive;
        private bool _isPromActive;
        

        //private RectTransform _oldROMRect2;

        #endregion

        private void Awake()
        {
            if (_sliderMin == null || _sliderMax == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing slider min: " + _sliderMin + ", max: " + _sliderMax);
#endif
                return;
            }

            if (_fillArea == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing fill area");
#endif
                return;
            }



            _fillRect = _fillArea.transform.GetChild(0).transform as RectTransform;
            _oldROMRect = oldROMArea.transform.GetChild(0).transform as RectTransform;

            if (AppData.plutoData.mechIndex == 3)
          {

            _sliderMaxHoc.gameObject.SetActive(true);
            _sliderMinHoc.gameObject.SetActive(true);
            //Activate to those two buttons
            //fill area
            //_oldROMRect2 = oldROMArea2.transform.GetChild(0).transform as RectTransform;
            _maxValue = 90;
            _minValue = -90;
            
           _currePostion.gameObject.SetActive(true);
            _currePostionHoc.gameObject.SetActive(true);
         }
         if (warningText != null)
      {
        warningText.gameObject.SetActive(false);
       }
        }

        private void Start()
        {

            if (AppData.plutoData.mechIndex == 3)
          {
              _currePostion.gameObject.SetActive(true);
              _currePostionHoc.gameObject.SetActive(true);
            }
            // if (!_setupOnStart) return;
            // Setup(_minValue, _maxValue, _initialMinValue, _initialMaxValue);
        }

        private void Update()
        {
            currentPositonUpdate();
            //currentPositonUpdate1();

            if (UpdateMinMaxvalues)
            {
                updateMinMaxVal();
            }
            
           
        }

        public void currentPositonUpdate()
   {        
         
            _currePostion.value = AppData.plutoData.angle;
            _currePostionHoc.value = -_currePostion.value;
           
            //_currePostion.value = AppData.plutoData.angle;

        }

        public void updateMinMaxVal()
    {

            if (AppData.plutoData.mechIndex == 3)
        {
        // Get the current position value
           float currentValue = _currePostion.value;

           _currePostionHoc.value = -currentValue;

        // Set the minSlider at minAng and allow it to move towards maxAng
           if (currentValue < minAng)
        {
            minAng = Mathf.Clamp(currentValue, _minValue, _maxValue);
            _sliderMin.setSliderVal(minAng);

            _sliderMinHoc.setSliderVal(-minAng);
        }

        // Allow the maxSlider to move within the range starting from minAng
        if (currentValue > maxAng)
        {
            maxAng = Mathf.Clamp(currentValue, _minValue, _maxValue);
            _sliderMax.setSliderVal(maxAng);

            _sliderMaxHoc.setSliderVal(-maxAng);
        }

        // Ensure that the current position starts at minAng and goes to maxAng
        if (currentValue >= minAng && currentValue <= maxAng)
        {
            _sliderMin.setSliderVal(minAng);
            _sliderMax.setSliderVal(maxAng);

            _sliderMinHoc.setSliderVal(-minAng);
            _sliderMaxHoc.setSliderVal(-maxAng);

            //_sliderMinHoc.setSliderVal(minAng);
            //_sliderMaxHoc.setSliderVal(maxAng);
        }
       }

            /*if (AppData.plutoData.mechIndex == 3)
            {  

                 
                // Handling MinSlider
                if (_currePostion.value < minAng)
                {
                    minAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMin.setSliderVal(minAng);

                    // Automatically mirror to MaxSlider with inverted value
                    maxAng = -minAng;
                    _sliderMax.setSliderVal(maxAng);

                   //OldROMRECT();
                }

                // Handling MaxSlider
                if (_currePostion.value > maxAng)
                {
                    maxAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMax.setSliderVal(maxAng);

                    // Automatically mirror to MinSlider with inverted value
                    minAng = -maxAng;
                    _sliderMin.setSliderVal(minAng);
                }
                //OldROMRECT();

            }//
            else
            {
                // Normal operation if PlutoMech != 3
                if (_currePostion.value < minAng)
                {
                    minAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMin.setSliderVal(minAng);
                }
                if (_currePostion.value > maxAng)
                {
                    maxAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMax.setSliderVal(maxAng);
                }
                //OldROMRECT();
            }
        }

        

        public void Setup(float minValue, float maxValue, float initialMinValue, float initialMaxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _initialMinValue = initialMinValue;
            _initialMaxValue = initialMaxValue;
        
            _sliderMin.Setup(_initialMinValue, minValue, maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, minValue, maxValue, MaxValueChanged);
            

            MinValueChanged(_initialMinValue);
            MaxValueChanged(_initialMaxValue);

           
            _currePostion.minValue = minValue;
            _currePostion.maxValue = maxValue;
            

            OldROMRECT();
        }

        public void startAssessment(float val)
        {
            minAng = val;
            maxAng = val;
            _initialMinValue = val;
            _initialMaxValue = val;

            //_sliderMin.Setup(val, _minValue, _maxValue, MinValueChanged);
            //_sliderMax.Setup(val, _minValue, _maxValue, MaxValueChanged);
          
            _sliderMin.Setup(_initialMinValue, _minValue, _maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, _minValue, _maxValue, MaxValueChanged);
            


            MinValueChanged(val);
            MaxValueChanged(val);

            //_currePostion.minValue = _minValue;
            //_currePostion.maxValue = _maxValue;
            
            _currePostion.minValue = _minValue;
            _currePostion.maxValue = _maxValue;
            


            _oldROMRect.localScale = new Vector3(1, 5f, 1);
        }

        private void OldROMRECT()
        {


            //if (AppData.plutoData.mechIndex == 3){
                // Calculate and set the position for oldROMRect
                //float offsetMin = ((minAng - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
                //_oldROMRect.offsetMin = new Vector2(offsetMin, _fillRect.offsetMin.y);

               // float offsetMax = (1 - ((maxAng - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
               // _oldROMRect.offsetMax = new Vector2(-offsetMax, _fillRect.offsetMax.y);
           //}
           /* if (AppData.plutoData.mechIndex == 3)
          {
            // Calculate and set the position for OldROMRect1
           float offsetMin = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
           _oldROMRect.offsetMin = new Vector2(offsetMin, _fillRect.offsetMin.y);

           float offsetMax = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
           _oldROMRect.offsetMax = new Vector2(-offsetMax, _fillRect.offsetMax.y);

        // Calculate and set the position for OldROMRect2 (mirroring behavior)
          float offsetMin2 = ((-MaxValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
          _oldROMRect2.offsetMin = new Vector2(offsetMin2, _fillRect.offsetMin.y);

           float offsetMax2 = (1 - ((-MinValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
           _oldROMRect2.offsetMax = new Vector2(-offsetMax2, _fillRect.offsetMax.y);
        }//
        
            
            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;

            _oldROMRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);
            offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;

            _oldROMRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);
            
        }

        private void MinValueChanged(float value)
        {
            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
            _fillRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);

            if ((MaxValue - value) < _minDistance)
            {
                _sliderMin.Value = MaxValue - _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMin.transform.SetAsLastSibling();
        }

        private void MaxValueChanged(float value)
        {
            float offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
            _fillRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);

            if ((value - MinValue) < _minDistance)
            {
                _sliderMax.Value = MinValue + _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMax.transform.SetAsLastSibling();
        }
        

        
    }

    
}*/





















/*#region Includes
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#endregion

namespace TS.DoubleSlider
{
    [RequireComponent(typeof(RectTransform))]
    public class DoubleSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private SingleSlider _sliderMin;
        [SerializeField] private SingleSlider _sliderMax;
        [SerializeField] private Slider _currePostion;

        [SerializeField] private Slider _sliderMinHoc;
        [SerializeField] private Slider _sliderMaxHoc;
        

        

        [SerializeField] private Text warningText;

        [SerializeField] private RectTransform _fillArea;
        [SerializeField] private RectTransform _fillAreahoc;
        [SerializeField] private RectTransform oldROMArea;
        

        //[SerializeField] private RectTransform oldROMArea2;

        [Header("Configuration")]
        [SerializeField] private bool _setupOnStart;
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _minDistance;
        [SerializeField] private bool _wholeNumbers;
        [SerializeField] private float _initialMinValue;
        [SerializeField] private float _initialMaxValue;

        [Header("Events")]
        public UnityEvent<float, float> OnValueChanged;

        public float minAng, maxAng;
        public bool UpdateMinMaxvalues;

        public PromWF_Scn_Hndlr_newUI promSlider;
        public AromWF_Scn_Hndlr_newUI aromSlider;

        public bool IsEnabled
        {
            get { return _sliderMax.IsEnabled && _sliderMin.IsEnabled; }
            set
            {
                _sliderMin.IsEnabled = value;
                _sliderMax.IsEnabled = value;
            }
        }

        public float MinValue => _sliderMin.Value;
        public float MaxValue => _sliderMax.Value;

        public bool WholeNumbers
        {
            get => _wholeNumbers;
            set
            {
                _wholeNumbers = value;
                _sliderMin.WholeNumbers = _wholeNumbers;
                _sliderMax.WholeNumbers = _wholeNumbers;
            }
        }

        public bool IsDisabled { get; internal set; }

        private RectTransform _fillRect;
        private RectTransform _oldROMRect;
        private bool _isAromActive;
        private bool _isPromActive;
        

        //private RectTransform _oldROMRect2;

        #endregion

        private void Awake()
        {
            if (_sliderMin == null || _sliderMax == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing slider min: " + _sliderMin + ", max: " + _sliderMax);
#endif
                return;
            }

            if (_fillArea == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing fill area");
#endif
                return;
            }



            _fillRect = _fillArea.transform.GetChild(0).transform as RectTransform;
            _oldROMRect = oldROMArea.transform.GetChild(0).transform as RectTransform;

            if (AppData.plutoData.mechIndex == 3)
          {

            _sliderMaxHoc.gameObject.SetActive(true);
            _sliderMinHoc.gameObject.SetActive(true);
            //Activate to those two buttons
            //fill area
             //_oldROMRect2 = oldROMArea2.transform.GetChild(0).transform as RectTransform;

            _minValue = -90;
            _maxValue = 90;
           _currePostion.gameObject.SetActive(true);
         }
         if (warningText != null)
      {
        warningText.gameObject.SetActive(false);
       }
        }

        private void Start()
        {

            if (AppData.plutoData.mechIndex == 3)
          {
              _currePostion.gameObject.SetActive(true);
            }
            // if (!_setupOnStart) return;
            // Setup(_minValue, _maxValue, _initialMinValue, _initialMaxValue);
        }

        private void Update()
        {
            currentPositonUpdate();
            //currentPositonUpdate1();

            if (UpdateMinMaxvalues)
            {
                updateMinMaxVal();
            }
           
        }

        public void currentPositonUpdate()
   {        
         
            _currePostion.value = AppData.plutoData.angle;
            
            //_currePostion.value = AppData.plutoData.angle;

        }

        public void updateMinMaxVal()
        {
            if (AppData.plutoData.mechIndex == 3)
            {  

                 
                // Handling MinSlider
                if (_currePostion.value < minAng)
                {
                    minAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMin.setSliderVal(minAng);

                    // Automatically mirror to MaxSlider with inverted value
                    maxAng = -minAng;
                    _sliderMax.setSliderVal(maxAng);

                   //OldROMRECT();
                }

                // Handling MaxSlider
                if (_currePostion.value > maxAng)
                {
                    maxAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMax.setSliderVal(maxAng);

                    // Automatically mirror to MinSlider with inverted value
                    minAng = -maxAng;
                    _sliderMin.setSliderVal(minAng);
                }
                //OldROMRECT();

            }
            else
            {
                // Normal operation if PlutoMech != 3
                if (_currePostion.value < minAng)
                {
                    minAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMin.setSliderVal(minAng);
                }
                if (_currePostion.value > maxAng)
                {
                    maxAng = Mathf.Clamp(_currePostion.value, _minValue, _maxValue);
                    _sliderMax.setSliderVal(maxAng);
                }
                //OldROMRECT();
            }
        }

        

        public void Setup(float minValue, float maxValue, float initialMinValue, float initialMaxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _initialMinValue = initialMinValue;
            _initialMaxValue = initialMaxValue;
        
            _sliderMin.Setup(_initialMinValue, minValue, maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, minValue, maxValue, MaxValueChanged);
            

            MinValueChanged(_initialMinValue);
            MaxValueChanged(_initialMaxValue);

           
            _currePostion.minValue = minValue;
            _currePostion.maxValue = maxValue;
            

            OldROMRECT();
        }

        public void startAssessment(float val)
        {
            minAng = val;
            maxAng = val;
            _initialMinValue = val;
            _initialMaxValue = val;

            //_sliderMin.Setup(val, _minValue, _maxValue, MinValueChanged);
            //_sliderMax.Setup(val, _minValue, _maxValue, MaxValueChanged);
          
            _sliderMin.Setup(_initialMinValue, _minValue, _maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, _minValue, _maxValue, MaxValueChanged);
            


            MinValueChanged(val);
            MaxValueChanged(val);

            //_currePostion.minValue = _minValue;
            //_currePostion.maxValue = _maxValue;
            
            _currePostion.minValue = _minValue;
            _currePostion.maxValue = _maxValue;
            


            _oldROMRect.localScale = new Vector3(1, 5f, 1);
        }

        private void OldROMRECT()
        {


            //if (AppData.plutoData.mechIndex == 3){
                // Calculate and set the position for oldROMRect
                //float offsetMin = ((minAng - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
                //_oldROMRect.offsetMin = new Vector2(offsetMin, _fillRect.offsetMin.y);

               // float offsetMax = (1 - ((maxAng - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
               // _oldROMRect.offsetMax = new Vector2(-offsetMax, _fillRect.offsetMax.y);
           //}
           /* if (AppData.plutoData.mechIndex == 3)
          {
            // Calculate and set the position for OldROMRect1
           float offsetMin = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
           _oldROMRect.offsetMin = new Vector2(offsetMin, _fillRect.offsetMin.y);

           float offsetMax = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
           _oldROMRect.offsetMax = new Vector2(-offsetMax, _fillRect.offsetMax.y);

        // Calculate and set the position for OldROMRect2 (mirroring behavior)
          float offsetMin2 = ((-MaxValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
          _oldROMRect2.offsetMin = new Vector2(offsetMin2, _fillRect.offsetMin.y);

           float offsetMax2 = (1 - ((-MinValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
           _oldROMRect2.offsetMax = new Vector2(-offsetMax2, _fillRect.offsetMax.y);
        }//
        
            
            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;

            _oldROMRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);
            offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;

            _oldROMRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);
            
        }

        private void MinValueChanged(float value)
        {
            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
            _fillRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);

            if ((MaxValue - value) < _minDistance)
            {
                _sliderMin.Value = MaxValue - _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMin.transform.SetAsLastSibling();
        }

        private void MaxValueChanged(float value)
        {
            float offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
            _fillRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);

            if ((value - MinValue) < _minDistance)
            {
                _sliderMax.Value = MinValue + _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMax.transform.SetAsLastSibling();
        }
        

        
    }

    
}*/






































/*#region Includes
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#endregion

namespace TS.DoubleSlider
{
    [RequireComponent(typeof(RectTransform))]
    public class DoubleSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [SerializeField] private SingleSlider _sliderMin;
        [SerializeField] private SingleSlider _sliderMax;
        [SerializeField] private Slider promslider;

        [SerializeField] private RectTransform _fillArea;
        [SerializeField] private RectTransform oldROMArea;

        [Header("Configuration")]
        [SerializeField] private bool _setupOnStart;
        [SerializeField] private float _minValue;
        [SerializeField] private float _maxValue;
        [SerializeField] private float _minDistance;
        [SerializeField] private bool _wholeNumbers;
        [SerializeField] private float _initialMinValue;
        [SerializeField] private float _initialMaxValue;

        [Header("Events")]
        public UnityEvent<float, float> OnValueChanged;

        public float minAng, maxAng;
        public bool UpdateMinMaxvalues;

        public bool IsEnabled
        {
            get { return _sliderMax.IsEnabled && _sliderMin.IsEnabled; }
            set
            {
                _sliderMin.IsEnabled = value;
                _sliderMax.IsEnabled = value;
            }
        }

        public float MinValue => _sliderMin.Value;
        public float MaxValue => _sliderMax.Value;

        public bool WholeNumbers
        {
            get => _wholeNumbers;
            set
            {
                _wholeNumbers = value;
                _sliderMin.WholeNumbers = _wholeNumbers;
                _sliderMax.WholeNumbers = _wholeNumbers;
            }
        }

        private RectTransform _fillRect;
        private RectTransform _oldROMRect;

        #endregion

        private void Awake()
        {
            if (_sliderMin == null || _sliderMax == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing slider min: " + _sliderMin + ", max: " + _sliderMax);
#endif
                return;
            }

            if (_fillArea == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Missing fill area");
#endif
                return;
            }

            _fillRect = _fillArea.transform.GetChild(0).transform as RectTransform;
            _oldROMRect = oldROMArea.transform.GetChild(0).transform as RectTransform;

            if (AppData.plutoData.mechIndex == 3)
          {
            _minValue = -90;
            _maxValue = 90;
           promslider.gameObject.SetActive(false);
         }
        }

        private void Start()
        {

            if (AppData.plutoData.mechIndex == 3)
          {
              promslider.gameObject.SetActive(false);
            }
            // if (!_setupOnStart) return;
            // Setup(_minValue, _maxValue, _initialMinValue, _initialMaxValue);
        }

        private void Update()
        {
            currentPositonUpdate();

            if (UpdateMinMaxvalues)
            {
                updateMinMaxVal();
            }
        }

        public void currentPositonUpdate()
        {
            promslider.value = AppData.plutoData.angle;
        }

        public void updateMinMaxVal()
        {
            if (AppData.plutoData.mechIndex == 3)
            {  

            
                // Handling MinSlider
                if (promslider.value < minAng)
                {
                    minAng = Mathf.Clamp(promslider.value, _minValue, _maxValue);
                    _sliderMin.setSliderVal(minAng);

                    // Automatically mirror to MaxSlider with inverted value
                    maxAng = -minAng;
                    _sliderMax.setSliderVal(maxAng);

                   
                }

                // Handling MaxSlider
                if (promslider.value > maxAng)
                {
                    maxAng = Mathf.Clamp(promslider.value, _minValue, _maxValue);
                    _sliderMax.setSliderVal(maxAng);

                    // Automatically mirror to MinSlider with inverted value
                    minAng = -maxAng;
                    _sliderMin.setSliderVal(minAng);
                }
            }
            else
            {
                // Normal operation if PlutoMech != 3
                if (promslider.value < minAng)
                {
                    minAng = Mathf.Clamp(promslider.value, _minValue, _maxValue);
                    _sliderMin.setSliderVal(minAng);
                }
                if (promslider.value > maxAng)
                {
                    maxAng = Mathf.Clamp(promslider.value, _minValue, _maxValue);
                    _sliderMax.setSliderVal(maxAng);
                }
            }
        }

        public void Setup(float minValue, float maxValue, float initialMinValue, float initialMaxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _initialMinValue = initialMinValue;
            _initialMaxValue = initialMaxValue;

            _sliderMin.Setup(_initialMinValue, minValue, maxValue, MinValueChanged);
            _sliderMax.Setup(_initialMaxValue, minValue, maxValue, MaxValueChanged);

            MinValueChanged(_initialMinValue);
            MaxValueChanged(_initialMaxValue);

            promslider.minValue = minValue;
            promslider.maxValue = maxValue;

            OldROMRECT();
        }

        public void startAssessment(float val)
        {
            minAng = val;
            maxAng = val;
            _initialMinValue = val;
            _initialMaxValue = val;

            _sliderMin.Setup(val, _minValue, _maxValue, MinValueChanged);
            _sliderMax.Setup(val, _minValue, _maxValue, MaxValueChanged);

            MinValueChanged(val);
            MaxValueChanged(val);

            promslider.minValue = _minValue;
            promslider.maxValue = _maxValue;

            _oldROMRect.localScale = new Vector3(1, 5f, 1);
        }

        private void OldROMRECT()
        {
            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;

            _oldROMRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);
            offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;

            _oldROMRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);
        }

        private void MinValueChanged(float value)
        {
            float offset = ((MinValue - _minValue) / (_maxValue - _minValue)) * _fillArea.rect.width;
            _fillRect.offsetMin = new Vector2(offset, _fillRect.offsetMin.y);

            if ((MaxValue - value) < _minDistance)
            {
                _sliderMin.Value = MaxValue - _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMin.transform.SetAsLastSibling();
        }

        private void MaxValueChanged(float value)
        {
            float offset = (1 - ((MaxValue - _minValue) / (_maxValue - _minValue))) * _fillArea.rect.width;
            _fillRect.offsetMax = new Vector2(-offset, _fillRect.offsetMax.y);

            if ((value - MinValue) < _minDistance)
            {
                _sliderMax.Value = MinValue + _minDistance;
            }

            OnValueChanged.Invoke(MinValue, MaxValue);
            _sliderMax.transform.SetAsLastSibling();
        }
    }
}*/




