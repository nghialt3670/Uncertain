using System.Collections.Generic;
using UnityEngine;

public enum Role
{
    ALIEN,
    HUMAN
}

[System.Serializable]
public class Player
{
    [SerializeField] public string name;
    [SerializeField] public int score;
    [SerializeField] public List<string> words;
    [SerializeField] public List<Role> roles;
    [SerializeField] public List<int> voteIndices;
}
