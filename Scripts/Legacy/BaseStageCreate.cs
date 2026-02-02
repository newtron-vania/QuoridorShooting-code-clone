using System.Collections;
using System.Collections.Generic;

// 절차적 맵생성 코드입니다.
// 도전모드 개발 중 3개의 main path를 둔 ChallengeStageCreate.cs와 달리
// 트리 구조로 스테이지가 배치된 맵입니다.
// Legacy 폴더에 넣은 이유는 현재 사용되지는 않지만, 트리 구조의 맵이 나중에 참고될 여지 때문입니다.

// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
/*
 * 완전한트리구조를 만드는 클래스입니다.
 * 해당 클래스에선 root노드에서 얼마만큼 떨어져있냐를 기준으로 위치 정보를 반환합니다. 
 */
public class BaseStageCreate : StageCreateStyle
{

    private int _startChild;
    private int _maxFieldLevel;

    private class BaseStageNode
    {
        public int FieldLevel { get; private set; }
        public int Deviation { get; private set; }

        public List<int> PathList;

        public BaseStageNode(int fieldLevel, int deviation)
        {
            FieldLevel = fieldLevel;
            Deviation = deviation;

            PathList = new List<int>();
        }
    }

    private List<BaseStageNode> _nodeList = new List<BaseStageNode>();

    public BaseStageCreate(int maxField, int startChild)
    {
        _maxChild = 3;
        _startChild = startChild;
        _maxFieldLevel = maxField;
        Count = _maxFieldLevel * (2 * (_startChild + _maxFieldLevel - 1)) / 2;

        _nodeList.Add(new BaseStageNode(0, 0));

        GenerateStage();
    }

    public override int[] GetStagePos(int idx)
    {
        int[] pos = new int[2];
        pos[0] = GetNodeFieldLevel(idx);
        pos[1] = GetNodeDeviation(idx);

        return pos;
    }

    public override List<int> GetStagePathList(int idx)
    {
        return GetNodePathList(idx);
    }



    public override HashSet<int> GetAllStagePathSet(int idx)
    {
        HashSet<int> pathList = new HashSet<int>();


        Queue<int> childIdxQueue = new Queue<int>();

        foreach (int childIdx in GetNodePathList(idx))
        {
            childIdxQueue.Enqueue(childIdx);
            pathList.Add(childIdx);
        }

        while (childIdxQueue.Count > 0)
        {
            int curIdx = childIdxQueue.Dequeue();
            List<int> pathIdxList = GetNodePathList(curIdx);

            foreach (int childIdx in pathIdxList)
            {
                if (pathList.Contains(childIdx)) continue;
                childIdxQueue.Enqueue(childIdx);
                pathList.Add(childIdx);

            }

        }

        return pathList;
    }



    private int GetNodeFieldLevel(int idx)
    {
        return _nodeList[idx].FieldLevel;
    }

    private int GetNodeDeviation(int idx)
    {
        return _nodeList[idx].Deviation;
    }

    private List<int> GetNodePathList(int idx)
    {
        return _nodeList[idx].PathList;
    }


    /// <summary>
    /// 챕터의 스테이지 구조를 완전한 트리 구조로 생성합니다.
    /// </summary>
    private void GenerateStage()
    {
        int prevFieldLimitIdx = 1;

        for (int lv = 1; lv <= _maxFieldLevel; lv++)
        {
            int parentIdx = _nodeList.Count - prevFieldLimitIdx;

            int fieldLimit = _startChild + (lv - 1) * 2;


            //자식 노드들을 먼저 생성합니다
            for (int NodeCount = 0; NodeCount < fieldLimit; NodeCount++)
            {
                BaseStageNode node = new BaseStageNode(lv, -fieldLimit / 2 + NodeCount);
                _nodeList.Add(node);
            }

            //그 후 부모노드와 연결합니다.
            int childIdx = _nodeList.Count - fieldLimit;
            for (int parentCount = 0; parentCount < prevFieldLimitIdx; parentCount++)
            {
                for (int childCount = 0; childCount < _maxChild; childCount++)
                {
                    _nodeList[parentIdx + parentCount].PathList.Add(childIdx + childCount);
                }
                childIdx++;
            }
            prevFieldLimitIdx = fieldLimit;
        }

    }


}
