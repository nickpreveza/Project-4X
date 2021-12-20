using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData 
{
    public GameMode gameMode;
    public int turns;
    public int playersInSession;
    public PlayerData player1Data;
    public PlayerData player2Data;
    public PlayerData player3Data;
    public PlayerData player4Data;
    public PlayerData player5Data;
    public PlayerData player6Data;
    public int playerScore;

}

public enum GameMode
{
    SinglePlayer,
    MultiPlayer
}

public enum Platform
{
    Android,
    iOS
}

public enum CivType
{
    Nick,
    Harrys,
    Lydia,
    Tziot
}

