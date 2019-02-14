﻿using System;
using BepInEx;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Custom control that draws a slider and a text box (both are used to edit the same value)
    /// </summary>
    public class MakerSlider : BaseEditableGuiEntry<float>
    {
        private static Transform _sliderCopy;

        private readonly string _settingName;

        private readonly float _maxValue;
        private readonly float _minValue;
        private readonly float _defaultValue;
        
        public MakerSlider(MakerCategory category, string settingName, float minValue, float maxValue, float defaultValue, BaseUnityPlugin owner) : base(category, defaultValue, owner)
        {
            _settingName = settingName;

            _minValue = minValue;
            _maxValue = maxValue;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Custom converter from text in the textbox to the slider value.
        /// If not set, <code>float.Parse(txt) / 100f</code> is used.
        /// </summary>
        public Func<string, float> StringToValue { get; set; }

        /// <summary>
        /// Custom converter from the slider value to what's displayed in the textbox.
        /// If not set, <code>Mathf.RoundToInt(f * 100).ToString()</code> is used.
        /// </summary>
        public Func<float, string> ValueToString { get; set; }
        
        private static Transform SliderCopy
        {
            get
            {
                if (_sliderCopy == null)
                    MakeCopy();

                return _sliderCopy;
            }
        }

        private static void MakeCopy()
        {
            // Exists in male and female maker
            var originalSlider = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/00_FaceTop/tglAll/AllTop/sldTemp").transform;

            _sliderCopy = Object.Instantiate(originalSlider, GuiCacheTransfrom, true);
            _sliderCopy.gameObject.SetActive(false);
            _sliderCopy.name = "sldTemp" + GuiApiNameAppendix;

            var slider = _sliderCopy.Find("Slider").GetComponent<Slider>();
            slider.onValueChanged.RemoveAllListeners();

            var inputField = _sliderCopy.Find("InputField").GetComponent<TMP_InputField>();
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onSubmit.RemoveAllListeners();
            inputField.onEndEdit.RemoveAllListeners();

            var resetButton = _sliderCopy.Find("Button").GetComponent<Button>();
            resetButton.onClick.RemoveAllListeners();

            foreach (var renderer in _sliderCopy.GetComponentsInChildren<Image>())
                renderer.raycastTarget = true;
        }

        /// <inheritdoc />
        protected internal override void Initialize()
        {
            if (_sliderCopy == null)
                MakeCopy();
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var tr = Object.Instantiate(SliderCopy, subCategoryList, true);

            tr.name = "sldTemp" + GuiApiNameAppendix;

            var textMesh = tr.Find("textShape").GetComponent<TextMeshProUGUI>();
            textMesh.text = _settingName;
            textMesh.color = TextColor;

            var slider = tr.Find("Slider").GetComponent<Slider>();
            slider.minValue = _minValue;
            slider.maxValue = _maxValue;
            slider.onValueChanged.AddListener(SetNewValue);

            slider.GetComponent<ObservableScrollTrigger>().OnScrollAsObservable().Subscribe(data =>
            {
                var scrollDelta = data.scrollDelta.y;
                var valueChange = Mathf.Pow(10, Mathf.Round(Mathf.Log10(slider.maxValue / 100)));

                if (scrollDelta < 0f)
                    slider.value += valueChange;
                else if (scrollDelta > 0f)
                    slider.value -= valueChange;
            });

            var inputField = tr.Find("InputField").GetComponent<TMP_InputField>();
            inputField.onEndEdit.AddListener(txt =>
            {
                var result = StringToValue?.Invoke(txt) ?? float.Parse(txt) / 100f;
                slider.value = Mathf.Clamp(result, slider.minValue, slider.maxValue);
            });

            slider.onValueChanged.AddListener(f =>
            {
                if (ValueToString != null)
                    inputField.text = ValueToString(f);
                else
                    inputField.text = Mathf.RoundToInt(f * 100).ToString();
            });

            var resetButton = tr.Find("Button").GetComponent<Button>();
            resetButton.onClick.AddListener(() => slider.value = _defaultValue);

            BufferedValueChanged.Subscribe(f => slider.value = f);

            return tr.gameObject;
        }
    }
}