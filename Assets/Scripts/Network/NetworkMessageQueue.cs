using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine.Networking;
using Debug = Constants.ConsoleLogs;

namespace Network
{
    public class NetworkMessageQueue
    {
        /// <summary> 送信するデータのQueue </summary>
        private readonly Queue<(UnityWebRequest request, Timer counter)> _messageQueue = default;
        /// <summary> 1回の送信処理にかけていい時間 </summary>
        private readonly float _executionTime = 1f;

        /// <summary> 溜めておける未送信、送信中のメッセージ数 </summary>
        private const int MaxStackCount = 5;

        public (UnityWebRequest request, Timer counter) CurrentMessageQueue
        {
            get
            {
                if (_messageQueue == null || _messageQueue.Count <= 0) { return default; }

                return _messageQueue.Peek();
            }
        }

        public NetworkMessageQueue(float executionTime)
        {
            _messageQueue = new();
            _executionTime = executionTime;
        }

        public bool Enqueue(UnityWebRequest request, NetworkModel model)
        {
            if (_messageQueue.Count + 1 >= MaxStackCount) { Debug.Log("これ以上メッセージを溜められません"); return false; }

            var timer = new Timer(_executionTime * 1000f);
            _messageQueue.Enqueue((request, timer));

            //一定時間経過したときの処理
            timer.Elapsed += (sender, e) =>
            {
                Console.WriteLine("一定時間経過したため、データの送信を中断します");
                //todo : ここでmodelに送信処理を中断する通知を行う
                //model.DeleteUnsentRequest(); のような感じ

                //一定時間が経過したら未送信であっても削除する
                Dequeue();
            };

            timer.Start();
            return true;
        }

        public void Dequeue()
        {
            if (_messageQueue == null || _messageQueue.Count <= 0) { return; }

            _ = _messageQueue.Dequeue();
            if (_messageQueue.Count > 0)
            {
                //未送信のデータがまだあれば、そのデータの送信を開始する
            }
        }
    }
}
