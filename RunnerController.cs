using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class RunnerController : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private Camera _mainCam;
    private Plane _plane;
    private Vector3 _oldPosition;

    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float minX = -2f;
    [SerializeField] private float maxX = 2f;

    private bool _isPressed = false;
    private Vector3 _lastPosition;
    private float _stopTimer = 0f;
    private const float StopDurationThreshold = 0.5f;
    private bool _isJumping = false;

    private void OnEnable()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _mainCam = Camera.main;
        _plane = new Plane(Vector3.up, Vector3.zero);

        JumpButton.OnButtonClicked.AddListener(Jump);

        if (Input.GetMouseButton(0))
        {
            _oldPosition = GetMousePoint();
            _isPressed = true;
        }
    }

    private Vector3 GetTouchPoint(Touch touch)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane));
    }

    private void Update()
    {
        if (HelperUI.IsOverUI())
        {
            _isPressed = false;
            return;
        }

        bool isMobile = Application.isMobilePlatform;

        if (isMobile)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 touchPosition = touch.position;
                Vector3 worldPosition = _mainCam.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, _mainCam.nearClipPlane));

                if (worldPosition.y > 4.45f)
                {
                    _isPressed = false;
                    return;
                }

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _oldPosition = GetTouchWorldPoint(touch);
                        _isPressed = true;
                        break;

                    case TouchPhase.Moved:
                        _isPressed = true;
                        break;

                    case TouchPhase.Ended:
                        _isPressed = false;
                        break;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                _oldPosition = GetMousePoint();
                _isPressed = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isPressed = false;
            }

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        Vector3 velocity;
        bool isMobile = Application.isMobilePlatform;

        if (_isPressed)
        {
            Vector3 currentInputPosition;

            if (isMobile)
            {
                if (Input.touchCount > 0)
                {
                    currentInputPosition = GetTouchWorldPoint(Input.GetTouch(0));
                }
                else
                {
                    currentInputPosition = _oldPosition;
                }
            }
            else
            {
                currentInputPosition = GetMousePoint();
            }

            Vector3 delta = currentInputPosition - _oldPosition;
            _oldPosition = currentInputPosition;

            float xVelocity = delta.x / Time.fixedDeltaTime;
            float newXPosition = Mathf.Clamp(_rigidBody.position.x + xVelocity * Time.fixedDeltaTime, minX, maxX);
            velocity = new Vector3((newXPosition - _rigidBody.position.x) / Time.fixedDeltaTime, _rigidBody.velocity.y, forwardSpeed);
        }
        else
        {
            velocity = new Vector3(0, _rigidBody.velocity.y, forwardSpeed);
        }

        _rigidBody.velocity = velocity;

        // Логика остановки: если объект почти не движется по оси Z в течение некоторого времени – запускаем эффекты разрушения
        if (Mathf.Abs(_rigidBody.position.z - _lastPosition.z) < 0.01f)
        {
            _stopTimer += Time.fixedDeltaTime;

            if (_stopTimer >= StopDurationThreshold)
            {
                DestroyEffects();
            }
        }
        else
        {
            _stopTimer = 0f;
        }

        _lastPosition = _rigidBody.position;
    }

    private Vector3 GetTouchWorldPoint(Touch touch)
    {
        Ray ray = _mainCam.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y, 0));
        if (_plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }



    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    private void Jump()
    {
        if (_isJumping) return;
        _isJumping = true;
        SoundManagerGRGR.Instance.Play("Jump");
        _rigidBody.AddForce(new Vector3(0, 350f, 0), ForceMode.Impulse);
        StartCoroutine(HandleJump());
    }

    private IEnumerator HandleJump()
    {
        yield return new WaitForSeconds(0.5f);
        _isJumping = false;
    }

    private Vector3 GetMousePoint()
    {
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        if (_plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return Vector3.zero;
    }

    [SerializeField] MeshRenderer secondMeshRenderer;
    public void DestroyEffects()
    {
        SoundManagerGRGR.Instance.Play("Lose");
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<DestroyEffectPlayer>().DestroyEffect();

        if(secondMeshRenderer != null)
            secondMeshRenderer.enabled = false;

        GetComponent<CubeController>().enabled = false;

        FindObjectOfType<SceneRestart>().RestartScene();
    }

    private void OnDisable()
    {
        JumpButton.OnButtonClicked.RemoveListener(Jump);
        DOTween.KillAll();
        _isJumping = false;
    }
}
public static class HelperUI
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
        RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, Camera, out var result);
        return result;
    }

    public static void DeleteChildren(this Transform t)
    {
        foreach (Transform child in t) Object.Destroy(child.gameObject);
    }
}
