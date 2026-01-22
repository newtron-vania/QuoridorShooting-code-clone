using System.Collections;
using System.Collections.Generic;

// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
/*
 * 챕터의 구조를 생성하는 토대입니다.
 * 외부에서는 이 클래스의 함수를 통해서만 정보를 주고 받습니다.
 * 외부에서 필요한 정보가 추가로 있다면 이 클래스를 편집하면 됩니다.
 * 다만 편집시 모든 자식 클래스에도 편집이 필요합니다.
 */
public abstract class StageCreateStyle
{
    protected int _maxChild;

    protected readonly int _seed;
    public int Count { get; protected set; }

    //자식간의 사이각
    public float ChildAngle => 180f / (_maxChild + 1);

    /// <summary>
    /// 스테이지의 위치정보를 반환합니다.
    /// </summary>
    /// <returns>
    /// 0: level, y, 필드레벨
    /// 1: deviation, x, 루트로부터 떨어진 거리
    /// </returns>
    public abstract int[] GetStagePos(int idx);


    /// <summary>
    /// 스테이지와 이어진 스테이지 인덱스를 반환합니다.
    /// </summary>
    public abstract List<int> GetStagePathList(int idx);

    /// <summary>
    /// 스테이지를 통해 갈 수 있는 모든 스테이지 인덱스 집합을 반환합니다.
    /// </summary>
    public abstract HashSet<int> GetAllStagePathSet(int idx);



}
