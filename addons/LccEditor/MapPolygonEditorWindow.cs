using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace LccEditor
{
    [MenuTree("地图/多边形区域", 10)]
    public sealed class MapPolygonEditorWindow : LccEditorWindowBase
    {
        private const string EditorMetaKey = "map_polygon_editor";
        private const string TypeMetaKey = "map_polygon_type";

        private OptionButton? _typeOption;
        private Label? _pathLabel;
        private Label? _rootLabel;
        private Label? _statusLabel;
        private ItemList? _polygonList;
        private FileDialog? _fileDialog;
        private Node2D? _editRoot;
        private string _texturePath = string.Empty;
        private Vector2 _textureSize;
        private PolygonAreaType _currentType = PolygonAreaType.Collision;

        public override Control BuildContent()
        {
            var root = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(720, 480),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            };

            root.AddChild(BuildToolbar());
            root.AddChild(BuildBody());
            root.AddChild(BuildFileDialog());

            return root;
        }

        private Control BuildToolbar()
        {
            var root = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };

            var toolbar = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            root.AddChild(toolbar);

            var bindRootButton = new Button
            {
                Text = "绑定选中节点",
            };
            bindRootButton.Pressed += BindSelectedRoot;
            toolbar.AddChild(bindRootButton);

            var openButton = new Button
            {
                Text = "选择图片",
            };
            openButton.Pressed += OpenTextureDialog;
            toolbar.AddChild(openButton);

            _typeOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(120, 0),
            };
            _typeOption.AddItem(PolygonAreaType.Collision.ToString());
            _typeOption.AddItem(PolygonAreaType.Occlusion.ToString());
            _typeOption.ItemSelected += OnTypeSelected;
            toolbar.AddChild(_typeOption);

            var createButton = new Button
            {
                Text = "新建区域",
            };
            createButton.Pressed += CreatePolygonNode;
            toolbar.AddChild(createButton);

            var deleteButton = new Button
            {
                Text = "删除选中",
            };
            deleteButton.Pressed += DeleteSelectedPolygon;
            toolbar.AddChild(deleteButton);

            var refreshButton = new Button
            {
                Text = "刷新",
            };
            refreshButton.Pressed += RefreshPolygonList;
            toolbar.AddChild(refreshButton);

            var saveButton = new Button
            {
                Text = "保存JSON",
            };
            saveButton.Pressed += Save;
            toolbar.AddChild(saveButton);

            var loadButton = new Button
            {
                Text = "加载JSON",
            };
            loadButton.Pressed += Load;
            toolbar.AddChild(loadButton);

            _pathLabel = new Label
            {
                Text = "未选择图片",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                ClipText = true,
            };
            toolbar.AddChild(_pathLabel);

            _rootLabel = new Label
            {
                Text = "编辑根节点：未绑定",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                ClipText = true,
            };
            root.AddChild(_rootLabel);

            _statusLabel = new Label
            {
                Text = "绑定 Node2D 后，新建或加载 Polygon2D，用 Godot 原生多边形编辑点位。",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            root.AddChild(_statusLabel);

            return root;
        }

        private Control BuildBody()
        {
            _polygonList = new ItemList
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                AllowReselect = true,
            };
            _polygonList.ItemSelected += OnPolygonSelected;
            return _polygonList;
        }

        private FileDialog BuildFileDialog()
        {
            _fileDialog = new FileDialog
            {
                Access = FileDialog.AccessEnum.Resources,
                FileMode = FileDialog.FileModeEnum.OpenFile,
                Title = "选择地图图片",
            };
            _fileDialog.AddFilter("*.png, *.jpg, *.jpeg, *.webp ; Image Files");
            _fileDialog.FileSelected += OnTextureSelected;
            return _fileDialog;
        }

        private void OpenTextureDialog()
        {
            _fileDialog?.PopupCenteredRatio(0.75f);
        }

        private void OnTextureSelected(string path)
        {
            Texture2D texture = ResourceLoader.Load<Texture2D>(path);
            if (texture == null)
            {
                GD.PushWarning($"无法加载图片: {path}");
                return;
            }

            _texturePath = path;
            _textureSize = texture.GetSize();

            if (_pathLabel != null)
            {
                _pathLabel.Text = path;
            }

            SetStatus($"已选择图片: {path}");
        }

        private void OnTypeSelected(long index)
        {
            _currentType = index == 1 ? PolygonAreaType.Occlusion : PolygonAreaType.Collision;
        }

        private void BindSelectedRoot()
        {
            Godot.Collections.Array<Node> selectedNodes = EditorPlugin.GetEditorInterface().GetSelection().GetSelectedNodes();
            if (selectedNodes.Count > 0)
            {
                if (selectedNodes[0] is Polygon2D polygonNode && polygonNode.GetParent() is Node2D parent)
                {
                    SetEditRoot(parent);
                    SelectEditorNode(polygonNode);
                    return;
                }

                if (selectedNodes[0] is Node2D node2D)
                {
                    SetEditRoot(node2D);
                    return;
                }
            }

            Node editedSceneRoot = EditorPlugin.GetEditorInterface().GetEditedSceneRoot();
            if (editedSceneRoot is Node2D sceneNode2D)
            {
                SetEditRoot(sceneNode2D);
                return;
            }

            GD.PushWarning("请先在场景树中选择一个 Node2D 作为编辑根节点。");
        }

        private void SetEditRoot(Node2D root)
        {
            _editRoot = root;
            if (_rootLabel != null)
            {
                _rootLabel.Text = $"编辑根节点：{root.GetPath()}";
            }

            RefreshPolygonList();
            SetStatus($"已绑定编辑根节点: {root.Name}");
        }

        private void CreatePolygonNode()
        {
            if (!EnsureEditRoot())
            {
                return;
            }

            PolygonAreaType type = GetCurrentType();
            var polygon = new Polygon2D
            {
                Name = GetNextPolygonName(type),
                Color = GetAreaColor(type),
                Polygon = CreateDefaultPolygon(),
            };
            MarkPolygonNode(polygon, type);

            _editRoot!.AddChild(polygon);
            TrySetOwner(polygon);
            RefreshPolygonList();
            SelectEditorNode(polygon);
            SetStatus($"已新建区域: {polygon.Name}");
        }

        private void DeleteSelectedPolygon()
        {
            Polygon2D? polygon = GetSelectedPolygonNode();
            if (polygon == null)
            {
                GD.PushWarning("请先在列表或场景树中选择一个多边形区域。");
                return;
            }

            string nodeName = polygon.Name;
            polygon.GetParent()?.RemoveChild(polygon);
            polygon.QueueFree();
            RefreshPolygonList();
            SetStatus($"已删除区域: {nodeName}");
        }

        private void OnPolygonSelected(long index)
        {
            List<Polygon2D> polygons = GetPolygonNodes();
            if (index < 0 || index >= polygons.Count)
            {
                return;
            }

            SelectEditorNode(polygons[(int)index]);
        }

        private void RefreshPolygonList()
        {
            if (_polygonList == null)
            {
                return;
            }

            _polygonList.Clear();

            foreach (Polygon2D polygon in GetPolygonNodes())
            {
                PolygonAreaType type = GetPolygonType(polygon);
                _polygonList.AddItem($"{polygon.Name} [{type}] {polygon.Polygon.Length}");
            }
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(_texturePath))
            {
                GD.PushWarning("请先选择图片。");
                return;
            }

            if (!EnsureEditRoot())
            {
                return;
            }

            string savePath = GetJsonPath(_texturePath);
            string globalPath = ProjectSettings.GlobalizePath(savePath);
            string? directory = Path.GetDirectoryName(globalPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            MapPolygonDocument document = CreateDocument();
            string json = JsonSerializer.Serialize(document, MapPolygonJson.Options);
            File.WriteAllText(globalPath, json, Encoding.UTF8);
            GD.Print($"保存多边形区域: {savePath}");
            SetStatus($"已保存: {savePath}");
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(_texturePath))
            {
                GD.PushWarning("请先选择图片。");
                return;
            }

            if (!EnsureEditRoot())
            {
                return;
            }

            string savePath = GetJsonPath(_texturePath);
            string globalPath = ProjectSettings.GlobalizePath(savePath);
            if (!File.Exists(globalPath))
            {
                GD.PushWarning($"未找到多边形 JSON: {savePath}");
                return;
            }

            string json = File.ReadAllText(globalPath, Encoding.UTF8);
            MapPolygonDocument? document = JsonSerializer.Deserialize<MapPolygonDocument>(json, MapPolygonJson.Options);
            if (document == null)
            {
                GD.PushWarning($"无法读取多边形区域: {savePath}");
                return;
            }

            ClearManagedPolygonNodes();
            foreach (MapPolygonData data in document.Polygons)
            {
                var polygon = new Polygon2D
                {
                    Name = string.IsNullOrWhiteSpace(data.Name) ? GetNextPolygonName(data.Type) : data.Name,
                    Color = GetAreaColor(data.Type),
                    Polygon = data.Points.ToArray(),
                };
                MarkPolygonNode(polygon, data.Type);

                _editRoot!.AddChild(polygon);
                TrySetOwner(polygon);
            }

            RefreshPolygonList();
            GD.Print($"加载多边形区域: {savePath}");
            SetStatus($"已加载: {savePath}");
        }

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

        private List<Vector2> GetPointsInRootSpace(Polygon2D polygon)
        {
            var points = new List<Vector2>();
            Transform2D rootTransform = _editRoot!.GlobalTransform.AffineInverse();

            foreach (Vector2 point in polygon.Polygon)
            {
                Vector2 globalPoint = polygon.GlobalTransform * point;
                points.Add(rootTransform * globalPoint);
            }

            return points;
        }

        private List<Polygon2D> GetPolygonNodes()
        {
            var polygons = new List<Polygon2D>();
            if (_editRoot == null)
            {
                return polygons;
            }

            CollectPolygonNodes(_editRoot, polygons);
            return polygons;
        }

        private void CollectPolygonNodes(Node node, List<Polygon2D> polygons)
        {
            foreach (Node child in node.GetChildren())
            {
                if (child is Polygon2D polygon && IsManagedPolygonNode(polygon))
                {
                    polygons.Add(polygon);
                }

                CollectPolygonNodes(child, polygons);
            }
        }

        private Polygon2D? GetSelectedPolygonNode()
        {
            Godot.Collections.Array<Node> selectedNodes = EditorPlugin.GetEditorInterface().GetSelection().GetSelectedNodes();
            if (selectedNodes.Count > 0 && selectedNodes[0] is Polygon2D selectedPolygon && IsManagedPolygonNode(selectedPolygon))
            {
                return selectedPolygon;
            }

            if (_polygonList == null)
            {
                return null;
            }

            int[] selectedItems = _polygonList.GetSelectedItems();
            if (selectedItems.Length == 0)
            {
                return null;
            }

            List<Polygon2D> polygons = GetPolygonNodes();
            int index = (int)selectedItems[0];
            return index >= 0 && index < polygons.Count ? polygons[index] : null;
        }

        private bool EnsureEditRoot()
        {
            if (_editRoot != null && GodotObject.IsInstanceValid(_editRoot))
            {
                return true;
            }

            BindSelectedRoot();
            return _editRoot != null && GodotObject.IsInstanceValid(_editRoot);
        }

        private void SelectEditorNode(Node node)
        {
            EditorSelection selection = EditorPlugin.GetEditorInterface().GetSelection();
            selection.Clear();
            selection.AddNode(node);
        }

        private void ClearManagedPolygonNodes()
        {
            List<Polygon2D> polygons = GetPolygonNodes();
            foreach (Polygon2D polygon in polygons)
            {
                polygon.GetParent()?.RemoveChild(polygon);
                polygon.QueueFree();
            }
        }

        private void TrySetOwner(Node node)
        {
            Node sceneRoot = EditorPlugin.GetEditorInterface().GetEditedSceneRoot();
            if (sceneRoot != null && sceneRoot.IsAncestorOf(node))
            {
                node.Owner = sceneRoot;
            }
        }

        private void MarkPolygonNode(Polygon2D polygon, PolygonAreaType type)
        {
            polygon.SetMeta(EditorMetaKey, true);
            polygon.SetMeta(TypeMetaKey, type.ToString());
        }

        private bool IsManagedPolygonNode(Polygon2D polygon)
        {
            if (polygon.HasMeta(EditorMetaKey))
            {
                return true;
            }

            return polygon.Name.ToString().StartsWith($"{PolygonAreaType.Collision}_", StringComparison.Ordinal)
                   || polygon.Name.ToString().StartsWith($"{PolygonAreaType.Occlusion}_", StringComparison.Ordinal);
        }

        private PolygonAreaType GetPolygonType(Polygon2D polygon)
        {
            if (polygon.HasMeta(TypeMetaKey)
                && Enum.TryParse(polygon.GetMeta(TypeMetaKey).ToString(), out PolygonAreaType type))
            {
                return type;
            }

            string name = polygon.Name.ToString();
            if (name.StartsWith($"{PolygonAreaType.Occlusion}_", StringComparison.Ordinal))
            {
                return PolygonAreaType.Occlusion;
            }

            return PolygonAreaType.Collision;
        }

        private PolygonAreaType GetCurrentType()
        {
            if (_typeOption == null)
            {
                return _currentType;
            }

            return _typeOption.Selected == 1 ? PolygonAreaType.Occlusion : PolygonAreaType.Collision;
        }

        private string GetNextPolygonName(PolygonAreaType type)
        {
            int index = 1;
            string prefix = type.ToString();
            HashSet<string> existingNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (Polygon2D polygon in GetPolygonNodes())
            {
                existingNames.Add(polygon.Name);
            }

            string name;
            do
            {
                name = $"{prefix}_{index:000}";
                index++;
            } while (existingNames.Contains(name));

            return name;
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = message;
            }
        }

        private static Vector2[] CreateDefaultPolygon()
        {
            return new[]
            {
                new Vector2(0, 0),
                new Vector2(96, 0),
                new Vector2(96, 96),
                new Vector2(0, 96),
            };
        }

        private static string GetJsonPath(string texturePath)
        {
            string extension = Path.GetExtension(texturePath);
            if (string.IsNullOrEmpty(extension))
            {
                return $"{texturePath}.poly.json";
            }

            return $"{texturePath.Substring(0, texturePath.Length - extension.Length)}.poly.json";
        }

        private static Color GetAreaColor(PolygonAreaType type)
        {
            return type == PolygonAreaType.Occlusion
                ? new Color(0.0f, 0.75f, 1.0f, 0.35f)
                : new Color(1.0f, 0.25f, 0.1f, 0.35f);
        }
    }

    public enum PolygonAreaType
    {
        Collision,
        Occlusion,
    }

    public sealed class MapPolygonDocument
    {
        public string Texture { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MapPolygonData> Polygons { get; set; } = new List<MapPolygonData>();
    }

    public sealed class MapPolygonData
    {
        public string Name { get; set; } = string.Empty;
        public PolygonAreaType Type { get; set; }
        public List<Vector2> Points { get; set; } = new List<Vector2>();
    }

    public static class MapPolygonJson
    {
        public static readonly JsonSerializerOptions Options = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.Converters.Add(new Vector2JsonConverter());
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            return options;
        }
    }

    public sealed class Vector2JsonConverter : System.Text.Json.Serialization.JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Vector2 must be an array.");
            }

            reader.Read();
            float x = ReadFloat(ref reader);
            reader.Read();
            float y = ReadFloat(ref reader);
            reader.Read();

            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException("Vector2 array must contain two values.");
            }

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }

        private static float ReadFloat(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetSingle();
            }

            if (reader.TokenType == JsonTokenType.String && float.TryParse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return value;
            }

            throw new JsonException("Vector2 value must be a number.");
        }
    }
}
