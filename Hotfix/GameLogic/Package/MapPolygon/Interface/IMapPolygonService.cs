using Godot;

namespace LccHotfix
{
    /// <summary>
    /// 地图多边形运行时服务接口。
    /// </summary>
    public interface IMapPolygonService : IService
    {
        /// <summary>
        /// 读取地图多边形 JSON 文档。
        /// </summary>
        public MapPolygonDocument LoadDocument(string jsonPath);

        /// <summary>
        /// 使用默认配置创建运行时地图节点。
        /// </summary>
        public Node2D CreateRuntimeMap(string jsonPath, Node parent);

        /// <summary>
        /// 使用指定配置创建运行时地图节点。
        /// </summary>
        public Node2D CreateRuntimeMap(string jsonPath, Node parent, MapPolygonBuildOptions options);

        /// <summary>
        /// 清理指定节点下由本服务创建的运行时地图节点。
        /// </summary>
        public void ClearRuntimeMap(Node node);
    }
}