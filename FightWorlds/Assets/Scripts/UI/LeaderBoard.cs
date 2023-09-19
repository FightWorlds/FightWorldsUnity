using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField] private Transform boardContainer;
    private const int boardSize = 10;
    private const int topRecord = 1234567;
    private string[] playerNames;

    public void UpdateBoard(int record)
    {
        playerNames = new string[boardSize - 1]
        { "Maxira", "sergio", "hmmm", "UNKNOWN", "piu-pau",
        "XX_K1LL3R_XX", "shaadaw", "sussy baka", "pigeon"};
        Dictionary<string, int> board = new Dictionary<string, int>(boardSize)
        {
            {playerNames[0], topRecord},
            {playerNames[1], record + 5},
            {"You", record},
            {playerNames[2], 0},
            {playerNames[3], 0},
            {playerNames[4], 0},
            {playerNames[5], 0},
            {playerNames[6], 0},
            {playerNames[7], 0},
            {playerNames[8], 0}
        };
        // custom hardcode filling
        int prevRecord = record;
        for (int i = 3; i < boardSize; i++)
        {
            int dif = Random.Range(0, 10) * i;
            prevRecord = (prevRecord > dif) ? prevRecord - dif : prevRecord;
            board[playerNames[i - 1]] = prevRecord;
        }
        // dict to ui
        for (int i = 0; i < boardSize; i++)
        {
            var boardPos = board.ElementAt(i);
            var posContainer = boardContainer.GetChild(i);
            posContainer.GetChild(0).GetComponent<Text>().text = boardPos.Key;
            posContainer.GetChild(1).GetComponent<Text>().text =
                boardPos.Value.ToString();
        }
    }
}
