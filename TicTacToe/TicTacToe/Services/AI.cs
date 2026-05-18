using System;

namespace TicTacToe.Services
{
    public class AI
    {
        public enum Difficulty { Easy, Medium, Hard }

        private readonly char _aiSymbol;
        private readonly char _humanSymbol;
        private readonly Random _rng = new();

        private readonly IMoveStrategy _strategy;

        public Difficulty Level { get; set; }

        public AI(char aiSymbol, char humanSymbol, Difficulty level = Difficulty.Hard)
        {
            _aiSymbol = aiSymbol;
            _humanSymbol = humanSymbol;
            Level = level;
            
            _strategy = level switch
            {
                Difficulty.Easy => new EasyStrategy(),
                Difficulty.Medium => new MediumStrategy(),
                _ => new HardStrategy()
            };
        }

        public int GetBestMove(char[,] board)
        {
            return _strategy.GetMove(this, board);
        }

        private interface IMoveStrategy
        {
            int GetMove(AI ai, char[,] board);
        }

        private class EasyStrategy : IMoveStrategy
        {
            public int GetMove(AI ai, char[,] board)
            {
                if (ai._rng.NextDouble() < 0.30)
                {
                    int block = ai.FindWinningMove(board, ai._humanSymbol);
                    if (block >= 0) return block;
                }
                return ai.RandomMove(board);
            }
        }

        private class MediumStrategy : IMoveStrategy
        {
            public int GetMove(AI ai, char[,] board)
            {
                int win = ai.FindWinningMove(board, ai._aiSymbol);
                if (win >= 0) return win;

                int block = ai.FindWinningMove(board, ai._humanSymbol);
                if (block >= 0) return block;

                if (ai._rng.NextDouble() < 0.50)
                    return ai.MinimaxMove(board, maxDepth: 3);

                return ai.RandomMove(board);
            }
        }

        private class HardStrategy : IMoveStrategy
        {
            public int GetMove(AI ai, char[,] board)
            {
                return ai.MinimaxMove(board, maxDepth: 9);
            }
        }

        private int MinimaxMove(char[,] board, int maxDepth)
        {
            int bestScore = int.MinValue;
            int bestMove = -1;
            int[] order = ShuffledOrder();

            foreach (int i in order)
            {
                int row = i / 3, col = i % 3;
                if (board[row, col] != ' ') continue;

                board[row, col] = _aiSymbol;
                int score = Minimax(board, 0, false, int.MinValue, int.MaxValue, maxDepth);
                board[row, col] = ' ';

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }
            return bestMove;
        }

        private int Minimax(char[,] board, int depth, bool isMaximizing, int alpha, int beta, int maxDepth)
        {
            char winner = CheckWinner(board);
            if (winner == _aiSymbol) return 10 - depth;
            if (winner == _humanSymbol) return depth - 10;
            if (IsBoardFull(board)) return 0;
            if (depth >= maxDepth) return Evaluate(board);

            if (isMaximizing)
            {
                int best = int.MinValue;
                for (int i = 0; i < 9; i++)
                {
                    int r = i / 3, c = i % 3;
                    if (board[r, c] != ' ') continue;
                    board[r, c] = _aiSymbol;
                    best = Math.Max(best, Minimax(board, depth + 1, false, alpha, beta, maxDepth));
                    board[r, c] = ' ';
                    alpha = Math.Max(alpha, best);
                    if (beta <= alpha) break;
                }
                return best;
            }
            else
            {
                int best = int.MaxValue;
                for (int i = 0; i < 9; i++)
                {
                    int r = i / 3, c = i % 3;
                    if (board[r, c] != ' ') continue;
                    board[r, c] = _humanSymbol;
                    best = Math.Min(best, Minimax(board, depth + 1, true, alpha, beta, maxDepth));
                    board[r, c] = ' ';
                    beta = Math.Min(beta, best);
                    if (beta <= alpha) break;
                }
                return best;
            }
        }

        private int Evaluate(char[,] board)
        {
            return CountOpenLines(board, _aiSymbol) - CountOpenLines(board, _humanSymbol);
        }

        private int CountOpenLines(char[,] board, char symbol)
        {
            int count = 0;
            int[][] lines = new int[][] {
                new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
                new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
                new[] {0,4,8}, new[] {2,4,6}
            };
            char opp = symbol == _aiSymbol ? _humanSymbol : _aiSymbol;
            foreach (var line in lines)
            {
                bool blocked = false;
                int own = 0;
                foreach (int idx in line)
                {
                    char cell = board[idx / 3, idx % 3];
                    if (cell == opp) { blocked = true; break; }
                    if (cell == symbol) own++;
                }
                if (!blocked) count += own + 1;
            }
            return count;
        }

        private int FindWinningMove(char[,] board, char symbol)
        {
            for (int i = 0; i < 9; i++)
            {
                int r = i / 3, c = i % 3;
                if (board[r, c] != ' ') continue;
                board[r, c] = symbol;
                bool wins = CheckWinner(board) == symbol;
                board[r, c] = ' ';
                if (wins) return i;
            }
            return -1;
        }

        private int RandomMove(char[,] board)
        {
            int[] empty = new int[9];
            int count = 0;
            for (int i = 0; i < 9; i++)
                if (board[i / 3, i % 3] == ' ') empty[count++] = i;
            return count > 0 ? empty[_rng.Next(count)] : -1;
        }

        private int[] ShuffledOrder()
        {
            int[] order = {0, 1, 2, 3, 4, 5, 6, 7, 8};
            for (int i = 8; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            return order;
        }

        private char CheckWinner(char[,] board)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i,0] != ' ' && board[i,0] == board[i,1] && board[i,1] == board[i,2])
                    return board[i,0];
                if (board[0,i] != ' ' && board[0,i] == board[1,i] && board[1,i] == board[2,i])
                    return board[0,i];
            }
            if (board[0,0] != ' ' && board[0,0] == board[1,1] && board[1,1] == board[2,2])
                return board[0,0];
            if (board[0,2] != ' ' && board[0,2] == board[1,1] && board[1,1] == board[2,0])
                return board[0,2];
            return ' ';
        }

        private bool IsBoardFull(char[,] board)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i,j] == ' ') return false;
            return true;
        }
    }
}