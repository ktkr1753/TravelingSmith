using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using System.Linq;

public static class UtilityTool
{
    public static T CreateInstance<T>(PackedScene prefab, Node parnet, Vector2 pos) where T : Node
    {
        T instance = prefab.Instantiate() as T;
        if (instance is Node2D instance2D)
        {
            instance2D.Position = pos;
        }
        else if (instance is Control instanceControl)
        {
            instanceControl.Position = pos;
        }
        else 
        {
            Debug.PrintErr("instance不是Node2D或Control，無法設定Position");
            if(instance == null) 
            {
                Debug.PrintErr($"instance is Null, 可能是prefab的類型不對?");
            }
            else 
            {
                Debug.PrintErr($"instance name:{instance.Name}, Type:{instance.GetType().Name}");
            }
        }

        if (parnet != null)
        {
            parnet.AddChild(instance);
        }
        else 
        {
            Debug.PrintWarn("指定的parent為null");
        }

        return instance;
    }

    public static T CreateInstance<T>(string prefabPath) where T : Node
    {
        PackedScene packedScene = ResourceLoader.Load($"{prefabPath}.tscn") as PackedScene;

        T instance = CreateInstance<T>(packedScene, null, Vector2.Zero);
        return instance;
    }

    public static T CreateInstance<T>(PackedScene prefab) where T : Node
    {
        T instance = CreateInstance<T>(prefab, null, Vector2.Zero);
        return instance;
    }

    public static T CreateInstance<T>(PackedScene prefab, Vector2 pos) where T : Node
    {
        T instance = CreateInstance<T>(prefab, null, pos);
        return instance;
    }

    public static T CreateInstance<T>(PackedScene prefab, Node parnet) where T : Node
    {
        T instance = CreateInstance<T>(prefab, parnet, Vector2.Zero);
        return instance;
    }

    public static List<Node> SortedNearestNodes(Node mainNode, List<Node> unsortedNodes) 
    {
        List<Node> result = new List<Node>();

        List<KeyValuePair<Node, float>> datas = new List<KeyValuePair<Node, float>>();

        Vector3 mainPos = GetNodePosition(mainNode);

        for(int i = 0; i < unsortedNodes.Count; i++) 
        {
            Vector3 nodePos = GetNodePosition(unsortedNodes[i]);
            float distance = mainPos.DistanceTo(nodePos);

            datas.Add(new KeyValuePair<Node, float>(unsortedNodes[i], distance));
        }

        //排序
        datas = datas.OrderBy(data => data.Value).ToList();

        for (int i = 0; i < datas.Count; i++)
        {
            result.Add(datas[i].Key);
        }

        return result;
    }

    public static Vector3 GetNodePosition(Node node)
    {
        Vector3 result = Vector3.Zero;

        if (node is Node2D node2D) 
        {
            result = new Vector3(node2D.Position.X, node2D.Position.Y, 0);
        }
        else if (node is Control control)
        {
            result = new Vector3(control.Position.X, control.Position.Y, 0);
        }
        else if (node is Node3D node3D)
        {
            result = new Vector3(node3D.Position.X, node3D.Position.Y, node3D.Position.Z);
        }

        return result;
    }

    //转换为TileMap使用的tile data
    public static int[] ToTileData(Vector2I coords, int source_id, Vector2I atlas_coords,
    int alternative_tile, bool transpose, bool flip_v, bool flip_h) 
    {
        List<int> array = new List<int>(3);

        //第1位32位整数前16位表示坐标的y，后16位表示坐标x
        int coordsX = (coords.X >= 0 ? coords.X : ((1 << 16) + coords.X));
        int coordsY = (coords.Y >= 0 ? coords.Y * (1 << 16) : (((1 << 16) + coords.Y) << 16));
        int coords_32 = coordsY | coordsX;
        array.Add(coords_32);

        //第2位32位整数前16位表示图集坐标的x，后16位表示源id
        //int atlas_source_32 = (atlas_coords.X << 16) | source_id;
        int atlas_source_32 = (atlas_coords.X >= 0 ? atlas_coords.X * (1 << 16) : (((1 << 16) + atlas_coords.X) << 16)) | source_id;
        array.Add(atlas_source_32);

        //第3位32位整数的第1位是0，因为这是一个整数，如果转置是true第2位是1否则位0
        int int_32 = ((transpose ? 1 : 0) << 1);
        //如果垂直翻转为true则第3位为1否则为0
        int_32 = int_32 | ((flip_v ? 1 : 0) << 1);
        //如果水平翻转为true则第4位为1否则为0
        int_32 = (int_32 | (flip_h ? 1 : 0)) << 28;    //(int_32 | (1 if flip_h else 0)) << 28
        //后12位表示备选
        int_32 = int_32 | (alternative_tile << 16);
        //最后的16位是图集坐标y
        int_32 = int_32 | atlas_coords.Y;

        array.Add(int_32);

        return array.ToArray();
    }

    public static int[] ToTileData(int layer, Vector2I coords, TileMap tileMap) 
    {
        int source_id = tileMap.GetCellSourceId(layer, coords);
        Vector2I atlas_coords = tileMap.GetCellAtlasCoords(layer, coords);
        int alternative_tile = tileMap.GetCellAlternativeTile(layer, coords);
        TileData tileData = tileMap.GetCellTileData(layer, coords);
        if(tileData == null) 
        {
            return new int[0];
        }
        else 
        {
            bool transpose = tileData.Transpose;
            bool flip_v = tileData.FlipV;
            bool flip_h = tileData.FlipH;
            return ToTileData(coords, source_id, atlas_coords, alternative_tile, transpose, flip_v, flip_h);
        }
    }

    public static bool[] GetTerrainPeeringBitType(TileData tile, int terrainIndex)
    {
        bool[] result = new bool[8];

        int topLeftCorner = -1;
        int topSide = -1;
        int topRightCorner = -1;
        int leftSide = -1;
        int rightSide = -1;
        int bottomLeftCorner = -1;
        int bottomSide = -1;
        int bottomRightCorner = -1;
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.TopLeftCorner))
        {
            topLeftCorner = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.TopLeftCorner);			//左上
        }
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.TopSide))
        {
            topSide = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.TopSide);						//上
        }
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.TopRightCorner))
        {
            topRightCorner = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.TopRightCorner);			//右上
        }
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.LeftSide))
        {
            leftSide = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.LeftSide);					//左
        }
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.RightSide))
        {
            rightSide = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.RightSide);					//右
        }
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.BottomLeftCorner))
        {
            bottomLeftCorner = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.BottomLeftCorner);    //左下
        }
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.BottomSide))
        {
            bottomSide = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.BottomSide);                //下
        }
        if (tile.IsValidTerrainPeeringBit(TileSet.CellNeighbor.BottomRightCorner))
        {
            bottomRightCorner = tile.GetTerrainPeeringBit(TileSet.CellNeighbor.BottomRightCorner);	//右下
        }

        if (topLeftCorner == terrainIndex) { result[0] = true; }
        if (topSide == terrainIndex) { result[1] = true; }
        if (topRightCorner == terrainIndex) { result[2] = true; }
        if (leftSide == terrainIndex) { result[3] = true; }
        if (rightSide == terrainIndex) { result[4] = true; }
        if (bottomLeftCorner == terrainIndex) { result[5] = true; }
        if (bottomSide == terrainIndex) { result[6] = true; }
        if (bottomRightCorner == terrainIndex) { result[7] = true; }

        return result;
    }

    public static Dictionary<TileSet.CellNeighbor, int> GetTerrainPeeringBitDic(TileData tile) 
    {
        Dictionary<TileSet.CellNeighbor, int> result = new Dictionary<TileSet.CellNeighbor, int>();

        void CheckAndSetPeeringBit(Dictionary<TileSet.CellNeighbor, int> result, TileSet.CellNeighbor type) 
        {
            if (tile == null)
            {
                result[type] = -2;
            }
            else if (tile.IsValidTerrainPeeringBit(type))
            {
                result[type] = tile.GetTerrainPeeringBit(type);
            }
            else
            {
                result[type] = -1;
            }
        }

        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.TopLeftCorner);          //左上
        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.TopSide);                //上
        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.TopRightCorner);         //右上
        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.LeftSide);               //左
        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.RightSide);              //右
        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.BottomLeftCorner);       //左下
        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.BottomSide);             //下
        CheckAndSetPeeringBit(result, TileSet.CellNeighbor.BottomRightCorner);      //右下

        return result;
    }
}
