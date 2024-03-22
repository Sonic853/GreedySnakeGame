using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sonic853.GreedySnakeGame
{
    public class Snake : MonoBehaviour
    {
        /// <summary>
        /// 第一个元素是蛇头，后面的元素是蛇尾，在移动时先把蛇尾移动到前一个元素的位置，然后再移动蛇头，不可向相反方向移动
        /// </summary>
        /// <returns></returns>
        readonly List<Transform> snakeBody = new();
        public int Length => snakeBody.Count;
        /// <summary>
        /// 蛇身体的预制件
        /// </summary>
        [SerializeField] GameObject bodyPrefab;
        /// <summary>
        /// 放置蛇身体的地方
        /// </summary>
        [SerializeField] GameObject bodyParent;
        /// <summary>
        /// 音源
        /// </summary>
        [SerializeField] AudioSource audioSource;
        /// <summary>
        /// 死亡音效
        /// </summary>
        [SerializeField] AudioClip deathSound;
        /// <summary>
        /// 吃入音效
        /// </summary>
        [SerializeField] AudioClip eatSound;
        /// <summary>
        /// 胜利音效
        /// </summary>
        [SerializeField] AudioClip winSound;
        /// <summary>
        /// 游戏文字
        /// </summary>
        [SerializeField] TextMeshPro gameText;
        /// <summary>
        /// 开始长度
        /// </summary>
        readonly int startLength = 2;
        /// <summary>
        /// 速度
        /// </summary>
        public float speed = 1;
        /// <summary>
        /// 上一次移动的时间
        /// </summary>
        float lastMoveTime = 0;
        /// <summary>
        /// 方向
        /// </summary>
        Direction direction = Direction.Right;
        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool IsDead { get; private set; } = false;
        public static Snake Instance { get; private set; }
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        // void Start()
        // {
        //     Init();
        // }
        public void Init()
        {
            IsDead = false;
            lastMoveTime = Time.time;
            direction = Direction.Right;
            foreach (var body in snakeBody)
            {
                Destroy(body.gameObject);
            }
            snakeBody.Clear();
            for (int i = 0; i < startLength; i++)
            {
                snakeBody.Add(Instantiate(bodyPrefab, new Vector3(-i, 0, 0), Quaternion.identity, bodyParent.transform).transform);
            }
            gameText.gameObject.SetActive(false);
        }
        void Update()
        {
            if (IsDead || snakeBody.Count >= Map.Instance.Area)
                return;
            // WASD 或者 方向键 控制方向
            if (Input.anyKeyDown)
            {
                // 头部
                var headPosition = snakeBody[0].position;
                // 头部的上一个部位
                var lastHeadPosition = snakeBody[1].position;
                // 根据这两个部位的位置，判断方向
                Direction _direction;
                switch (true)
                {
                    case var _ when headPosition.x > lastHeadPosition.x:
                        _direction = Direction.Right;
                        break;
                    case var _ when headPosition.x < lastHeadPosition.x:
                        _direction = Direction.Left;
                        break;
                    case var _ when headPosition.y > lastHeadPosition.y:
                        _direction = Direction.Up;
                        break;
                    case var _ when headPosition.y < lastHeadPosition.y:
                        _direction = Direction.Down;
                        break;
                    default:
                        Debug.LogError("方向错误");
                        _direction = direction;
                        break;
                }
                switch (Input.inputString)
                {
                    case "w":
                    case "W":
                    case "UpArrow":
                        if (_direction != Direction.Down)
                            _direction = Direction.Up;
                        break;
                    case "s":
                    case "S":
                    case "DownArrow":
                        if (_direction != Direction.Up)
                            _direction = Direction.Down;
                        break;
                    case "a":
                    case "A":
                    case "LeftArrow":
                        if (_direction != Direction.Right)
                            _direction = Direction.Left;
                        break;
                    case "d":
                    case "D":
                    case "RightArrow":
                        if (_direction != Direction.Left)
                            _direction = Direction.Right;
                        break;
                    default:
                        _direction = direction;
                        break;
                }
                direction = _direction;
            }
            if (Time.time - lastMoveTime > 1 / speed)
            {
                lastMoveTime = Time.time;
                Move();
            }
        }
        void Move()
        {
            Vector3 nextPosition = snakeBody[0].position;
            switch (direction)
            {
                case Direction.Up:
                    nextPosition += Vector3.up;
                    break;
                case Direction.Down:
                    nextPosition += Vector3.down;
                    break;
                case Direction.Left:
                    nextPosition += Vector3.left;
                    break;
                case Direction.Right:
                    nextPosition += Vector3.right;
                    break;
            }
            // 根据 nextPosition 查找碰撞体
            var hit = Physics2D.OverlapPoint(nextPosition);
            if (hit != null)
            {
                if (hit.CompareTag("Food"))
                {
                    // 吃到食物
                    Destroy(hit.gameObject);
                    snakeBody.Insert(0, Instantiate(bodyPrefab, nextPosition, Quaternion.identity, bodyParent.transform).transform);
                    if (snakeBody.Count >= Map.Instance.Area)
                    {
                        // 胜利
                        audioSource.PlayOneShot(winSound);
                        gameText.gameObject.SetActive(true);
                        gameText.text = $"You Win!\nScore: {snakeBody.Count - startLength}";
                        return;
                    }
                    else
                    {
                        audioSource.PlayOneShot(eatSound);
                    }
                    Map.Instance.CreateFood();
                }
                else if (hit.CompareTag("Wall") || hit.CompareTag("Body"))
                {
                    // 死亡
                    IsDead = true;
                    audioSource.PlayOneShot(deathSound);
                    gameText.gameObject.SetActive(true);
                    gameText.text = $"Game Over\nScore: {snakeBody.Count - startLength}";
                }
                return;
            }
            snakeBody.Insert(0, snakeBody[^1]);
            snakeBody.RemoveAt(snakeBody.Count - 1);
            snakeBody[0].position = nextPosition;
        }
    }
    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
