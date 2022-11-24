 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Board_script : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int height;
    public int width;
    public int offSet;
    private int maxCheckTimes = 0;
    private FindMatches findMatches;
    public GameObject tilePrefab;
    public GameObject destroyEffect;
    public GameObject[] dots;
    private Background_tile[,] allTiles;
    public GameObject[,] allDots;

    private void Start()
    {
        findMatches = FindObjectOfType<FindMatches>();
        allTiles = new Background_tile[width, height];
        allDots = new GameObject[width, height];
        SetUp();
    }

    private void SetUp()
    {
        for(int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j + offSet);
                GameObject backgroundTile =  Instantiate(tilePrefab,tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";
                int dotToUse = Random.Range(0, dots.Length);

                while (MatchesAt(i, j, dots[dotToUse]) && maxCheckTimes < 100)
                {
                    dotToUse = Random.Range(0, dots.Length);
                    maxCheckTimes++;
                }
                maxCheckTimes = 0;

                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                dot.GetComponent<DotController_script>().row = j;
                dot.GetComponent<DotController_script>().column = i;
                dot.transform.parent = this.transform;
                dot.name = "(" + i + "," + j + ")";
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject dot)
    {
        if (column > 1 && row > 1)
        {
            if (allDots[column - 1, row].tag == dot.tag && allDots[column - 2, row].tag == dot.tag)
            {
                return true;
            }
            if (allDots[column, row - 1].tag == dot.tag && allDots[column, row - 2].tag == dot.tag)
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[column, row - 1].tag == dot.tag && allDots[column, row - 2].tag == dot.tag)
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (allDots[column - 1, row].tag == dot.tag && allDots[column - 2, row].tag == dot.tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<DotController_script>().isMatched)
        {
            findMatches.currentMatches.Remove(allDots[column, row]);
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, 0.3f);
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount != 0)
                {
                    allDots[i, j].GetComponent<DotController_script>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FillingBoardCo());
    }

    private void RefillingBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject dot1 = Instantiate(dots[dotToUse], tempPosition,Quaternion.identity);
                    allDots[i, j] = dot1;
                    dot1.GetComponent<DotController_script>().row = j;
                    dot1.GetComponent<DotController_script>().column = i;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<DotController_script>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillingBoardCo()
    {
        //ѕополн€ем игровое поле новыми точками
        RefillingBoard();
        yield return new WaitForSeconds(0.2f);

        //ѕровер€ем наличие совпадений и удал€ем их
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.1f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(0.2f);
        currentState = GameState.move;
    }
}
