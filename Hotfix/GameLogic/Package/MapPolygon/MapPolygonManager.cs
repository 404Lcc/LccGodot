using Godot;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;

namespace LccHotfix
{
    /// <summary>
    /// 地图多边形区域类型。
    /// </summary>
    public enum MapPolygonAreaType
    {
        Collision,
        Occlusion,
    }

    /// <summary>
    /// 地图多边形运行时文档。
    /// </summary>
    public sealed class MapPolygonDocument
    {
        public string Texture { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MapPolygonData> Polygons { get; set; } = new List<MapPolygonData>();
    }

    /// <summary>
    /// 单个地图多边形数据。
    /// </summary>
    public sealed class MapPolygonData
    {
        public string Name { get; set; } = string.Empty;
        public MapPolygonAreaType Type { get; set; }
        public List<Vector2> Points { get; set; } = new List<Vector2>();
    }

    /// <summary>
    /// 运行时地图多边形构建配置。
    /// </summary>
    public sealed class MapPolygonBuildOptions
    {
        public bool BuildCollision { get; set; } = true;
        public bool BuildOcclusion { get; set; } = true;
        public bool BuildTexture { get; set; }
        public uint CollisionLayer { get; set; } = 1;
        public uint CollisionMask { get; set; } = 1;
        public int OcclusionZIndex { get; set; } = 100;
        public string RootName { get; set; } = "MapPolygonRuntimeRoot";
    }

    /// <summary>
    /// 地图多边形运行时服务。
    /// </summary>
    internal class MapPolygonManager : Module, IMapPolygonService
    {
        private const string RuntimeMetaKey = "map_polygon_runtime";
        private const string RuntimeTypeMetaKey = "map_polygon_runtime_type";
        private const string TextureNodeName = "MapTexture";
        private const string CollisionsNodeName = "Collisions";
        private const string OcclusionsNodeName = "Occlusions";

        /// <summary>
        /// 地图多边形服务不需要每帧轮询。
        /// </summary>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 地图多边形服务关闭时无持久资源需要释放。
        /// </summary>
        internal override void Shutdown()
        {
        }

        /// <summary>
        /// 读取地图多边形 JSON 文档。
        /// </summary>
        public MapPolygonDocument LoadDocument(string jsonPath)
        {
            string json = Godot.FileAccess.GetFileAsString(jsonPath);
            if (string.IsNullOrEmpty(json))
            {
                throw new FileNotFoundException($"读取地图多边形数据失败：{jsonPath}");
            }

            return ReadDocumentJson(json);
        }

        /// <summary>
        /// 使用默认配置创建运行时地图节点。
        /// </summary>
        public Node2D CreateRuntimeMap(string jsonPath, Node parent)
        {
            return CreateRuntimeMap(jsonPath, parent, new MapPolygonBuildOptions());
        }

        /// <summary>
        /// 使用指定配置创建运行时地图节点。
        /// </summary>
        public Node2D CreateRuntimeMap(string jsonPath, Node parent, MapPolygonBuildOptions options)
        {
            MapPolygonDocument document = LoadDocument(jsonPath);
            Node2D root = CreateRoot(options.RootName);
            parent.AddChild(root);

            Texture2D occlusionTexture = null!;
            if (options.BuildOcclusion)
            {
                occlusionTexture = LoadTexture(document.Texture);
            }

            if (options.BuildTexture)
            {
                CreateTextureNode(root, document.Texture);
            }

            Node2D collisionRoot = CreateChildRoot(root, CollisionsNodeName);
            Node2D occlusionRoot = CreateChildRoot(root, OcclusionsNodeName);

            foreach (MapPolygonData data in document.Polygons)
            {
                if (data.Points.Count < 3)
                {
                    GD.PushWarning($"跳过点数不足的地图多边形：{data.Name}, {data.Type}");
                    continue;
                }

                if (data.Type == MapPolygonAreaType.Collision && options.BuildCollision)
                {
                    CreateCollisionNode(collisionRoot, data, options);
                }

                if (data.Type == MapPolygonAreaType.Occlusion && options.BuildOcclusion)
                {
                    CreateOcclusionNode(occlusionRoot, data, options, occlusionTexture);
                }
            }

            return root;
        }

        /// <summary>
        /// 清理指定节点下由本服务创建的运行时地图节点。
        /// </summary>
        public void ClearRuntimeMap(Node node)
        {
            if (node == null)
            {
                return;
            }

            if (IsRuntimeNode(node))
            {
                RemoveNode(node);
                return;
            }

            var runtimeNodes = new List<Node>();
            foreach (Node child in node.GetChildren())
            {
                if (IsRuntimeNode(child))
                {
                    runtimeNodes.Add(child);
                }
            }

            foreach (Node runtimeNode in runtimeNodes)
            {
                RemoveNode(runtimeNode);
            }
        }

        /// <summary>
        /// 创建运行时地图根节点。
        /// </summary>
        private static Node2D CreateRoot(string name)
        {
            var root = new Node2D
            {
                Name = name,
            };
            MarkRuntimeNode(root, "Root");
            return root;
        }

        /// <summary>
        /// 创建运行时地图子根节点。
        /// </summary>
        private static Node2D CreateChildRoot(Node parent, string name)
        {
            var root = new Node2D
            {
                Name = name,
            };
            MarkRuntimeNode(root, name);
            parent.AddChild(root);
            return root;
        }

        /// <summary>
        /// 创建地图贴图节点。
        /// </summary>
        private static void CreateTextureNode(Node parent, string texturePath)
        {
            Texture2D texture = LoadTexture(texturePath);
            if (texture == null)
            {
                return;
            }

            var sprite = new Sprite2D
            {
                Name = TextureNodeName,
                Texture = texture,
                Centered = false,
            };
            MarkRuntimeNode(sprite, "Texture");
            parent.AddChild(sprite);
        }

        /// <summary>
        /// 加载地图贴图资源。
        /// </summary>
        private static Texture2D LoadTexture(string texturePath)
        {
            Texture2D texture = ResourceLoader.Load<Texture2D>(texturePath);
            if (texture == null)
            {
                GD.PushWarning($"无法加载地图贴图：{texturePath}");
            }

            return texture;
        }

        /// <summary>
        /// 创建地图碰撞节点。
        /// </summary>
        private static void CreateCollisionNode(Node parent, MapPolygonData data, MapPolygonBuildOptions options)
        {
            var body = new StaticBody2D
            {
                Name = data.Name,
                CollisionLayer = options.CollisionLayer,
                CollisionMask = options.CollisionMask,
            };
            MarkRuntimeNode(body, data.Type.ToString());

            var polygon = new CollisionPolygon2D
            {
                Name = $"{data.Name}_Polygon",
                Polygon = data.Points.ToArray(),
            };
            MarkRuntimeNode(polygon, data.Type.ToString());

            body.AddChild(polygon);
            parent.AddChild(body);
        }

        /// <summary>
        /// 创建地图视觉遮挡节点。
        /// </summary>
        private static void CreateOcclusionNode(Node parent, MapPolygonData data, MapPolygonBuildOptions options, Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            Vector2[] points = data.Points.ToArray();
            var polygon = new Polygon2D
            {
                Name = data.Name,
                Polygon = points,
                UV = points,
                Texture = texture,
                Color = Colors.White,
                ZIndex = options.OcclusionZIndex,
            };
            MarkRuntimeNode(polygon, data.Type.ToString());
            parent.AddChild(polygon);
        }

        /// <summary>
        /// 从场景树移除节点。
        /// </summary>
        private static void RemoveNode(Node node)
        {
            Node parent = node.GetParent();
            if (parent != null)
            {
                parent.RemoveChild(node);
            }

            node.QueueFree();
        }

        /// <summary>
        /// 标记节点为运行时地图节点。
        /// </summary>
        private static void MarkRuntimeNode(Node node, string nodeType)
        {
            node.SetMeta(RuntimeMetaKey, true);
            node.SetMeta(RuntimeTypeMetaKey, nodeType);
        }

        /// <summary>
        /// 判断节点是否为运行时地图节点。
        /// </summary>
        private static bool IsRuntimeNode(Node node)
        {
            return node.HasMeta(RuntimeMetaKey);
        }

        /// <summary>
        /// 将 LitJson 文档解析为地图多边形数据。
        /// </summary>
        private static MapPolygonDocument ReadDocumentJson(string json)
        {
            JsonData root = JsonMapper.ToObject(json);
            RequireObject(root, "root");

            var document = new MapPolygonDocument
            {
                Texture = ReadString(root, "texture"),
                Width = ReadInt(root, "width"),
                Height = ReadInt(root, "height"),
            };

            JsonData polygons = ReadRequired(root, "polygons");
            RequireArray(polygons, "polygons");

            for (int i = 0; i < polygons.Count; i++)
            {
                JsonData polygon = polygons[i];
                RequireObject(polygon, $"polygons[{i}]");

                string typeName = ReadString(polygon, "type");
                if (!Enum.TryParse(typeName, out MapPolygonAreaType type))
                {
                    throw new InvalidDataException($"无效的多边形类型：{typeName}");
                }

                var data = new MapPolygonData
                {
                    Name = ReadString(polygon, "name"),
                    Type = type,
                };

                JsonData points = ReadRequired(polygon, "points");
                RequireArray(points, $"polygons[{i}].points");

                for (int j = 0; j < points.Count; j++)
                {
                    JsonData point = points[j];
                    RequireObject(point, $"polygons[{i}].points[{j}]");
                    data.Points.Add(new Vector2(
                        ReadFloat(point, "x"),
                        ReadFloat(point, "y")));
                }

                document.Polygons.Add(data);
            }

            return document;
        }

        /// <summary>
        /// 读取必需的 JSON 属性。
        /// </summary>
        private static JsonData ReadRequired(JsonData data, string key)
        {
            if (!data.ContainsKey(key))
            {
                throw new InvalidDataException($"缺少 JSON 属性：{key}");
            }

            return data[key];
        }

        /// <summary>
        /// 读取必需的字符串属性。
        /// </summary>
        private static string ReadString(JsonData data, string key)
        {
            JsonData value = ReadRequired(data, key);
            if (!value.IsString)
            {
                throw new InvalidDataException($"{key} 必须是字符串。");
            }

            return (string)value;
        }

        /// <summary>
        /// 读取必需的整数属性。
        /// </summary>
        private static int ReadInt(JsonData data, string key)
        {
            JsonData value = ReadRequired(data, key);
            if (value.IsInt)
            {
                return (int)value;
            }

            if (value.IsLong)
            {
                return (int)(long)value;
            }

            throw new InvalidDataException($"{key} 必须是整数。");
        }

        /// <summary>
        /// 读取必需的浮点属性。
        /// </summary>
        private static float ReadFloat(JsonData data, string key)
        {
            JsonData value = ReadRequired(data, key);
            if (value.IsDouble)
            {
                return (float)(double)value;
            }

            if (value.IsInt)
            {
                return (int)value;
            }

            if (value.IsLong)
            {
                return (long)value;
            }

            throw new InvalidDataException($"{key} 必须是数字。");
        }

        /// <summary>
        /// 校验 JSON 节点为对象。
        /// </summary>
        private static void RequireObject(JsonData data, string path)
        {
            if (!data.IsObject)
            {
                throw new InvalidDataException($"{path} 必须是对象。");
            }
        }

        /// <summary>
        /// 校验 JSON 节点为数组。
        /// </summary>
        private static void RequireArray(JsonData data, string path)
        {
            if (!data.IsArray)
            {
                throw new InvalidDataException($"{path} 必须是数组。");
            }
        }
    }
}