using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace ChatGPT
{
    public sealed class ChatGPT_GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject FieldBox;
        [SerializeField] private GameObject PreviewBox;
        [SerializeField] private TMP_Text ScoreText;

        [SerializeField] private int FieldWidth = 10;
        [SerializeField] private int FieldHeight = 20;
        [SerializeField] private float FallIntervalSeconds = 0.5f;
        [SerializeField] private List<int> ScorePerLine = new List<int> { 100, 300, 500, 800 };

        private void Start()
        {
        }

        private void Update()
        {
        }
    }
}
