using MineSweeper_MVC.Models;

namespace MineSweeper_MVC.Services
{
    public class CoreGameServices
    {
        // ------------------------------------------------------------
        // PUBLIC API — what the controller is allowed to call
        // ------------------------------------------------------------

        public Board.GameStatus ProcessCellClick(Board board, int row, int col)
        {
            var current = board.Cells[row, col];

            // --- 0. IGNORE INVALID OR POST-GAME CLICKS ----------------
            if (current.IsVisited || current.IsFlagged ||
                board.CurrentStatus != Board.GameStatus.InProgress)
            {
                return board.CurrentStatus;
            }

            // --- 1. HANDLE BOMB CLICK ---------------------------------
            if (current.IsBomb)
            {
                RevealAllBombs(board);
                board.CurrentStatus = Board.GameStatus.Lost;
                board.EndTime = DateTime.UtcNow;
                return Board.GameStatus.Lost;
            }

            // --- 2. HANDLE SAFE TILE ----------------------------------
            current.IsVisited = true;

            if (current.NumberOfBombNeighbors == 0)
                FloodFill(board, row, col);

            // --- 3. CHECK FOR WIN -------------------------------------
            var state = board.DetermineGameState();
            board.CurrentStatus = state;

            if (state == Board.GameStatus.Won)
            {
                RevealEntireBoard(board);
                board.EndTime = DateTime.UtcNow;
            }

            return state;
        }

        public void ToggleFlag(Board board, int row, int col)
        {
            var cell = board.Cells[row, col];

            if (cell.IsVisited || board.CurrentStatus != Board.GameStatus.InProgress)
                return;

            cell.IsFlagged = !cell.IsFlagged;
        }

        // ------------------------------------------------------------
        // PRIVATE GAME RULE HELPERS - For CoreGameServices Only
        // ------------------------------------------------------------

        private void FloodFill(Board board, int row, int col)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int newRow = row + dr;
                    int newCol = col + dc;

                    if (!board.IsCellOnBoard(newRow, newCol))
                        continue;

                    var neighbor = board.Cells[newRow, newCol];

                    if (!neighbor.IsVisited && !neighbor.IsBomb)
                    {
                        neighbor.IsVisited = true;

                        if (neighbor.NumberOfBombNeighbors == 0)
                            FloodFill(board, newRow, newCol);
                    }
                }
            }
        }

        private void RevealAllBombs(Board board)
        {
            for (int r = 0; r < board.Size; r++)
            {
                for (int c = 0; c < board.Size; c++)
                {
                    if (board.Cells[r, c].IsBomb)
                        board.Cells[r, c].IsVisited = true;
                }
            }
        }

        private void RevealEntireBoard(Board board)
        {
            for (int r = 0; r < board.Size; r++)
            {
                for (int c = 0; c < board.Size; c++)
                {
                    board.Cells[r, c].IsVisited = true;
                }
            }
        }
    }
}
