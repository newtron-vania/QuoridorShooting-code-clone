using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 절차적 맵 구현 feat: Add Challenge Stage Create
// 3 Main Path와 확률적으로 생성되는 Branch Path
/*
 * StageCreateStyle을 상속 받은 BaseStageCreate.cs에서 트리 구조로 타일이 뻗어
 * 여기서는 똑같이 상속 받아 3개의 메인 Main Path(column)으로 수정, 각각 col1 col2 col3
 * x좌표는 -1, 0, 1로 표현
*/
public class ChallengeStageCreate : StageCreateStyle
{
    private const int MAIN_PATH_COUNT = 3;
    private const int FINAL_FLOOR = 10;
    
    /*
     * 매 챕터마다의 맵 생성을 위한 ChallengeStageNode(연결 정보, 시각화)
     * 실제 게임은 Stage에서 담당(타일 타입, 클리어 유무 등)
     * StageSelect.cs에서 두 클래스가 연동되어 사용됩니다.
     */
    private class ChallengeStageNode
    {
        public int Floor;
        public int Column;
        public List<int> NextStageIndices; // 연결된 노드의 인덱스 목록

        public ChallengeStageNode(int floor, int col)
        {
            Floor = floor;
            Column = col;
            NextStageIndices = new List<int>();
        }
    }

    private List<ChallengeStageNode> _nodeList = new List<ChallengeStageNode>();

    public ChallengeStageCreate(int seed)
    {
        _maxChild = 3;
        Seed = seed; //Random Branch 확인용 임시 변수
        //readonly, 디버깅을 위해 모든 seed는 동일하게 유지
        Count = ((FINAL_FLOOR - 1) * MAIN_PATH_COUNT) + 1; //3*9+1(보스)
        
        //_nodeList.Add(new ChallengeStageNode(0, 0));
        /*
        * Main Path를 선택하는 노드는 따로 필요 없음. 아무런 이벤트가 없기 때문.
        * 추후에 해당 상태를 시각적으로 표현하게 될 경우를 위해 남겨둡니다.
        * -> GetIndex, GetStagePos 같이 수정 필요.
        */
        GenerateMapStructure();
    }

    private int GetIndex(int floor, int col)
    {
        //처음 Main Path를 선택하는 노드가 따로 추가된다면 수정 필요
        return (floor - 1) * MAIN_PATH_COUNT + col;
    }

    //단순히 위치만 조정하는 게 아니라 CreateStage() 로직에도 사용되지 수정하게 된다면 아주 주의할 것!!!
    public override int[] GetStagePos(int idx)
    {
        // 외부에서 들어오는 idx는 1부터 시작하므로 -1 하여 접근
        if (idx < 1 || idx > _nodeList.Count)
            return new int[] { 0, 0 };
        //처음 Main Path를 선택하는 노드가 따로 추가된다면 수정 필요
        int[] pos = new int[2];
        pos[0] = _nodeList[idx-1].Floor;
        pos[1] = _nodeList[idx-1].Column - 1; //0,1,2 -> -1,0,1
        return pos;
    }

    public override List<int> GetStagePathList(int idx)
    {
        if (idx < 1 || idx > _nodeList.Count)
            return new List<int>();
        return GetNodePathList(idx);
    }

    public override HashSet<int> GetAllStagePathSet(int idx)
    {
        //BaseStageCreate처럼 BFS로 탐색
        HashSet<int> pathList = new HashSet<int>();
        if (idx < 1 || idx > _nodeList.Count)
            return pathList;
        Queue<int> queue = new Queue<int>();

        //바로 다음 노드
        foreach (int childIdx in GetNodePathList(idx))
        {
            queue.Enqueue(childIdx);
            pathList.Add(childIdx);
        }

        //큐가 빌 때까지 반복
        while (queue.Count > 0)
        {
            int curIdx = queue.Dequeue();
            List<int> nextPaths = GetNodePathList(curIdx);

            foreach (int nextIdx in nextPaths)
            {
                if (!pathList.Contains(nextIdx))
                {
                    pathList.Add(nextIdx);
                    queue.Enqueue(nextIdx);
                }
            }
        }

        return pathList;
    }

    private List<int> GetNodePathList(int idx)
    {
        if (idx < 1 || idx > _nodeList.Count)
            return new List<int>();
        return _nodeList[idx - 1].NextStageIndices;
    }

    private void GenerateMapStructure()
    {
        Random.InitState(Seed); //검색용 주석: 절차적맵 생성시드, seed, 시드

        // 1~9
        for (int floor = 1; floor < FINAL_FLOOR; floor++)
        {
            for (int col = 0; col < MAIN_PATH_COUNT; col++)
            {
                _nodeList.Add(new ChallengeStageNode(floor, col));
            }
        }

        // 10 보스 중앙 Main Path
        _nodeList.Add(new ChallengeStageNode(FINAL_FLOOR, 1));

        ConnectPaths(); //노드 연결 작업
    }

    private void ConnectPaths()
    {
        //1층부터 8층까지 연결
        for (int floor = 1; floor < FINAL_FLOOR - 1; floor++)
        {
            for (int col = 0; col < MAIN_PATH_COUNT; col++)
            {
                int currentIdx = GetIndex(floor, col);
                ChallengeStageNode currentNode = _nodeList[currentIdx];

                //Main Path : 바로 부모 노드와 연결 
                int mainPathIdx = GetIndex(floor + 1, col);
                currentNode.NextStageIndices.Add(mainPathIdx + 1);

                //Branch Path : 50% 확률로 연결, 왼쪽 또는 오른쪽 대각선으로
                if (col != 0 && Random.value < 0.5f) 
                {
                    int leftBranchIdx = GetIndex(floor + 1, col - 1);
                    currentNode.NextStageIndices.Add(leftBranchIdx + 1);
                }
                if (col != (MAIN_PATH_COUNT - 1) && Random.value < 0.5f)
                {
                    int rightBranchIdx = GetIndex(floor + 1, col + 1);
                    currentNode.NextStageIndices.Add(rightBranchIdx + 1);
                }
            }
        }

        //9층에서 10층
        // 9층의 모든 노드는 10층(보스)으로 연결되어야 함 
        int bossIdx = _nodeList.Count;
        for (int col = 0; col < MAIN_PATH_COUNT; col++)
        {
            int currentIdx = GetIndex(FINAL_FLOOR - 1, col);
            _nodeList[currentIdx].NextStageIndices.Add(bossIdx);
        }
    }
}