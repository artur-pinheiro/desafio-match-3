using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Gazeus.DesafioMatch3.Core.GameService;

namespace Gazeus.DesafioMatch3.Views
{
    public class BoardView : MonoBehaviour
    {
        public event Action<int, int> TileClicked;

        [SerializeField] private GridLayoutGroup _boardContainer;
        [SerializeField] private TilePrefabRepository _tilePrefabRepository;
        [SerializeField] private TileSpotView _tileSpotPrefab;

        [Header("Particle Pool")]
        [SerializeField] private ParticlePool _particlePool;
        [SerializeField] private Vector3 _positionOffset = Vector3.zero; 


        private GameObject[][] _tiles;
        private TileSpotView[][] _tileSpots;

        public void CreateBoard(List<List<Tile>> board)
        {
            _boardContainer.constraintCount = board[0].Count;
            _tiles = new GameObject[board.Count][];
            _tileSpots = new TileSpotView[board.Count][];

            for (int y = 0; y < board.Count; y++)
            {
                _tiles[y] = new GameObject[board[0].Count];
                _tileSpots[y] = new TileSpotView[board[0].Count];

                for (int x = 0; x < board[0].Count; x++)
                {
                    TileSpotView tileSpot = Instantiate(_tileSpotPrefab);
                    tileSpot.transform.SetParent(_boardContainer.transform, false);
                    tileSpot.SetPosition(x, y);
                    tileSpot.Clicked += TileSpot_Clicked;

                    _tileSpots[y][x] = tileSpot;

                    int tileTypeIndex = board[y][x].Type;
                    if (tileTypeIndex > -1)
                    {
                        GameObject tilePrefab = _tilePrefabRepository.TileTypePrefabList[tileTypeIndex];
                        GameObject tile = Instantiate(tilePrefab);
                        tileSpot.SetTile(tile);

                        _tiles[y][x] = tile;
                    }
                }
            }
        }

        public Tween CreateTile(List<AddedTileInfo> addedTiles)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < addedTiles.Count; i++)
            {
                AddedTileInfo addedTileInfo = addedTiles[i];
                Vector2Int position = addedTileInfo.Position;

                TileSpotView tileSpot = _tileSpots[position.y][position.x];

                GameObject tilePrefab = _tilePrefabRepository.TileTypePrefabList[addedTileInfo.Type];
                GameObject tile = Instantiate(tilePrefab);
                tileSpot.SetTile(tile);

                _tiles[position.y][position.x] = tile;

                tile.transform.localScale = Vector2.zero;
                sequence.Join(tile.transform.DOScale(1.0f, 0.2f));
            }

            return sequence;
        }

        public Tween DestroyTiles(List<Vector2Int> matchedPosition)
        {
            for (int i = 0; i < matchedPosition.Count; i++)
            {
                Vector2Int position = matchedPosition[i];

                SpawnParticles(_tiles[position.y][position.x].transform.position);              

                Destroy(_tiles[position.y][position.x]);
                _tiles[position.y][position.x] = null;
                EventSystem.OnTileDestroyed?.Invoke();
                }

            return DOVirtual.DelayedCall(0.2f, () => { });
        }

        private void SpawnParticles(Vector3 position) {
            _particlePool ??= GetComponent<ParticlePool>();
            _particlePool.SpawnParticle(position, Quaternion.Euler(0,180,0));
        }

        public Tween MoveTiles(List<MovedTileInfo> movedTiles)
        {
            GameObject[][] tiles = new GameObject[_tiles.Length][];
            for (int y = 0; y < _tiles.Length; y++)
            {
                tiles[y] = new GameObject[_tiles[y].Length];
                for (int x = 0; x < _tiles[y].Length; x++)
                {
                    tiles[y][x] = _tiles[y][x];
                }
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < movedTiles.Count; i++)
            {
                MovedTileInfo movedTileInfo = movedTiles[i];

                Vector2Int from = movedTileInfo.From;
                Vector2Int to = movedTileInfo.To;

                sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x]));

                tiles[to.y][to.x] = _tiles[from.y][from.x];
            }

            _tiles = tiles;

            return sequence;
        }

        public Tween SwapTiles(int fromX, int fromY, int toX, int toY, bool reverse = false)
        {
            Sequence sequence = DOTween.Sequence();

            MatchMode matchMode = (MatchMode) PlayerPrefs.GetInt("MatchMode", 0);

            switch ( matchMode ) {
                case MatchMode.Swap2:
                    sequence.Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX]));
                    sequence.Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX]));

                    (_tiles[toY][toX], _tiles[fromY][fromX]) = (_tiles[fromY][fromX], _tiles[toY][toX]);

                    break;
                case MatchMode.Rotate4:
                    if (!reverse) {
                        //Move Clockwise
                        Vector2Int topIndexes = GetTopLeftIndexes(fromX, fromY, toX, toY);
                        int topLeftX = topIndexes.y, topLeftY = topIndexes.x;

                        if ( topLeftX == -1 || topLeftY == -1 )
                            break;

                        sequence.Append(_tileSpots[topLeftY][topLeftX].AnimatedSetTile(_tiles[topLeftY + 1][topLeftX]));
                        sequence.Join(_tileSpots[topLeftY + 1][topLeftX].AnimatedSetTile(_tiles[topLeftY + 1][topLeftX + 1]));
                        sequence.Join(_tileSpots[topLeftY + 1][topLeftX + 1].AnimatedSetTile(_tiles[topLeftY][topLeftX + 1]));
                        sequence.Join(_tileSpots[topLeftY][topLeftX + 1].AnimatedSetTile(_tiles[topLeftY][topLeftX]));

                        (_tiles[topLeftY][topLeftX], _tiles[topLeftY + 1][topLeftX], _tiles[topLeftY + 1][topLeftX + 1], _tiles[topLeftY][topLeftX + 1]) =
                        (_tiles[topLeftY + 1][topLeftX], _tiles[topLeftY + 1][topLeftX + 1], _tiles[topLeftY][topLeftX + 1], _tiles[topLeftY][topLeftX]);

                    } else {
                        //Reverse movement and Move Back Counter Clockwise
                        Vector2Int topIndexes = GetTopLeftIndexes(toX, toY, fromX, fromY);
                        int topLeftX = topIndexes.y, topLeftY = topIndexes.x;

                        if ( topLeftX == -1 || topLeftY == -1 )
                            break;

                        sequence.Append(_tileSpots[topLeftY][topLeftX].AnimatedSetTile(_tiles[topLeftY][topLeftX + 1]));
                        sequence.Join(_tileSpots[topLeftY + 1][topLeftX].AnimatedSetTile(_tiles[topLeftY][topLeftX]));
                        sequence.Join(_tileSpots[topLeftY + 1][topLeftX + 1].AnimatedSetTile(_tiles[topLeftY + 1][topLeftX]));
                        sequence.Join(_tileSpots[topLeftY][topLeftX + 1].AnimatedSetTile(_tiles[topLeftY + 1][topLeftX + 1]));

                        (_tiles[topLeftY][topLeftX], _tiles[topLeftY + 1][topLeftX], _tiles[topLeftY + 1][topLeftX + 1], _tiles[topLeftY][topLeftX + 1]) =
                        (_tiles[topLeftY][topLeftX + 1], _tiles[topLeftY][topLeftX], _tiles[topLeftY + 1][topLeftX], _tiles[topLeftY + 1][topLeftX + 1]);
                    }

                    break;
                default:
                    break;
            }            

            return sequence;
        }

        private Vector2Int GetTopLeftIndexes(int fromX, int fromY, int toX, int toY) {

            int deltaX = toX - fromX;
            int deltaY = toY - fromY;

            if ( (Mathf.Abs(deltaX) == 1 && deltaY == 0) || // Horizontal adjacency
                (Mathf.Abs(deltaY) == 1 && deltaX == 0) )   // Vertical adjacency
            {
                // Determine the top-left corner based on the direction of movement
                int topLeftX = 0;
                int topLeftY = 0;

                if ( deltaX == 0 ) {
                    if ( deltaY == -1 ) { //upwards
                        topLeftY = toY;
                        topLeftX = toX;
                    } else if ( deltaY == 1 ) { //downwards
                        topLeftY = fromY;
                        topLeftX = fromX - 1;
                    }
                } else if ( deltaY == 0 ) {
                    if ( deltaX == -1 ) { // leftward
                        topLeftY = fromY - 1;
                        topLeftX = toX;
                    } else if ( deltaX == 1 ) { // rightward
                        topLeftY = fromY;
                        topLeftX = fromX;
                    }
                }

                return new Vector2Int(topLeftY, topLeftX);
            }

            return new Vector2Int(-1, -1);
        }

        #region Events
        private void TileSpot_Clicked(int x, int y)
        {
            TileClicked(x, y);
        }
        #endregion
    }
}
