using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DotController_script : MonoBehaviour
{
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    private FindMatches findMatches;
    private Board_script board;
    private GameObject otherDot;

    private Vector2 firstTouchPos;
    private Vector2 finalTouchPos;
    private Vector2 tempPosition;
    private float swipeAngle = 0f;
    private float swipeResist = 0.7f;

    public bool isColumnArrow;
    public bool isRowArrow;
    public GameObject columnArrow;
    public GameObject rowArrow;
    public List<GameObject> currentMatches = new List<GameObject>();

    private void Start()
    {
        isColumnArrow = false;
        isRowArrow = false;
        board = FindObjectOfType<Board_script>();
        findMatches = FindObjectOfType<FindMatches>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //column = targetX;
        //previousColumn = column;
        //previousRow = row;
    }

    //For testing and debuging;
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRowArrow = true;
            GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
            arrow.transform.parent = this.transform;

        }
    }

    private void Update()
    {
        FindAllMatches();
        //FindMatch(); 
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color = new Color(0f, 0f, 2f);
        }
        targetX = column;
        targetY = row;
        SwipeHorizontal();
        SwipeVertical();
       
    }

    private void SwipeHorizontal()
    {
        if (Mathf.Abs(targetX - transform.position.x) > 0.1f)
        {
            //Move to the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            
        }
        else
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
        }
    }

    private void SwipeVertical()
    {
        if (Mathf.Abs(targetY - transform.position.y) > 0.1f)
        {
            //Move to the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            
        }
        else
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            
        }
    }

    private IEnumerator CheckMatchCo()
    {
        yield return new WaitForSeconds(0.5f);
        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<DotController_script>().isMatched)
            {
                otherDot.GetComponent<DotController_script>().row = row;
                otherDot.GetComponent<DotController_script>().column = column;

                row = previousRow;
                column = previousColumn;

                yield return new WaitForSeconds(0.3f);
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            otherDot = null;
        }
        
    }

    public void OnMouseDown()
    {
        //Получаем координаты первого касания
        //Проверяем текущее состояние
        if (board.currentState == GameState.move)
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log($"First touch position is {firstTouchPos}");
        }
    }

    public void OnMouseUp()
    {
        if(board.currentState == GameState.move)
        finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateTheAngle();
        //Debug.Log($"Final touch position is {finalTouchPos}");
    }
    void CalculateTheAngle()
    {
        if (Mathf.Abs(finalTouchPos.y - firstTouchPos.y) > swipeResist || Mathf.Abs(finalTouchPos.x - firstTouchPos.x) > swipeResist)
        {
            swipeAngle = Mathf.Atan2(finalTouchPos.y - firstTouchPos.y, finalTouchPos.x - firstTouchPos.x) * 180 / Mathf.PI;
            Debug.Log($"Swipe angle is {swipeAngle}");
            MoveDots();
            board.currentState = GameState.wait;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }
    private void MoveDots()
    {
        //Swipe to Right direction
        if (swipeAngle > -45 && swipeAngle <= 45 && column< board.width - 1)
        {
            otherDot = board.allDots[column + 1, row];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<DotController_script>().column -= 1;
            column += 1;
        }
        //Swipe to Up direction
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            otherDot = board.allDots[column, row + 1];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<DotController_script>().row -= 1;
            row += 1;
        }
        //Swipe to Left direction
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            otherDot = board.allDots[column - 1, row];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<DotController_script>().column += 1;
            column -= 1;
        }
        //Swipe to Down direction
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            otherDot = board.allDots[column, row - 1];
            previousColumn = column;
            previousRow = row;
            otherDot.GetComponent<DotController_script>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMatchCo());
    }

    //private void FindMatch()
    //{
    //    if (column > 0 && column < board.width - 1)
    //    {
    //        GameObject leftDot1 = board.allDots[column - 1, row];
    //        GameObject rightDot1 = board.allDots[column + 1, row];
    //        if (leftDot1 != null && rightDot1 != null)
    //        {
    //            if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
    //            {
    //                leftDot1.GetComponent<DotController_script>().isMatched = true;
    //                rightDot1.GetComponent<DotController_script>().isMatched = true;
    //                isMatched = true;
    //            }
    //        }

    //    }
    //    if (row > 0 && row < board.height - 1)
    //    {
    //        GameObject upDot1 = board.allDots[column, row + 1];
    //        GameObject downDot1 = board.allDots[column, row - 1];
    //        if (upDot1 != null && downDot1 != null)
    //        {
    //            if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
    //            {
    //                upDot1.GetComponent<DotController_script>().isMatched = true;
    //                downDot1.GetComponent<DotController_script>().isMatched = true;
    //                isMatched = true;
    //            }
    //        }

    //    }
    //}

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }
    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];
                if (currentDot != null)
                {
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];
                        if (leftDot != null && rightDot != null)
                        {
                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                if (currentDot.GetComponent<DotController_script>().isRowArrow
                                    || leftDot.GetComponent<DotController_script>().isRowArrow
                                    || rightDot.GetComponent<DotController_script>().isRowArrow)
                                {
                                    currentMatches.Union(GetRowDots(j));
                                }

                                if (!currentMatches.Contains(leftDot))
                                {
                                    currentMatches.Add(leftDot);
                                }
                                if (!currentMatches.Contains(rightDot))
                                {
                                    currentMatches.Add(rightDot);
                                }
                                leftDot.GetComponent<DotController_script>().isMatched = true;
                                rightDot.GetComponent<DotController_script>().isMatched = true;
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<DotController_script>().isMatched = true;
                            }
                        }
                    }
                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];
                        if (upDot != null && downDot != null)
                        {
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                if (!currentMatches.Contains(upDot))
                                {
                                    currentMatches.Add(upDot);
                                }
                                if (!currentMatches.Contains(downDot))
                                {
                                    currentMatches.Add(downDot);
                                }
                                upDot.GetComponent<DotController_script>().isMatched = true;
                                downDot.GetComponent<DotController_script>().isMatched = true;
                                if (!currentMatches.Contains(currentDot))
                                {
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<DotController_script>().isMatched = true;
                            }
                        }
                    }
                }
            }
        }

        List<GameObject> GetColumnDots(int column)
        {
            List<GameObject> dots = new List<GameObject>();
            for (int i = 0; i < board.height; i++)
            {
                if (board.allDots[column, i] != null)
                {
                    dots.Add(board.allDots[column, i]);
                    board.allDots[column, i].GetComponent<DotController_script>().isMatched = true;
                }
            }
            return dots;
        }
        List<GameObject> GetRowDots(int row)
        {
            List<GameObject> dots = new List<GameObject>();
            for (int i = 0; i < board.width; i++)
            {
                if (board.allDots[i, row] != null)
                {
                    dots.Add(board.allDots[i, row]);
                    board.allDots[i, row].GetComponent<DotController_script>().isMatched = true;
                }
            }
            return dots;
        }
    }



}
