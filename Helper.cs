using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;


namespace Helpers
{
    public static class Helper
    {
        private static Camera _camera;

        public static Camera Camera
        {
            get
            {
                if (_camera == null) _camera = Camera.main;
                return _camera;
            }
        }


        private static readonly Dictionary<float, WaitForSeconds> WaitDictionary = new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds GetWait(float time)
        {
            if (WaitDictionary.TryGetValue(time, out var wait)) return wait;

            WaitDictionary[time] = new WaitForSeconds(time);
            return WaitDictionary[time];
        }

        private static PointerEventData _eventDataPos;
        private static List<RaycastResult> _results;

        public static bool IsOverUI()
        {
            _eventDataPos = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            _results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventDataPos, _results);
            //for (int i = 0; i < _results.Count; i++)
            //{
            //    Debug.Log(_results[i].gameObject.name);
            //}
            return _results.Count > 0;
        }

        public static Vector2 GetWorldPosOfCanvasElement(RectTransform element)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(element,element.position,Camera,out var result);
            return result;
        }

        public static void DeleteChildren(this Transform t)
        {
            foreach (Transform child in t) Object.Destroy(child.gameObject);
        }
    }

    

}

