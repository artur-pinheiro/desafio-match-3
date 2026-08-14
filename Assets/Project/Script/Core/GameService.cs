using Gazeus.DesafioMatch3.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Core {
    public class GameService {

        public enum GameType { 
            Match3 = 3,
            Match4 = 4            
        }

        [SerializeField][Range(0, 1)] private float _specialTileRate = 0.05f;

        private List<List<Tile>> _boardTiles;
        private List<int> _tilesTypes;
        private List<int> _specialTypes;
        private int _tileCount;
        private GameType _gameType;

        private readonly int ROWBREAKER = 4;
        private readonly int COLUMNBREAKER = 5;
        private readonly int BOMB = 6;

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    switch ( _gameType ) {
                        case GameType.Match3:
                            if ( x > 1 && 
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x - 1].Type == newBoard[y][x - 2].Type ) {

                                return true;
                            }

                            if ( y > 1 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y - 1][x].Type == newBoard[y - 2][x].Type ) {

                                return true;
                            }
                            break;
                        case GameType.Match4:
                            if ( x > 0 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type ) {

                                if ( y > 0 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type ) {

                                    if ( newBoard[y][x].Type == newBoard[y - 1][x - 1].Type ) {

                                        return true;
                                    }
                                }
                            }

                            
                            break;
                        default:
                            return false;                            
                    }
                    
                }
            }

            return false;
        }

        public List<List<Tile>> StartGame(int boardWidth, int boardHeight)        {
            _gameType = (GameType)PlayerPrefs.GetInt("GameMode", 3);

            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _specialTypes = new List<int> { 4, 5, 6 };
            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            return _boardTiles;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);

            List<BoardSequence> boardSequences = new();
            List<List<bool>> matchedTiles = FindMatches(newBoard);

            while (HasMatch(matchedTiles))
            {
                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new();
                for (int y = 0; y < newBoard.Count; y++)
                {
                    for (int x = 0; x < newBoard[y].Count; x++)
                    {
                        if (matchedTiles[y][x])
                        {
                            matchedPosition.Add(new Vector2Int(x, y));
                            newBoard[y][x] = new Tile { Id = -1, Type = -1 };
                        }
                    }
                }

                // Dropping the tiles
                Dictionary<int, MovedTileInfo> movedTiles = new();
                List<MovedTileInfo> movedTilesList = new();
                for (int i = 0; i < matchedPosition.Count; i++)
                {
                    int x = matchedPosition[i].x;
                    int y = matchedPosition[i].y;
                    if (y > 0)
                    {
                        for (int j = y; j > 0; j--)
                        {
                            Tile movedTile = newBoard[j - 1][x];
                            newBoard[j][x] = movedTile;
                            if (movedTile.Type > -1)
                            {
                                if (movedTiles.ContainsKey(movedTile.Id))
                                {
                                    movedTiles[movedTile.Id].To = new Vector2Int(x, j);
                                }
                                else
                                {
                                    MovedTileInfo movedTileInfo = new()
                                    {
                                        From = new Vector2Int(x, j - 1),
                                        To = new Vector2Int(x, j)
                                    };
                                    movedTiles.Add(movedTile.Id, movedTileInfo);
                                    movedTilesList.Add(movedTileInfo);
                                }
                            }
                        }

                        newBoard[0][x] = new Tile
                        {
                            Id = -1,
                            Type = -1
                        };
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new();
                for (int y = newBoard.Count - 1; y > -1; y--)
                {
                    for (int x = newBoard[y].Count - 1; x > -1; x--)
                    {
                        if (newBoard[y][x].Type == -1)
                        {
                            int tileType = Random.Range(0, _tilesTypes.Count);
                            Tile tile = newBoard[y][x];
                            tile.Id = _tileCount++;
                            tile.Type = _gameType == GameType.Match3 ? TrySwapToSpecial(tileType) : tileType;
                            addedTiles.Add(new AddedTileInfo
                            {
                                Position = new Vector2Int(x, y),
                                Type = tile.Type
                            });
                        }
                    }
                }

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles
                };
                boardSequences.Add(sequence);
                matchedTiles = FindMatches(newBoard);
            }

            _boardTiles = newBoard;

            return boardSequences;
        }

        private List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
        {
            List<List<Tile>> newBoard = new(boardToCopy.Count);
            for (int y = 0; y < boardToCopy.Count; y++)
            {
                newBoard.Add(new List<Tile>(boardToCopy[y].Count));
                for (int x = 0; x < boardToCopy[y].Count; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y].Add(new Tile { Id = tile.Id, Type = tile.Type });
                }
            }

            return newBoard;
        }

        private int TrySwapToSpecial(int tileType) {
            if (Random.Range(0f,1f) <= _specialTileRate) {
                return _specialTypes[Random.Range(0, _specialTypes.Count)];
            }
            return tileType;
        }

        private bool IsSpecialTile(int tileType) {
            if (tileType == COLUMNBREAKER ||
                tileType == ROWBREAKER ||
                tileType == BOMB) {
                return true;
            }
            return false;
        }

        private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
        {
            List<List<Tile>> board = new(height);
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board.Add(new List<Tile>(width));
                for (int x = 0; x < width; x++)
                {
                    board[y].Add(new Tile { Id = -1, Type = -1 });
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    if (x > 1 &&
                        board[y][x - 1].Type == board[y][x - 2].Type)
                    {
                        noMatchTypes.Remove(board[y][x - 1].Type);
                    }

                    if (y > 1 &&
                        board[y - 1][x].Type == board[y - 2][x].Type)
                    {
                        noMatchTypes.Remove(board[y - 1][x].Type);
                    }

                    board[y][x].Id = _tileCount++;
                    
                    if (_gameType == GameType.Match3) {
                        //Check if this tile will be replaced by a Special Tile
                        board[y][x].Type = TrySwapToSpecial(noMatchTypes[Random.Range(0, noMatchTypes.Count)]);
                    } else {
                        board[y][x].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                    }            
                }
            }

            return board;
        }

        private List<List<bool>> FindMatches(List<List<Tile>> newBoard)
        {
            List<List<bool>> matchedTiles = new();
            for (int y = 0; y < newBoard.Count; y++)
            {
                matchedTiles.Add(new List<bool>(newBoard[y].Count));
                for (int x = 0; x < newBoard.Count; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {

                    switch ( _gameType ) {
                        case GameType.Match3:

                            if ( IsSpecialTile(newBoard[y][x].Type) ) {
                                continue;
                            }

                            if ( x > 1 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type &&
                                newBoard[y][x - 1].Type == newBoard[y][x - 2].Type ) {

                                matchedTiles[y][x] = true;
                                matchedTiles[y][x - 1] = true;
                                matchedTiles[y][x - 2] = true;
                            }

                            if ( y > 1 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type &&
                                newBoard[y - 1][x].Type == newBoard[y - 2][x].Type ) {
                                matchedTiles[y][x] = true;
                                matchedTiles[y - 1][x] = true;
                                matchedTiles[y - 2][x] = true;

                            }
                            break;
                        case GameType.Match4:
                            if ( x > 0 &&
                                newBoard[y][x].Type == newBoard[y][x - 1].Type ) {

                                if ( y > 0 &&
                                newBoard[y][x].Type == newBoard[y - 1][x].Type ) {

                                    if ( newBoard[y][x].Type == newBoard[y - 1][x - 1].Type ) {

                                        matchedTiles[y][x] = true;
                                        matchedTiles[y][x - 1] = true;
                                        matchedTiles[y - 1][x] = true;
                                        matchedTiles[y - 1][x - 1] = true;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }                    
                }
            }

            if ( _gameType == GameType.Match3 ) {
                return FindSpecialTiles(matchedTiles, newBoard); //After finding the regular matches, we look for Special Tiles around them and add their effects
            } else
                return matchedTiles;
        }

        private Vector2Int CheckRowNeighbors(int tileType, int x, int y, List<List<bool>> matchedTiles, List<List<Tile>> newBoard) {
            if ( x > 0 && newBoard[y][x - 1].Type == tileType )
                return new Vector2Int(x - 1, y);
            if (x + 3 < matchedTiles[y].Count && newBoard[y][x + 3].Type == tileType)
                return new Vector2Int(x + 3, y);
            return -Vector2Int.one;
        }

        private Vector2Int CheckColumnNeighbors(int tileType, int x, int y, List<List<bool>> matchedTiles, List<List<Tile>> newBoard) {
            if ( y > 0 && newBoard[y - 1][x].Type == tileType )
                return new Vector2Int(x, y - 1);
            if ( y + 3 < matchedTiles.Count && newBoard[y + 3][x].Type == tileType )
                return new Vector2Int(x, y + 3);
            return -Vector2Int.one;
        }

        void MarkAllMatchingTiles(int targetType, List<List<bool>> matchedTiles, List<List<Tile>> newBoard) {
            for ( int y = 0; y < matchedTiles.Count; y++ ) {
                for ( int x = 0; x < matchedTiles[y].Count; x++ ) {
                    if ( newBoard[y][x].Type == targetType ) {
                        matchedTiles[y][x] = true;
                    }
                }
            }
        }

        private List<List<bool>> FindSpecialTiles(List<List<bool>> matchedTiles, List<List<Tile>> newBoard) {

            for ( int y = 0; y < matchedTiles.Count; y++ ) {
                for ( int x = 0; x < matchedTiles[y].Count; x++ ) {
                    if ( matchedTiles[y][x] ) {
                        Vector2Int specialTileIndex;
                        //Checks neighboring tiles of a 3-Tile row combination
                        if ( x + 2 < matchedTiles[y].Count &&
                            matchedTiles[y][x + 1] &&
                            matchedTiles[y][x + 2] ) {
                            
                            specialTileIndex = CheckRowNeighbors(ROWBREAKER, x, y, matchedTiles, newBoard);
                            if (specialTileIndex != -Vector2Int.one) {
                                //Found ROWBREAKER: destroy entire ROW
                                for ( int j = 0; j < newBoard[y].Count; j++ )
                                    matchedTiles[y][j] = true;
                            } else {
                                specialTileIndex = CheckRowNeighbors(BOMB, x, y, matchedTiles, newBoard);
                                if ( specialTileIndex != -Vector2Int.one ) {
                                    //Found BOMB: destroy all equals
                                    matchedTiles[specialTileIndex.y][specialTileIndex.x] = true;
                                    MarkAllMatchingTiles(newBoard[y][x].Type, matchedTiles, newBoard);
                                }
                            }

                            x += 2;
                        }

                        //Checks neighboring tiles of a 3-Tile column combination
                        if ( y + 2 < matchedTiles.Count &&
                            matchedTiles[y + 1][x] &&
                            matchedTiles[y + 2][x] ) {

                            specialTileIndex = CheckColumnNeighbors(COLUMNBREAKER, x, y, matchedTiles, newBoard);
                            if ( specialTileIndex != -Vector2Int.one ) {
                                //Found COLUMNBREAKER: destroy entire COLUMN
                                for ( int j = 0; j < newBoard[y].Count; j++ )
                                    matchedTiles[j][x] = true;
                            } else {
                                specialTileIndex = CheckColumnNeighbors(BOMB, x, y, matchedTiles, newBoard);
                                if ( specialTileIndex != -Vector2Int.one ) {
                                    //Found BOMB: destroy all equals
                                    matchedTiles[specialTileIndex.y][specialTileIndex.x] = true;
                                    MarkAllMatchingTiles(newBoard[y][x].Type, matchedTiles, newBoard);
                                }
                            }

                            y += 2;
                        }
                    }
                }
            }
            return matchedTiles;
        }

        private bool HasMatch(List<List<bool>> list)
        {
            for (int y = 0; y < list.Count; y++)
            {
                for (int x = 0; x < list[y].Count; x++)
                {
                    if (list[y][x])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
