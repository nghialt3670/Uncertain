using System.Collections.Generic;
using UnityEngine;

public enum PlayerRole
{
    ALIEN,
    HUMAN
}

[System.Serializable]
public class Player
{
    [SerializeField] public string name = string.Empty;
    [SerializeField] public int score = 0;
    [SerializeField] public List<string> words = new();
    [SerializeField] public List<PlayerRole> roles = new();
    [SerializeField] public List<int> voteIndices = new();
}
