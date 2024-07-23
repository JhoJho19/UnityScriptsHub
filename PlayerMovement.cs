using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Main
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody _rigidBody;
        private Camera _mainCam;
        private Plane _plane;

        private Vector3 _oldPosition;

        public event Action OnStartJumping;
        public event Action OnStopJumping;

        private Animator _animator;

        private float _distanceToPoint;
        private Ray _ray;

        [SerializeField] private float _lerpSpeed;
        [SerializeField] private float _winLerpSpeed;

        [SerializeField] private float _speed;
        [SerializeField] private float _reducedSpeed;
        [SerializeField] private float _speedForwardInJump;
        private float _startSpeed;

        private CompositeDisposable _updateDis = new CompositeDisposable();
        private CompositeDisposable _fixedUpdateDis = new CompositeDisposable();

        private Collider _collider;

        private bool _isPressed;
        private bool _canMove = true;

        private float _sagTimer;
        [SerializeField] private float _timeToWaitUntilLose;

        [SerializeField] private Sag[] _sagObjects;

        private void Awake()
        {
            _collider = GetComponentInChildren<Collider>();
            _animator = GetComponentInChildren<Animator>();
            _mainCam = Camera.main;
            _rigidBody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            GameManager.OnGameStarted += StartGame;
            GameManager.OnWinAnimationStated += EndAnimationStarted;
            GameManager.OnGameOver += GameOver;
            GameManager.OnLevelCompleted += LevelCompleted;
        }

        private void StartGame()
        {

            _oldPosition = GetMousePoint();

            Observable.EveryUpdate().Subscribe(_ => PerformUpdate()).AddTo(_updateDis);
            Observable.EveryFixedUpdate().Subscribe(_ => PerformFixedUpdate()).AddTo(_fixedUpdateDis);
        }

        private void EndAnimationStarted()
        {
            _updateDis?.Clear();
            _fixedUpdateDis?.Clear();

            _lerpSpeed = _winLerpSpeed;
            _oldPosition = Vector3.zero;

            Observable.EveryFixedUpdate().Subscribe(_ => PerformLastAnimation()).AddTo(_fixedUpdateDis);
        }

        private void GameOver()
        {
            _collider.isTrigger = true;
            _speed = 0;
            _lerpSpeed = 0;
            _rigidBody.velocity = new Vector3(0f, _rigidBody.velocity.y, _rigidBody.velocity.z);
        }

        private void LevelCompleted()
        {
            _fixedUpdateDis?.Clear();

            _rigidBody.velocity = Vector3.zero;
            _rigidBody.isKinematic = true;
        }

        private void OnDisable()
        {
            GameManager.OnGameStarted -= StartGame;
            GameManager.OnWinAnimationStated -= EndAnimationStarted;
            GameManager.OnGameOver -= GameOver;
            GameManager.OnLevelCompleted -= LevelCompleted;
        }

        private void Start()
        {
            transform.SetParent(null);
            _startSpeed = _speed;
            _plane = new Plane(-_mainCam.transform.forward, transform.position);
            _isPressed = true;
        }

        private void PerformUpdate()
        {
            if (!_canMove) return;

            if (IsSaggedAll())
            {
                _sagTimer += Time.deltaTime;
                if (_sagTimer > _timeToWaitUntilLose)
                {
                    _rigidBody.constraints = RigidbodyConstraints.None;
                    GameManager.ChangeGameState(GameState.GameOver);

                    _updateDis?.Clear();
                    _fixedUpdateDis?.Clear();
                }
            }
            else
            {
                _sagTimer = 0;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _oldPosition = GetMousePoint();
                _isPressed = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                _rigidBody.velocity = Vector3.zero;
                _isPressed = false;
            }
        }

        private void FixedUpdate()
        {
            //PerformFixedUpdate();
        }


        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private Transform _centerOfMass;
        [SerializeField] private float _wallTimer;
        private void PerformFixedUpdate()
        {
            Vector3 velocity;
            if (_isPressed)
            {
                Vector3 delta = GetMousePoint() - _oldPosition;
                _oldPosition = GetMousePoint();

                float velocityTurn = delta.x / Time.fixedDeltaTime;
                float newXVelocity = Mathf.Lerp(_rigidBody.velocity.x, velocityTurn, _lerpSpeed * Time.fixedDeltaTime);
                velocity = new Vector3(newXVelocity, _rigidBody.velocity.y, _speed);

            }
            else
            {
                Vector3 forwardVelocity = Vector3.forward * _speed;
                forwardVelocity.y = _rigidBody.velocity.y;
                velocity = forwardVelocity;
            }
            if (_wallTrigger)
            {
                velocity = _wallTrigger.ChangeVelocity(velocity);
            }

            float xColliderSize = _collider.bounds.size.x / 2f;

            {
                Ray ray = new Ray(_centerOfMass.position + Vector3.up * 0.2f, Vector3.left);
                if (Physics.Raycast(ray, out RaycastHit hit, 2f, _obstacleMask, QueryTriggerInteraction.Collide))
                {
                    float frameXOffset = velocity.x * Time.fixedDeltaTime;
                    float newX = transform.position.x + frameXOffset;
                    float correctdedX = Mathf.Max(newX, hit.point.x + xColliderSize + 0.1f); // 0.1f -- половина ширины стенки
                    float correctdedXSpeed = (correctdedX - transform.position.x) / Time.fixedDeltaTime;
                    velocity.x = correctdedXSpeed;

                    if (newX < hit.point.x + xColliderSize + 0.1f)
                        velocity.x = Mathf.Clamp(velocity.x, -400f, 4f);

                }
            }

            {
                Ray ray = new Ray(_centerOfMass.position + Vector3.up * 0.2f, Vector3.right);
                if (Physics.Raycast(ray, out RaycastHit hit, 2f, _obstacleMask, QueryTriggerInteraction.Collide))
                {
                    float frameXOffset = velocity.x * Time.fixedDeltaTime;
                    float newX = transform.position.x + frameXOffset;
                    float correctdedX = Mathf.Min(newX, hit.point.x - xColliderSize - 0.1f); // 0.1f -- половина ширины стенки
                    float correctdedXSpeed = (correctdedX - transform.position.x) / Time.fixedDeltaTime;
                    velocity.x = correctdedXSpeed;

                    if(newX > hit.point.x - xColliderSize - 0.1f)
                        velocity.x = Mathf.Clamp(velocity.x, -4f, 4000f);
                }
            }


            _rigidBody.velocity = velocity;
        }

        private WallTrigger _wallTrigger;
        public void AddWallTrigger(WallTrigger wallTrigger)
        {
            _wallTrigger = wallTrigger;
        }

        public void RemoveWallTrigger()
        {
            _wallTrigger = null;
        }

        private void PerformLastAnimation()
        {
            Vector3 delta = _oldPosition - transform.position;

            float velocityTurn = delta.x / Time.fixedDeltaTime;
            float newXVelocity = Mathf.Lerp(_rigidBody.velocity.x, velocityTurn, _lerpSpeed * 0.5f * Time.fixedDeltaTime);

            //Vector3 velocity = new Vector3(newXVelocity, _rigidBody.velocity.y, _speed);
            float x = Mathf.Lerp(transform.position.x, 0f, Time.deltaTime * 3.5f);
            float xOffset = x - transform.position.x;
            float xSpeed = xOffset / Time.deltaTime;

            //transform.position = new Vector3(x, transform.position.y, transform.position.z);
            Vector3 velocity = new Vector3(xSpeed, 0f, _speed); //

            _rigidBody.velocity = velocity;
        }

        private Vector3 GetMousePoint()
        {
            _plane = new Plane(-_mainCam.transform.forward, transform.position);

            _ray = _mainCam.ScreenPointToRay(Input.mousePosition);

            _plane.Raycast(_ray, out _distanceToPoint);

            return _ray.GetPoint(_distanceToPoint);
        }

        private void OnTriggerEnter(Collider other)
        {
            Tramplin tramplin = other.GetComponent<Tramplin>();
            if (tramplin && !tramplin.IsUsed)
            {
                _animator.SetTrigger(Animations.NumbersFlip);
                tramplin.UseTramplin();
            }
        }

        public void DisableMouse()
        {
            _canMove = false;
            _isPressed = false;
        }
        public void EnableMouse()
        {
            _canMove = true;
        }

        public void Slowdown()
        {
            _speed = _reducedSpeed;
        }

        public void RevertSpeed()
        {
            OnStopJumping?.Invoke();
            _speed = _startSpeed;
        }

        public void SpeedUp()
        {
            OnStartJumping?.Invoke();
            _speed = _speedForwardInJump;
        }

        private void OnDestroy()
        {
            _updateDis?.Clear();
            _fixedUpdateDis?.Clear();
        }

        private bool IsSaggedAll()
        {
            for (int i = 0; i < _sagObjects.Length; i++)
            {
                if (!_sagObjects[i].IsSegged && _sagObjects[i].isActiveAndEnabled)
                {
                    _sagTimer = 0;
                    return false;
                }
            }
            return true;
        }

        public void SetSpeed(float speed)
        {
            _speed = speed;
        }
    }
}

