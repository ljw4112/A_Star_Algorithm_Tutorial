using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node
{
    public int x, y, g, h;
    // g : �������κ��� �̵��ߴ� �Ÿ�
    // h : |����| + |����| ��ֹ� �����Ͽ� ��ǥ������ �Ÿ�
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
        //NodeArray�� ũ�⸦ �����ְ� isWall, x, y ����
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

        //���۰� �� ���, ��������Ʈ�� ��������Ʈ, ����������Ʈ �ʱ�ȭ
        this.startNode = this.nodeArray[this.startPos.x - this.bottomLeft.x, this.startPos.y - this.bottomLeft.y];
        this.targetNode = this.nodeArray[this.targetPos.x - this.bottomLeft.x, this.targetPos.y - this.bottomLeft.y];

        this.openList = new List<Node>() { this.startNode };
        this.closedList = new List<Node>();
        this.finalNodeList = new List<Node>();

        while (this.openList.Count > 0)
        {
            //��������Ʈ �� ���� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �ϰ�
            //��������Ʈ���� ��������Ʈ�� �ű��
            this.curNode = this.openList[0];
            for(int i = 1; i < this.openList.Count; i++)
            {
                if (this.openList[i].F <= this.curNode.F
                    && this.openList[i].h < this.curNode.h)
                    this.curNode = this.openList[i];
            }

            this.openList.Remove(this.curNode);
            this.closedList.Add(this.curNode);

            //������ ó��
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
                    Debug.LogFormat("{0}��°�� {1}, {2}", i, this.finalNodeList[i].x, this.finalNodeList[i].y);
                }
                return;
            }

            // �֢آע�
            if (allowDiagonal)
            {
                AddOpenList(this.curNode.x + 1, this.curNode.y + 1);
                AddOpenList(this.curNode.x - 1, this.curNode.y + 1);
                AddOpenList(this.curNode.x - 1, this.curNode.y - 1);
                AddOpenList(this.curNode.x + 1, this.curNode.y - 1);
            }

            // �� �� �� ��
            AddOpenList(this.curNode.x, this.curNode.y + 1);
            AddOpenList(this.curNode.x + 1, this.curNode.y);
            AddOpenList(this.curNode.x, this.curNode.y - 1);
            AddOpenList(this.curNode.x - 1, this.curNode.y);
        }
    }

    private void AddOpenList(int x, int y)
    {
        // �����¿� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�...
        if(x >= this.bottomLeft.x && x < this.topRight.x + 1 &&
            y >= this.bottomLeft.y && y < this.topRight.y + 1 &&
            !this.nodeArray[x-this.bottomLeft.x, y-this.bottomLeft.y].isWall &&
            !this.closedList.Contains(this.nodeArray[x-this.bottomLeft.x, y - this.bottomLeft.y]))
        {
            //�밢�� ����, �� ���̷� ��� �ȵ�
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

            //�̿���忡 �ְ�, ������ 10, �밢���� 14
            Node neighborNode = this.nodeArray[x - this.bottomLeft.x, y - this.bottomLeft.y];
            int moveCost = this.curNode.g + (this.curNode.x - x == 0 || this.curNode.y == 0 ? 10 : 14);

            // �̵������ �̿���� G���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ������, G, H, parentNode�� ���� �� ��������Ʈ�� �߰�
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
