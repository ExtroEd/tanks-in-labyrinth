namespace Client.Logic;

public class LabyrinthGenerator(int width, int height)
{
    private readonly Random _random = new();

    public List<(int x1, int y1, int x2, int y2)> Generate()
    {
        var passages = new List<(int x1, int y1, int x2, int y2)>();
        var visited = new bool[width, height];
        var wallCandidates = new List<(int x1, int y1, int x2, int y2)>();

        const int startX = 0;
        const int startY = 0;
        visited[startX, startY] = true;

        AddNeighbors(startX, startY, visited, wallCandidates);

        while (wallCandidates.Count > 0)
        {
            var index = _random.Next(wallCandidates.Count);
            var wall = wallCandidates[index];
            wallCandidates.RemoveAt(index);

            if (visited[wall.x2, wall.y2]) continue;

            visited[wall.x2, wall.y2] = true;
            passages.Add(wall);

            AddNeighbors(wall.x2, wall.y2, visited, wallCandidates);
        }

        RemoveRandomWalls(passages, 0.05);

        return passages;
    }

    private void RemoveRandomWalls(List<(int x1, int y1, int x2, int y2)> passages, double chance)
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (x + 1 < width)
                {
                    var wall = (x, y, x + 1, y);
                    if (!passages.Contains(wall) && !passages.Contains((x + 1, y, x, y)) && _random.NextDouble() < chance)
                    {
                        passages.Add(wall);
                    }
                }

                if (y + 1 >= height) continue;
                {
                    var wall = (x, y, x, y + 1);
                    if (!passages.Contains(wall) && !passages.Contains((x, y + 1, x, y)) && _random.NextDouble() < chance)
                    {
                        passages.Add(wall);
                    }
                }
            }
        }
    }

    private void AddNeighbors(int x, int y, bool[,] visited, List<(int x1, int y1, int x2, int y2)> candidates)
    {
        int[] dx = [0, 0, 1, -1];
        int[] dy = [1, -1, 0, 0];

        for (var i = 0; i < 4; i++)
        {
            var nx = x + dx[i];
            var ny = y + dy[i];

            if (nx >= 0 && ny >= 0 && nx < width && ny < height && !visited[nx, ny])
            {
                candidates.Add((x, y, nx, ny));
            }
        }
    }
}
