using Godot;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LccEditor
{
    /// <summary>
    /// 导出的地图多边形区域类型。
    /// </summary>
    public enum PolygonAreaType
    {
        Collision,
        Occlusion,
    }

    /// <summary>
    /// 保存到贴图旁边的地图多边形文档数据。
    /// </summary>
    public sealed class MapPolygonDocument
    {
        public string Texture { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MapPolygonData> Polygons { get; set; } = new List<MapPolygonData>();
    }

    /// <summary>
    /// 单个多边形区域的数据，包含类型和地图空间坐标点。
    /// </summary>
    public sealed class MapPolygonData
    {
        public string Name { get; set; } = string.Empty;
        public PolygonAreaType Type { get; set; }
        public List<Vector2> Points { get; set; } = new List<Vector2>();
    }

    /// <summary>
    /// 用于创建 Polygon2D 地图区域并保存为 JSON 的编辑器窗口。
    /// </summary>
    [MenuTree("地图/多边形区域", 10)]
    public sealed class MapPolygonEditorWindow : LccEditorWindowBase
    {
        #region 配置字段

        private const string EditorMetaKey = "map_polygon_editor";
        private const string TypeMetaKey = "map_polygon_type";
        private const string RootNodeName = "MapPolygonEditorRoot";
        private const string TextureNodeName = "MapTexture";
        private const string PolygonsNodeName = "Polygons";
        private const string FixedEditScenePath = "res://Res/map.poly_edit.tscn";
        private const string FixedJsonPath = "res://Res/map.poly.json";

        private OptionButton _typeOption = null;
        private Label _pathLabel = null;
        private Label _sceneLabel = null;
        private Label _statusLabel = null;
        private FileDialog _replaceTextureDialog = null;
        private FileDialog _saveDataDialog = null;
        private FileDialog _loadDataDialog = null;
        private Node2D _polygonsRoot = null;
        private string _texturePath = string.Empty;
        private Vector2 _textureSize;

        #endregion

        #region UI构建

        /// <summary>
        /// 构建显示在 LccEditor 面板中的完整编辑器界面。
        /// </summary>
        public override Control BuildContent()
        {
            var root = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(720, 480),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            };

            root.AddChild(BuildToolbar());
            root.AddChild(BuildReplaceTextureDialog());
            root.AddChild(BuildSaveDataDialog());
            root.AddChild(BuildLoadDataDialog());
            return root;
        }

        /// <summary>
        /// 构建命令工具栏和状态文本。
        /// </summary>
        private Control BuildToolbar()
        {
            var root = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };

            var sceneToolbar = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            root.AddChild(sceneToolbar);

            AddButton(sceneToolbar, "打开场景", OpenEditScene);
            AddButton(sceneToolbar, "替换贴图", OpenReplaceTextureDialog);

            _pathLabel = new Label
            {
                Text = FixedJsonPath,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                ClipText = true,
                VerticalAlignment = VerticalAlignment.Center,
            };
            sceneToolbar.AddChild(_pathLabel);

            var editToolbar = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            root.AddChild(editToolbar);

            var typeLabel = new Label
            {
                Text = "区域类型",
                VerticalAlignment = VerticalAlignment.Center,
            };
            editToolbar.AddChild(typeLabel);

            _typeOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(120, 0),
            };
            _typeOption.AddItem(GetAreaTypeText(PolygonAreaType.Collision));
            _typeOption.AddItem(GetAreaTypeText(PolygonAreaType.Occlusion));
            editToolbar.AddChild(_typeOption);

            AddButton(editToolbar, "新建区域", CreatePolygonNode);
            AddButton(editToolbar, "保存数据", OpenSaveDataDialog);
            AddButton(editToolbar, "加载数据", OpenLoadDataDialog);

            _sceneLabel = new Label
            {
                Text = "编辑场景：未创建",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                ClipText = true,
            };
            root.AddChild(_sceneLabel);

            _statusLabel = new Label
            {
                Text = "打开固定编辑场景，并按固定数据文件加载区域。",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            root.AddChild(_statusLabel);

            return root;
        }

        /// <summary>
        /// 构建用于替换地图贴图的文件选择对话框。
        /// </summary>
        private FileDialog BuildReplaceTextureDialog()
        {
            _replaceTextureDialog = new FileDialog
            {
                Access = FileDialog.AccessEnum.Resources,
                FileMode = FileDialog.FileModeEnum.OpenFile,
                Title = "替换地图贴图",
            };
            _replaceTextureDialog.AddFilter("*.png, *.jpg, *.jpeg, *.webp ; 图片文件");
            _replaceTextureDialog.FileSelected += OnReplaceTextureSelected;
            return _replaceTextureDialog;
        }

        /// <summary>
        /// 构建用于指定保存数据文件的文件选择对话框。
        /// </summary>
        private FileDialog BuildSaveDataDialog()
        {
            _saveDataDialog = new FileDialog
            {
                Access = FileDialog.AccessEnum.Resources,
                FileMode = FileDialog.FileModeEnum.SaveFile,
                Title = "保存多边形数据",
                CurrentPath = FixedJsonPath,
            };
            _saveDataDialog.AddFilter("*.json, *.poly.json ; JSON文件");
            _saveDataDialog.FileSelected += OnSaveDataSelected;
            return _saveDataDialog;
        }

        /// <summary>
        /// 构建用于指定加载数据文件的文件选择对话框。
        /// </summary>
        private FileDialog BuildLoadDataDialog()
        {
            _loadDataDialog = new FileDialog
            {
                Access = FileDialog.AccessEnum.Resources,
                FileMode = FileDialog.FileModeEnum.OpenFile,
                Title = "加载多边形数据",
            };
            _loadDataDialog.AddFilter("*.json, *.poly.json ; JSON文件");
            _loadDataDialog.FileSelected += OnLoadDataSelected;
            return _loadDataDialog;
        }

        /// <summary>
        /// 添加一个绑定到指定命令的工具栏按钮。
        /// </summary>
        private static void AddButton(Container parent, string text, Action onPressed)
        {
            var button = new Button
            {
                Text = text,
                CustomMinimumSize = new Vector2(88, 0),
            };
            button.Pressed += onPressed;
            parent.AddChild(button);
        }

        #endregion

        #region 工具栏事件

        /// <summary>
        /// 打开固定的多边形编辑场景，不存在时用当前贴图创建。
        /// </summary>
        private void OpenEditScene()
        {
            if (File.Exists(ProjectSettings.GlobalizePath(FixedEditScenePath)))
            {
                EditorPlugin.GetEditorInterface().OpenSceneFromPath(FixedEditScenePath);
                BindPolygonsRoot();
                if (File.Exists(ProjectSettings.GlobalizePath(FixedJsonPath)))
                {
                    Load();
                }
                else
                {
                    SetStatus($"已打开编辑场景：{FixedEditScenePath}");
                }

                return;
            }

            MapPolygonDocument document = LoadFixedDocument();
            if (document == null)
            {
                return;
            }

            ApplyDocumentInfo(document);
            Texture2D texture = ResourceLoader.Load<Texture2D>(document.Texture);
            if (texture == null)
            {
                SetStatus($"无法加载图片：{document.Texture}");
                return;
            }

            CreateEditScene(texture);
            if (File.Exists(ProjectSettings.GlobalizePath(FixedJsonPath)))
            {
                Load();
            }
            else
            {
                SetStatus($"已创建编辑场景：{FixedEditScenePath}");
            }
        }

        /// <summary>
        /// 打开替换地图贴图的文件选择对话框。
        /// </summary>
        private void OpenReplaceTextureDialog()
        {
            _replaceTextureDialog.PopupCenteredRatio(0.75f);
        }

        /// <summary>
        /// 打开保存多边形数据的文件选择对话框。
        /// </summary>
        private void OpenSaveDataDialog()
        {
            _saveDataDialog.CurrentPath = FixedJsonPath;
            _saveDataDialog.PopupCenteredRatio(0.75f);
        }

        /// <summary>
        /// 打开加载多边形数据的文件选择对话框。
        /// </summary>
        private void OpenLoadDataDialog()
        {
            _loadDataDialog.PopupCenteredRatio(0.75f);
        }

        /// <summary>
        /// 使用选中的图片替换固定编辑场景中的地图贴图。
        /// </summary>
        private void OnReplaceTextureSelected(string path)
        {
            Texture2D texture = ResourceLoader.Load<Texture2D>(path);
            if (texture == null)
            {
                SetStatus($"无法加载图片：{path}");
                return;
            }

            _texturePath = path;
            _textureSize = texture.GetSize();

            if (_pathLabel != null)
            {
                _pathLabel.Text = path;
            }

            if (File.Exists(ProjectSettings.GlobalizePath(FixedEditScenePath)))
            {
                EditorPlugin.GetEditorInterface().OpenSceneFromPath(FixedEditScenePath);
                BindPolygonsRoot();
            }
            else
            {
                CreateEditScene(texture);
            }

            ReplaceMapTexture(texture);
            Save();
            SetStatus($"已替换地图贴图：{path}");
        }

        /// <summary>
        /// 将多边形数据保存到选中的 JSON 文件。
        /// </summary>
        private void OnSaveDataSelected(string path)
        {
            Save(path);
        }

        /// <summary>
        /// 从选中的 JSON 文件加载多边形数据。
        /// </summary>
        private void OnLoadDataSelected(string path)
        {
            Load(path);
        }

        #endregion

        #region 场景贴图

        /// <summary>
        /// 创建并保存包含贴图和多边形根节点的编辑场景。
        /// </summary>
        private void CreateEditScene(Texture2D texture)
        {
            var sceneRoot = new Node2D
            {
                Name = RootNodeName,
            };

            var sprite = new Sprite2D
            {
                Name = TextureNodeName,
                Texture = texture,
                Centered = false,
            };
            sceneRoot.AddChild(sprite);
            sprite.Owner = sceneRoot;

            var polygonsRoot = new Node2D
            {
                Name = PolygonsNodeName,
            };
            sceneRoot.AddChild(polygonsRoot);
            polygonsRoot.Owner = sceneRoot;

            var packedScene = new PackedScene();
            Error packResult = packedScene.Pack(sceneRoot);
            if (packResult != Error.Ok)
            {
                sceneRoot.QueueFree();
                SetStatus($"无法创建编辑场景：{FixedEditScenePath}, {packResult}");
                return;
            }

            string globalScenePath = ProjectSettings.GlobalizePath(FixedEditScenePath);
            string directory = Path.GetDirectoryName(globalScenePath)!;
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Error saveResult = ResourceSaver.Save(packedScene, FixedEditScenePath);
            sceneRoot.QueueFree();
            if (saveResult != Error.Ok)
            {
                SetStatus($"无法保存编辑场景：{FixedEditScenePath}, {saveResult}");
                return;
            }

            EditorPlugin.GetEditorInterface().OpenSceneFromPath(FixedEditScenePath);
            BindPolygonsRoot();
        }

        /// <summary>
        /// 替换或创建编辑场景根节点下的 MapTexture 精灵。
        /// </summary>
        private void ReplaceMapTexture(Texture2D texture)
        {
            Node editedSceneRoot = EditorPlugin.GetEditorInterface().GetEditedSceneRoot();
            if (editedSceneRoot == null)
            {
                SetStatus("无法获取当前编辑场景。");
                return;
            }

            Sprite2D sprite = editedSceneRoot.GetNodeOrNull<Sprite2D>(TextureNodeName);
            if (sprite == null)
            {
                sprite = new Sprite2D
                {
                    Name = TextureNodeName,
                };
                editedSceneRoot.AddChild(sprite);
                sprite.Owner = editedSceneRoot;
                editedSceneRoot.MoveChild(sprite, 0);
            }

            sprite.Texture = texture;
            sprite.Centered = false;
            SaveEditScene();
        }

        /// <summary>
        /// 在当前打开的编辑场景中查找或创建 Polygons 节点。
        /// </summary>
        private void BindPolygonsRoot()
        {
            Node editedSceneRoot = EditorPlugin.GetEditorInterface().GetEditedSceneRoot();
            if (editedSceneRoot == null)
            {
                _polygonsRoot = null!;
                SetStatus("无法获取当前编辑场景。");
                return;
            }

            _polygonsRoot = editedSceneRoot.GetNodeOrNull<Node2D>(PolygonsNodeName);
            if (_polygonsRoot == null)
            {
                _polygonsRoot = new Node2D
                {
                    Name = PolygonsNodeName,
                };
                editedSceneRoot.AddChild(_polygonsRoot);
                _polygonsRoot.Owner = editedSceneRoot;
                SaveEditScene();
            }

            if (_sceneLabel != null)
            {
                _sceneLabel.Text = $"编辑场景：{FixedEditScenePath} | 数据：{FixedJsonPath}";
            }
        }

        /// <summary>
        /// 确保 Polygons 根节点已经绑定到当前编辑场景。
        /// </summary>
        private bool EnsurePolygonsRoot()
        {
            if (_polygonsRoot != null && GodotObject.IsInstanceValid(_polygonsRoot))
            {
                return true;
            }

            BindPolygonsRoot();
            return _polygonsRoot != null && GodotObject.IsInstanceValid(_polygonsRoot);
        }

        /// <summary>
        /// 在存在编辑场景路径时保存当前打开的编辑场景。
        /// </summary>
        private void SaveEditScene()
        {
            Error result = EditorPlugin.GetEditorInterface().SaveScene();
            if (result != Error.Ok)
            {
                GD.PushWarning($"无法保存编辑场景：{FixedEditScenePath}, {result}");
            }
        }

        #endregion

        #region 区域节点

        /// <summary>
        /// 在 Polygons 根节点下创建一个受管理的空 Polygon2D。
        /// </summary>
        private void CreatePolygonNode()
        {
            if (!EnsurePolygonsRoot())
            {
                return;
            }

            PolygonAreaType type = GetCurrentType();
            var polygon = new Polygon2D
            {
                Name = GetNextPolygonName(type),
                Color = GetAreaColor(type),
                Polygon = Array.Empty<Vector2>(),
            };
            MarkPolygonNode(polygon, type);

            _polygonsRoot!.AddChild(polygon);
            TrySetOwner(polygon);
            SetStatus("已创建空多边形节点，请使用编辑器原生工具编辑多边形点。");
        }

        /// <summary>
        /// 从 Polygons 根节点中移除所有直属 Polygon2D 节点。
        /// </summary>
        private void ClearPolygonNodes()
        {
            if (_polygonsRoot == null || !GodotObject.IsInstanceValid(_polygonsRoot))
            {
                return;
            }

            var polygons = new List<Polygon2D>();
            foreach (Node child in _polygonsRoot.GetChildren())
            {
                if (child is Polygon2D polygon)
                {
                    polygons.Add(polygon);
                }
            }

            foreach (Polygon2D polygon in polygons)
            {
                Node parent = polygon.GetParent();
                if (parent != null)
                {
                    parent.RemoveChild(polygon);
                }

                polygon.QueueFree();
            }
        }

        /// <summary>
        /// 收集 Polygons 根节点下直属的受管理 Polygon2D 节点。
        /// </summary>
        private List<Polygon2D> GetPolygonNodes()
        {
            var polygons = new List<Polygon2D>();
            if (_polygonsRoot == null || !GodotObject.IsInstanceValid(_polygonsRoot))
            {
                return polygons;
            }

            foreach (Node child in _polygonsRoot.GetChildren())
            {
                if (child is Polygon2D polygon && IsManagedPolygonNode(polygon))
                {
                    polygons.Add(polygon);
                }
            }

            return polygons;
        }

        /// <summary>
        /// 将多边形的本地点坐标转换到 Polygons 根节点坐标空间。
        /// </summary>
        private List<Vector2> GetPointsInRootSpace(Polygon2D polygon)
        {
            var points = new List<Vector2>();
            Transform2D rootTransform = _polygonsRoot!.GlobalTransform.AffineInverse();

            foreach (Vector2 point in polygon.Polygon)
            {
                points.Add(rootTransform * (polygon.GlobalTransform * point));
            }

            return points;
        }

        /// <summary>
        /// 设置节点的场景归属，确保节点会随编辑场景一起保存。
        /// </summary>
        private void TrySetOwner(Node node)
        {
            Node sceneRoot = EditorPlugin.GetEditorInterface().GetEditedSceneRoot();
            if (sceneRoot != null && sceneRoot.IsAncestorOf(node))
            {
                node.Owner = sceneRoot;
            }
        }

        #endregion

        #region 保存加载

        /// <summary>
        /// 将受管理的多边形保存到贴图对应的 .poly.json 文件。
        /// </summary>
        private void Save()
        {
            Save(FixedJsonPath);
        }

        /// <summary>
        /// 将受管理的多边形保存到指定的 .poly.json 文件。
        /// </summary>
        private void Save(string resourcePath)
        {
            if (!EnsurePolygonsRoot())
            {
                return;
            }

            string globalPath = ProjectSettings.GlobalizePath(resourcePath);
            try
            {
                string directory = Path.GetDirectoryName(globalPath)!;
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonMapper.ToJson(CreateDocumentJson(), true);
                File.WriteAllText(globalPath, json, Encoding.UTF8);
                SaveEditScene();
                EditorPlugin.GetEditorInterface().GetResourceFilesystem().Scan();
                SetStatus($"已保存：{globalPath}");
            }
            catch (Exception ex)
            {
                GD.PushError($"保存多边形数据失败：{resourcePath} -> {globalPath}\n{ex}");
                SetStatus($"保存失败：{globalPath}");
            }
        }

        /// <summary>
        /// 从贴图对应的 .poly.json 文件加载受管理的多边形。
        /// </summary>
        private void Load()
        {
            Load(FixedJsonPath);
        }

        /// <summary>
        /// 从指定的 .poly.json 文件加载受管理的多边形。
        /// </summary>
        private void Load(string resourcePath)
        {
            if (!EnsurePolygonsRoot())
            {
                return;
            }

            string globalPath = ProjectSettings.GlobalizePath(resourcePath);
            if (!File.Exists(globalPath))
            {
                SetStatus($"未找到多边形数据：{resourcePath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(globalPath, Encoding.UTF8);
                MapPolygonDocument document = ReadDocumentJson(json);
                ApplyDocumentInfo(document);

                ClearPolygonNodes();
                int loadedCount = 0;
                foreach (MapPolygonData data in document.Polygons)
                {
                    var polygon = new Polygon2D
                    {
                        Name = GetLoadPolygonName(data),
                        Color = GetAreaColor(data.Type),
                        Polygon = data.Points.ToArray(),
                    };
                    MarkPolygonNode(polygon, data.Type);

                    _polygonsRoot!.AddChild(polygon);
                    TrySetOwner(polygon);
                    loadedCount++;
                }

                SaveEditScene();
                EditorPlugin.GetEditorInterface().GetResourceFilesystem().Scan();
                SetStatus($"已加载 {loadedCount} 个区域：{globalPath}");
            }
            catch (Exception ex)
            {
                GD.PushError($"加载多边形数据失败：{resourcePath} -> {globalPath}\n{ex}");
                SetStatus($"加载失败：{globalPath}");
            }
        }

        /// <summary>
        /// 读取固定数据文件中的地图多边形文档。
        /// </summary>
        private MapPolygonDocument LoadFixedDocument()
        {
            string globalPath = ProjectSettings.GlobalizePath(FixedJsonPath);
            if (!File.Exists(globalPath))
            {
                SetStatus($"未找到多边形数据：{FixedJsonPath}");
                return null!;
            }

            try
            {
                string json = File.ReadAllText(globalPath, Encoding.UTF8);
                return ReadDocumentJson(json);
            }
            catch (Exception ex)
            {
                GD.PushError($"读取多边形数据失败：{FixedJsonPath} -> {globalPath}\n{ex}");
                SetStatus($"读取失败：{globalPath}");
                return null!;
            }
        }

        /// <summary>
        /// 根据文档数据同步当前贴图路径和贴图尺寸。
        /// </summary>
        private void ApplyDocumentInfo(MapPolygonDocument document)
        {
            _texturePath = document.Texture;
            _textureSize = new Vector2(document.Width, document.Height);
            if (_pathLabel != null)
            {
                _pathLabel.Text = _texturePath;
            }
        }

        /// <summary>
        /// 根据当前受管理的 Polygon2D 节点创建内存文档数据。
        /// </summary>
        private MapPolygonDocument CreateDocument()
        {
            var document = new MapPolygonDocument
            {
                Texture = _texturePath,
                Width = (int)_textureSize.X,
                Height = (int)_textureSize.Y,
            };

            foreach (Polygon2D polygon in GetPolygonNodes())
            {
                document.Polygons.Add(new MapPolygonData
                {
                    Name = polygon.Name,
                    Type = GetPolygonType(polygon),
                    Points = GetPointsInRootSpace(polygon),
                });
            }

            return document;
        }

        #endregion

        #region JSON读写

        /// <summary>
        /// 将当前文档数据转换为 LitJson 对象树。
        /// </summary>
        private JsonData CreateDocumentJson()
        {
            MapPolygonDocument document = CreateDocument();
            JsonData root = CreateObject();
            root["texture"] = document.Texture;
            root["width"] = document.Width;
            root["height"] = document.Height;

            JsonData polygons = CreateArray();
            foreach (MapPolygonData data in document.Polygons)
            {
                JsonData polygon = CreateObject();
                polygon["name"] = data.Name;
                polygon["type"] = data.Type.ToString();

                JsonData points = CreateArray();
                foreach (Vector2 point in data.Points)
                {
                    JsonData pointJson = CreateObject();
                    pointJson["x"] = (double)point.X;
                    pointJson["y"] = (double)point.Y;
                    points.Add(pointJson);
                }

                polygon["points"] = points;
                polygons.Add(polygon);
            }

            root["polygons"] = polygons;
            return root;
        }

        /// <summary>
        /// 将 LitJson 文档解析为地图多边形数据。
        /// </summary>
        private MapPolygonDocument ReadDocumentJson(string json)
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
                if (!Enum.TryParse(typeName, out PolygonAreaType type))
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
        /// 创建一个空的 LitJson 对象节点。
        /// </summary>
        private static JsonData CreateObject()
        {
            var data = new JsonData();
            data.SetJsonType(JsonType.Object);
            return data;
        }

        /// <summary>
        /// 创建一个空的 LitJson 数组节点。
        /// </summary>
        private static JsonData CreateArray()
        {
            var data = new JsonData();
            data.SetJsonType(JsonType.Array);
            return data;
        }

        /// <summary>
        /// 读取必需的 JSON 属性，缺失时抛出带路径信息的异常。
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
        /// 读取必需的字符串 JSON 属性。
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
        /// 读取必需的整数 JSON 属性。
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
        /// 将必需的数字 JSON 属性读取为 float。
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
        /// 校验 JSON 节点是否为对象。
        /// </summary>
        private static void RequireObject(JsonData data, string path)
        {
            if (!data.IsObject)
            {
                throw new InvalidDataException($"{path} 必须是对象。");
            }
        }

        /// <summary>
        /// 校验 JSON 节点是否为数组。
        /// </summary>
        private static void RequireArray(JsonData data, string path)
        {
            if (!data.IsArray)
            {
                throw new InvalidDataException($"{path} 必须是数组。");
            }
        }

        #endregion

        #region 类型显示辅助

        /// <summary>
        /// 标记 Polygon2D 由此编辑器管理，并记录区域类型。
        /// </summary>
        private static void MarkPolygonNode(Polygon2D polygon, PolygonAreaType type)
        {
            polygon.SetMeta(EditorMetaKey, true);
            polygon.SetMeta(TypeMetaKey, type.ToString());
        }

        /// <summary>
        /// 判断 Polygon2D 是否属于此编辑器管理的节点集合。
        /// </summary>
        private static bool IsManagedPolygonNode(Polygon2D polygon)
        {
            return polygon.HasMeta(EditorMetaKey);
        }

        /// <summary>
        /// 获取多边形保存的区域类型，缺失时使用碰撞类型。
        /// </summary>
        private static PolygonAreaType GetPolygonType(Polygon2D polygon)
        {
            if (polygon.HasMeta(TypeMetaKey)
                && Enum.TryParse(polygon.GetMeta(TypeMetaKey).ToString(), out PolygonAreaType type))
            {
                return type;
            }

            return PolygonAreaType.Collision;
        }

        /// <summary>
        /// 从类型选择器获取当前选择的区域类型。
        /// </summary>
        private PolygonAreaType GetCurrentType()
        {
            if (_typeOption != null && _typeOption.Selected == 1)
            {
                return PolygonAreaType.Occlusion;
            }

            return PolygonAreaType.Collision;
        }

        /// <summary>
        /// 获取加载多边形时使用的节点名称。
        /// </summary>
        private string GetLoadPolygonName(MapPolygonData data)
        {
            if (string.IsNullOrWhiteSpace(data.Name))
            {
                return GetNextPolygonName(data.Type);
            }

            return data.Name;
        }

        /// <summary>
        /// 为指定区域类型生成下一个未使用的多边形名称。
        /// </summary>
        private string GetNextPolygonName(PolygonAreaType type)
        {
            var existingNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (Polygon2D polygon in GetPolygonNodes())
            {
                existingNames.Add(polygon.Name);
            }

            string prefix = type.ToString();
            int index = 1;
            string name;
            do
            {
                name = $"{prefix}_{index:000}";
                index++;
            } while (existingNames.Contains(name));

            return name;
        }

        /// <summary>
        /// 向编辑器界面写入状态消息。
        /// </summary>
        private void SetStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = message;
            }
        }

        /// <summary>
        /// 获取区域类型对应的显示颜色。
        /// </summary>
        private static Color GetAreaColor(PolygonAreaType type)
        {
            if (type == PolygonAreaType.Occlusion)
            {
                return new Color(0.0f, 0.0f, 0.0f, 0.35f);
            }

            return new Color(0.0f, 1.0f, 0.0f, 0.35f);
        }

        /// <summary>
        /// 获取区域类型对应的中文显示文本。
        /// </summary>
        private static string GetAreaTypeText(PolygonAreaType type)
        {
            if (type == PolygonAreaType.Occlusion)
            {
                return "遮挡";
            }

            return "碰撞";
        }

        #endregion
    }
}