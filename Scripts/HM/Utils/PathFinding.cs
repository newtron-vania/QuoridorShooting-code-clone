using System.Collections.Generic;
using System.Linq;
using HM.Containers;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace HM
{
    namespace Utils
    {
        public static class PathFindingUtils
        {
            private enum Direction
            {
                Up = 0,
                Down = 1,
                Left = 2,
                Right = 3,
            }

            private static bool[,,] mapGraph;

            private static Vector2Int[] defaultDirections =
                { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            //TODO: EnemyValues를 나중에 바뀔 CharacterStatus 같은 새로운 구초체가 나올 경우 변경 필요.
            // 벽과 캐릭터의 위치를 받아서 맵 그래프(9x9x4)를 만드는 함수
            public static bool[,,] GetMapGraph(List<Vector2Int> characterPositions, List<WallData> walls,
                bool canThrounghCharacter = false)
            {
                mapGraph = new bool[9, 9, 4]; // 9x9 맵, 4방향
                for (var x = 0; x < 9; x++)
                for (var y = 0; y < 9; y++)
                for (var direction = 0; direction < 4; direction++)
                    mapGraph[x, y, direction] = true;
                foreach (var wall in walls) SetWallData(wall);
                if (canThrounghCharacter) return mapGraph;
                foreach (var characterPosition in characterPositions)
                    SurroundWallToPosition(characterPosition + new Vector2Int(4, 4));

                return mapGraph;
            }

            public static void SetWallData(WallData wall)
            {
                // Debug.LogWarning($"[INFO] Pathfinding::SetWallData : wall Position : {wall.Position.x}, {wall.Position.y}");

                if (wall.IsHorizontal)
                {
                    TrySetMapGraph(wall.Position.x + 4, wall.Position.y + 4, 0, false);
                    TrySetMapGraph(wall.Position.x + 5, wall.Position.y + 4, 0, false);
                    TrySetMapGraph(wall.Position.x + 4, wall.Position.y + 5, 1, false);
                    TrySetMapGraph(wall.Position.x + 5, wall.Position.y + 5, 1, false);
                }
                else
                {
                    TrySetMapGraph(wall.Position.x + 4, wall.Position.y + 4, 3, false);
                    TrySetMapGraph(wall.Position.x + 4, wall.Position.y + 5, 3, false);
                    TrySetMapGraph(wall.Position.x + 5, wall.Position.y + 4, 2, false);
                    TrySetMapGraph(wall.Position.x + 5, wall.Position.y + 5, 2, false);
                }
            }

            public static void SurroundWallToPosition(Vector2Int position)
            {
                // Debug.LogWarning($"[INFO] Pathfinding::SurroundWallToPosition : SurroundWallToPosition : {position.x}, {position.y}");

                TrySetMapGraph(position.x, position.y, 0, false);
                TrySetMapGraph(position.x, position.y + 1, 1, false);
                TrySetMapGraph(position.x, position.y, 1, false);
                TrySetMapGraph(position.x, position.y - 1, 0, false);

                TrySetMapGraph(position.x, position.y, 2, false);
                TrySetMapGraph(position.x - 1, position.y, 3, false);
                TrySetMapGraph(position.x, position.y, 3, false);
                TrySetMapGraph(position.x + 1, position.y, 2, false);
            }

            // 유효성 검사 및 안전한 값 설정을 위한 헬퍼 메서드
            private static void TrySetMapGraph(int x, int y, int direction, bool value)
            {
                var pos = new Vector2Int(x, y);
                if (IsPositionValid(pos))
                {
                    mapGraph[x, y, direction] = value;
                }
                // Debug.LogWarning($"[Warning] Pathfinding::TrySetMapGraph: Invalid position: ({x}, {y}, {direction})");
            }

            // 목표지점으로 도달 가능한지 확인하는 함수 (using DFS)
            // 제한된 거리 추가
            public static bool CanReach(bool[,,] tileGraph, Vector2Int start, Vector2Int goal, int limit = 999,
                bool isExternal = false)
            {
                if (isExternal)
                {
                    Vector2Int offset = new Vector2Int(4, 4);
                    start += offset;
                    goal += offset;
                }

                bool[,] visited = new bool[9, 9];

                return BFS(tileGraph, ref visited, start, goal, limit);
            }

            public static bool CanReachWithoutStuckInWall(bool[,,] tileGraph, Vector2Int start, Vector2Int goal,
                bool isExternal = false)
            {
                // 1. 좌표계 변환 및 유효성 검사
                if (isExternal)
                {
                    var offset = new Vector2Int(4, 4);
                    start += offset;
                    goal += offset;
                }

                if (!IsPositionValid(start) || !IsPositionValid(goal))
                    return false;

                if (start == goal)
                    return true;

                // 2. 브레제넘 알고리즘 준비 (좌->우 순회 보정)
                if (start.x > goal.x) (start, goal) = (goal, start);

                int x0 = start.x, y0 = start.y, x1 = goal.x, y1 = goal.y;

                int dx = x1 - x0;
                // Bresenham's algorithm requires dy to be negative for correct error calculation.
                int dy = -(y1 - y0 >= 0 ? y1 - y0 : y0 - y1);
                int sx = 1; // x는 항상 증가하므로 1로 고정
                int sy = y0 < y1 ? 1 : -1;
                int error = dx + dy;

                // 3. 브레제넘 알고리즘 실행
                while (true)
                {
                    if (x0 == x1 && y0 == y1) break;

                    int prevX = x0, prevY = y0;
                    int e2 = 2 * error;

                    if (e2 >= dy)
                    {
                        if (x0 == x1) break;
                        error += dy;
                        x0 += sx;
                    }

                    if (e2 <= dx)
                    {
                        if (y0 == y1) break;
                        error += dx;
                        y0 += sy;
                    }

                    // 4. 이동 경로의 벽 검사를 별도 메서드로 위임
                    if (IsMovementBlocked(tileGraph, prevX, prevY, x0, y0, sy, dx, dy, error, e2))
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// 두 타일 간의 이동이 벽에 막히는지 검사합니다. (기존 로직 유지)
            /// </summary>
            private static bool IsMovementBlocked(bool[,,] tileGraph, int prevX, int prevY, int currentX, int currentY,
                int sy, int dx, int dy, int error, int e2)
            {
                // 이동이 없으면 막히지 않음
                if (prevX == currentX && prevY == currentY) return false;

                // 방향 변수화로 코드 통합
                var primaryVerticalDir = sy > 0 ? Direction.Up : Direction.Down;
                var oppositeVerticalDir = sy > 0 ? Direction.Down : Direction.Up;

                // 수직 이동 검사
                if (currentX == prevX && currentY != prevY)
                {
                    if (!tileGraph[prevX, prevY, (int)primaryVerticalDir]) return true;
                }
                // 수평 이동 검사 (항상 오른쪽으로만 이동)
                else if (currentX != prevX && currentY == prevY)
                {
                    if (!tileGraph[prevX, prevY, (int)Direction.Right]) return true;
                }
                // 대각선 이동 검사 (기존 로직 보존)
                else if (currentX != prevX && currentY != prevY)
                {
                    int mid = dx + dy;
                    if (mid == e2)
                    {
                        bool right = !tileGraph[prevX, prevY, (int)Direction.Right];
                        bool vertical = !tileGraph[prevX, prevY, (int)primaryVerticalDir];
                        bool left = !tileGraph[currentX, currentY, (int)Direction.Left];
                        bool oppositeVertical = !tileGraph[currentX, currentY, (int)oppositeVerticalDir];

                        if ((vertical && right) || (oppositeVertical && left) || (left && right) ||
                            (vertical && oppositeVertical)) return true;
                    }
                    else if (mid == error)
                    {
                        if (Mathf.Abs(dx) > Mathf.Abs(dy))
                        {
                            bool vertical = !tileGraph[prevX, prevY, (int)primaryVerticalDir];
                            bool left = !tileGraph[currentX, currentY, (int)Direction.Left];
                            if (vertical || left) return true;
                        }
                        else
                        {
                            bool right = !tileGraph[prevX, prevY, (int)Direction.Right];
                            bool oppositeVertical = !tileGraph[currentX, currentY, (int)oppositeVerticalDir];
                            if (right || oppositeVertical) return true;
                        }
                    }
                    else if (mid > error)
                    {
                        bool vertical = !tileGraph[prevX, prevY, (int)primaryVerticalDir];
                        bool left = !tileGraph[currentX, currentY, (int)Direction.Left];
                        if (vertical || left) return true;
                    }
                    else // mid < error
                    {
                        bool right = !tileGraph[prevX, prevY, (int)Direction.Right];
                        bool oppositeVertical = !tileGraph[currentX, currentY, (int)oppositeVerticalDir];
                        if (right || oppositeVertical) return true;
                    }
                }

                return false; // 경로가 막히지 않음
            }

            public static bool CheckStuck(bool[,,] tileGraph)
            {
                var visited = new bool[9, 9];
                // DFS(tileGraph, visited, new Vector2Int(0, 0), new Vector2Int(-1, -1), 999, 0, false);
                BFS(tileGraph, ref visited, new Vector2Int(0, 0), new Vector2Int(-1, -1), 999);
                for (var x = 0; x < 9; x++)
                for (var y = 0; y < 9; y++)
                    if (!visited[x, y])
                        return true;
                return false;
            }

            private static bool BFS(bool[,,] tileGraph, ref bool[,] visited, Vector2Int start, Vector2Int goal,
                int limit)
            {
                visited = new bool[tileGraph.GetLength(0), tileGraph.GetLength(1)];
                Queue<(Vector2Int position, int depth)> queue = new();
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                queue.Enqueue((start, 0));
                visited[start.x, start.y] = true;

                while (queue.Count > 0)
                {
                    var (now, depth) = queue.Dequeue();

                    if (now == goal) return true;
                    if (depth >= limit) continue;

                    for (var i = 0; i < directions.Length; i++)
                    {
                        var next = now + directions[i];

                        if (!IsPositionValid(next)) // 범위를 벗어난 경우
                            continue;
                        if (!tileGraph[now.x, now.y, i]) // 벽이 있는 경우
                            continue;
                        if (visited[next.x, next.y]) // 이미 방문한 경우
                            continue;

                        visited[next.x, next.y] = true;
                        queue.Enqueue((next, depth + 1));
                    }
                }

                return false;
            }


            // A* 알고리즘을 이용하여 경로를 찾는 함수
            public static List<Vector2Int> FindPath(bool[,,] tileGraph, Vector2Int start, Vector2Int goal,
                List<Vector2Int> movables = null)
            {
                return FindPath(tileGraph, start, new List<Vector2Int> { goal }, movables);
            }

            // A* 알고리즘을 이용하여 경로를 찾는 함수
            // 탐색 실패 시, 이동 가능한 경로 중 목표 지점에서 최근접한 경로를 반환
            /*
             탐색조건
             start - 시작지점
             goals - 목표지머(복수)
             directions - 이동 가능 방향
             */
            public static List<Vector2Int> FindPath(bool[,,] tileGraph, Vector2Int start, List<Vector2Int> goals,
                List<Vector2Int> movables = null)
            {
                var directions = movables?.ToArray();
                var openList = new List<Path>();
                var closedList = new HashSet<Path>();

                var offset = new Vector2Int(4, 4);
                start += offset;
                for (var i = 0; i < goals.Count; i++) goals[i] += offset;

                var startPath = new Path(start);
                var goalPaths = new List<Path>();

                foreach (var goal in goals) goalPaths.Add(new Path(goal));
                openList.Add(startPath);

                // 목표에 도달하지 못했을 때 가장 가까운 노드를 저장할 변수
                Path closestPath = null;
                var minHeuristic = float.MaxValue; // 최소 휴리스틱 값

                while (openList.Count > 0)
                {
                    var currentPath = openList.OrderBy(path => path.F).First();

                    if (goals.Contains(currentPath.Position)) return RetracePath(startPath, currentPath, offset);

                    openList.Remove(currentPath);
                    closedList.Add(currentPath);

                    // 목표와의 가장 가까운 위치 업데이트
                    var currentH = goals.Min(goal => MathUtils.GetManhattaDistance(currentPath.Position, goal));
                    if (currentH < minHeuristic)
                    {
                        minHeuristic = currentH;
                        closestPath = currentPath;
                    }

                    foreach (var neighborPosition in GetNeighbors(tileGraph, currentPath.Position, directions))
                    {
                        if (closedList.Any(path => path.Position == neighborPosition))
                            continue;

                        var newMovementCostToNeighbor = currentPath.G + 1;
                        var neighborPath = openList.FirstOrDefault(path => path.Position == neighborPosition);

                        if (neighborPath == null || newMovementCostToNeighbor < neighborPath.F)
                        {
                            if (neighborPath == null)
                            {
                                neighborPath = new Path(neighborPosition);
                                openList.Add(neighborPath);
                            }

                            neighborPath.G = newMovementCostToNeighbor;
                            float minH = 999;
                            foreach (var goal in goals)
                                minH = Mathf.Min(minH, MathUtils.GetManhattaDistance(neighborPosition, goal));
                            neighborPath.H = minH;
                            neighborPath.ParentPath = currentPath;
                        }
                    }
                }

                // 탐색 실패 시, 가장 가까운 노드까지의 경로 반환
                return closestPath != null
                    ? RetracePath(startPath, closestPath, offset)
                    : new List<Vector2Int> { start };
            }

            // 현재 위치에서 [이동 가능]한 이웃들을 반환하는 함수
            private static List<Vector2Int> GetNeighbors(bool[,,] tileGraph, Vector2Int pathPosition,
                Vector2Int[] directions = null)
            {
                var neighbors = new List<Vector2Int>();
                if (directions == null)
                    directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                for (var i = 0; i < directions.Length; i++)
                {
                    var neighborPosition = pathPosition + directions[i];
                    if (!IsPositionValid(neighborPosition)) continue;
                    if (CanReachWithoutStuckInWall(tileGraph, pathPosition, neighborPosition))
                        neighbors.Add(neighborPosition);
                }

                return neighbors;
            }

            public static bool IsPositionValid(Vector2Int position, int mapWidth = 9, int mapHeight = 9)
            {
                return position.x >= 0 && position.x < mapWidth && position.y >= 0 && position.y < mapHeight;
            }

            public static bool IsPositionValidWithOffset(Vector2Int position, int mapWidth = 9, int mapHeight = 9)
            {
                var checkedPos = position + new Vector2Int(mapWidth / 2, mapHeight / 2);
                return checkedPos.x >= 0 && checkedPos.x < mapWidth && checkedPos.y >= 0 && checkedPos.y < mapHeight;
            }

            // 경로를 정리하고 그리드 좌표로 변환하는 함수
            private static List<Vector2Int> RetracePath(Path startPath, Path endPath, Vector2Int offset)
            {
                var path = new List<Vector2Int>();
                var currentPath = endPath;

                while (currentPath != startPath)
                {
                    path.Add(currentPath.Position - offset);
                    currentPath = currentPath.ParentPath;
                }

                path.Reverse();
                return path;
            }
        }
    }
}