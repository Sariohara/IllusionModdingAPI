﻿using System;
using System.Linq;
using KKAPI.Utilities;
using Studio;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Studio.UI
{
    /// <summary>
    /// Custom control that draws a dropdown menu in the Chara > CurrentState studio menu.
    /// </summary>
    public class CurrentStateCategoryDropdown : BaseCurrentStateEditableGuiEntry<int>
    {
        private readonly string[] _items;
        private static GameObject _originalObject;

        /// <summary>
        /// A dropdown for the Chara > CurrentState studio menu.
        /// </summary>
        /// <param name="name">Name of the button, shown on left.</param>
        /// <param name="items">Items shown in the dropdown box. Value is the currently selected index.</param>
        /// <param name="updateValue">Function called when the current character changes and the on/off state needs to be updated.
        /// <code>OCIChar</code> is the newly selected character. Return the new state. Can't be null.</param>
        public CurrentStateCategoryDropdown(string name, string[] items, Func<OCIChar, int> updateValue) : base(name, updateValue)
        {
            _items = items;
        }

        /// <inheritdoc />
        protected override GameObject CreateItem(GameObject categoryObject)
        {
            if (_originalObject == null)
            {
                _originalObject = GameObject.Find("StudioScene/Canvas Main Menu/04_System/01_Screen Effect/Screen Effect/Viewport/Content/Atmosphere");
                // Unused controls, safe to remove for less overhead later on
                foreach (var tr in _originalObject.transform.Cast<Transform>().Where(t => t.name.StartsWith("Toggle")))
                    Object.DestroyImmediate(tr.gameObject);
            }

            var copy = Object.Instantiate(_originalObject, categoryObject.transform, true);
            copy.gameObject.SetActive(true);
            copy.transform.localScale = Vector3.one;
            copy.name = "CustomDropdown " + Name;

            var le = copy.AddComponent<LayoutElement>();
            le.preferredHeight = 20;
            le.preferredWidth = 210;

            var text = copy.transform.Find("Text Draw").GetComponentInChildren<Text>(true);
            text.lineSpacing = -20;

            text.gameObject.SetActive(true);
            text.text = Name;

            var trt = text.rectTransform;
            trt.offsetMin = new Vector2(0, -20);
            trt.offsetMax = new Vector2(100, 0);

            var dropdown = copy.GetComponentInChildren<Dropdown>(true);
            dropdown.gameObject.SetActive(true);
            var drt = dropdown.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(0, 1);
            drt.anchorMax = new Vector2(0, 1);
            drt.offsetMin = new Vector2(65, -20);
            drt.offsetMax = new Vector2(150, 0);

            dropdown.ClearOptions();
            dropdown.AddOptions(_items.ToList());

            dropdown.onValueChanged.ActuallyRemoveAllListeners();
            dropdown.onValueChanged.AddListener(Value.OnNext);
            Value.Subscribe(newSet => dropdown.value = newSet);

            return copy;
        }
    }
}