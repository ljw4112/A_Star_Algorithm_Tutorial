using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public int x, y, g, h;
    // g : 시작으로부터 이동했던 거리
    // h : |가로| + |세로| 장애물 무시하여 목표까지의 거리
    public bool isWall;
    public Node parentNode;

    public int F
    {
        get
        {
            return g + h;
        }
    }

    public Node(bool isWall, int x, int y)
    {
        this.isWall = isWall;
        this.x = x;
        this.y = y;
    }
}

public class GameManager : MonoBehaviour
{
    public Vector2Int bottomLeft, topRight, startPos, targetPos;
    public List<Node> finalNodeList;
    public bool allowDiagonal, dontCrossCorner;

    private int sizeX, sizeY;
    private Node[,] nodeArray;
    private Node startNode, targetNode, curNode;
    private List<Node> openList, closedList;

    public void PathFinding()
    {
        //NodeArray의 크기를 정해주고 isWall, x, y 대입
        this.sizeX = this.topRight.x - this.bottomLeft.x + 1;
        this.sizeY = this.topRight.y - this.bottomLeft.y + 1;
        this.nodeArray = new Node[this.sizeX, this.sizeY];

        for (int i = 0; i < this.sizeX; i++)
        {
            for (int j = 0; j < this.sizeY; j++)
            {
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + this.bottomLeft.x, j + this.bottomLeft.y), 0.4f))
                {
                    if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
                        isWall = true;
                }
                this.nodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y);
            }
        }

        //시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        this.startNode = this.nodeArray[this.startPos.x - this.bottomLeft.x, this.startPos.y - this.bottomLeft.y];
        this.targetNode = this.nodeArray[this.targetPos.x - this.bottomLeft.x, this.targetPos.y - this.bottomLeft.y];

        this.openList = new List<Node>() { this.startNode };
        this.closedList = new List<Node>();
        this.finalNodeList = new List<Node>();

        while (this.openList.Count > 0)
        {
            //열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고
            //열린리스트에서 닫힌리스트로 옮기기
            this.curNode = this.openList[0];
            for(int i = 1; i < this.openList.Count; i++)
            {
                if (this.openList[i].F <= this.curNode.F
                    && this.openList[i].h < this.curNode.h)
                    this.curNode = this.openList[i];
            }

            this.openList.Remove(this.curNode);
            this.closedList.Add(this.curNode);

            //마지막 처리
            if(this.curNode == this.targetNode)
            {
                Node targetCurNode = this.targetNode;
                while(targetCurNode != this.startNode)
                {
                    this.finalNodeList.Add(targetCurNode);
                    targetCurNode = targetCurNode.parentNode;
                }
                this.finalNodeList.Add(this.startNode);
                this.finalNodeList.Reverse();

                for(int i = 0; i < this.finalNodeList.Count; i++)
                {
                    Debug.LogFormat("{0}번째는 {1}, {2}", i, this.finalNodeList[i].x, this.finalNodeList[i].y);
                }
                return;
            }

            // ↗↖↙↘
            if (allowDiagonal)
            {
                AddOpenList(this.curNode.x + 1, this.curNode.y + 1);
                AddOpenList(this.curNode.x - 1, this.curNode.y + 1);
                AddOpenList(this.curNode.x - 1, this.curNode.y - 1);
                AddOpenList(this.curNode.x + 1, this.curNode.y - 1);
            }

            // ↑ → ↓ ←
            AddOpenList(this.curNode.x, this.curNode.y + 1);
            AddOpenList(this.curNode.x + 1, this.curNode.y);
            AddOpenList(this.curNode.x, this.curNode.y - 1);
            AddOpenList(this.curNode.x - 1, this.curNode.y);
        }
    }

    private void AddOpenList(int x, int y)
    {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면...
        if(x >= this.bottomLeft.x && x < this.topRight.x + 1 &&
            y >= this.bottomLeft.y && y < this.topRight.y + 1 &&
            !this.nodeArray[x-this.bottomLeft.x, y-this.bottomLeft.y].isWall &&
            !this.closedList.Contains(this.nodeArray[x-this.bottomLeft.x, y - this.bottomLeft.y]))
        {
            //대각선 허용시, 벽 사이로 통과 안됨
            if (this.allowDiagonal)
            {
                if (this.nodeArray[this.curNode.x - this.bottomLeft.x, y - this.bottomLeft.y].isWall &&
                    this.nodeArray[x - this.bottomLeft.x, this.curNode.y - this.bottomLeft.y].isWall)
                    return;
            }

            if (this.dontCrossCorner)
            {
                if (this.nodeArray[this.curNode.x - this.bottomLeft.x, y - this.bottomLeft.y].isWall ||
                    this.nodeArray[x - this.bottomLeft.x, this.curNode.y - this.bottomLeft.y].isWall)
                    return;
            }

            //이웃노드에 넣고, 직선은 10, 대각선은 14
            Node neighborNode = this.nodeArray[x - this.bottomLeft.x, y - this.bottomLeft.y];
            int moveCost = this.curNode.g + (this.curNode.x - x == 0 || this.curNode.y == 0 ? 10 : 14);

            // 이동비용이 이웃노드 G보다 작거나 또는 열린리스트에 이웃노드가 없으면, G, H, parentNode를 설정 후 열린리스트에 추가
            if(moveCost < neighborNode.g || !this.openList.Contains(neighborNode))
            {
                neighborNode.g = moveCost;
                neighborNode.h = (Mathf.Abs(neighborNode.x - this.targetNode.x) + Mathf.Abs(neighborNode.y - this.targetNode.y)) * 10;
                neighborNode.parentNode = this.curNode;

                this.openList.Add(neighborNode);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (this.finalNodeList.Count != 0)
        {
            for(int i = 0; i < this.finalNodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector2(this.finalNodeList[i].x, this.finalNodeList[i].y),
                    new Vector2(this.finalNodeList[i + 1].x, this.finalNodeList[i + 1].y));
            }
        }
    }
}
