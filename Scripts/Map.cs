using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sonic853.GreedySnakeGame
{
    public class Map : MonoBehaviour
    {
        [SerializeField] TMP_InputField widthInput;
        [SerializeField] TMP_InputField heightInput;
        /// <summary>
        /// 地图的宽度
        /// </summary>
        int Width => int.TryParse(widthInput.text, out var _) ? int.Parse(widthInput.text) : 15;
        /// <summary>
        /// 地图的高度
        /// </summary>
        int Height => int.TryParse(heightInput.text, out var _) ? int.Parse(heightInput.text) : 10;
        /// <summary>
        /// 总面积
        /// </summary>
        public int Area => Width * Height;
        /// <summary>
        /// 四周的墙，分别是上下左右
        /// </summary>
        [SerializeField] GameObject[] colliders;
        /// <summary>
        /// 食物的预制件
        /// </summary>
        [SerializeField] GameObject foodPrefab;
        public static Map Instance { get; private set; }
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
        void Start()
        {
            Init();
        }
        public void Init()
        {
            // colliders 有四个元素，分别是上下左右的墙，根据 width 和 height 调整墙的位置和大小
            var wallx = Width % 2 == 0 ? -0.5f : 0;
            var wally = Height % 2 == 0 ? -0.5f : 0;
            colliders[0].transform.position = new Vector3(wallx, Mathf.Floor((Height / 2f) + 0.5f), 0);
            colliders[0].transform.localScale = new Vector3(Width, 1, 1);
            colliders[1].transform.position = new Vector3(wallx, Mathf.Floor((-Height / 2f) - 0.5f), 0);
            colliders[1].transform.localScale = new Vector3(Width, 1, 1);
            colliders[2].transform.position = new Vector3(Mathf.Floor((-Width / 2f) - 0.5f), wally, 0);
            colliders[2].transform.localScale = new Vector3(1, Height, 1);
            colliders[3].transform.position = new Vector3(Mathf.Floor((Width / 2f) + 0.5f), wally, 0);
            colliders[3].transform.localScale = new Vector3(1, Height, 1);
            Snake.Instance.Init();
            ClearFood();
            CreateFood();
        }
        public void CreateFood()
        {
            if (Snake.Instance.Length >= Area)
                return;
            var point = new Vector3(Random.Range(-Width / 2, Width / 2), Random.Range(-Height / 2, Height / 2), 0);
            // 食物不能生成在蛇身上
            while (Physics2D.OverlapPoint(point) != null)
            {
                point = new Vector3(Random.Range(-Width / 2, Width / 2), Random.Range(-Height / 2, Height / 2), 0);
            }
            Instantiate(foodPrefab, point, Quaternion.identity);
        }
        public void ClearFood()
        {
            foreach (var food in GameObject.FindGameObjectsWithTag("Food"))
            {
                Destroy(food);
            }
        }
    }
}
