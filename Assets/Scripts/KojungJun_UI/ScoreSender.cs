using Shared.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSender : MonoBehaviour
{
    private int _currentScore = 0;    private bool hasSentFinishLine = false;

    public int CurrentScore
    {
        get => _currentScore;
        set
        {
            if (value != _currentScore)
            {
                _currentScore = value;
                //ScoreUpdate?.Invoke(_currentScore);
                //ClientMessageSender.SendScoreToServer(_currentScore); //������ ����
                LiteNetLibManager.Instance.ScoreUpdate(_currentScore);
            }
        }
    }
    public event Action<int> ScoreUpdate;
    private void FixedUpdate() {
        if (ScoreManager.Instance.IsTracking) {
            CurrentScore = ScoreManager.Instance.CurrentScore;
        }
    }
}
