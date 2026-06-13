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
        private OptionButton? _typeOption;
        private Label? _pathLabel;
        private ItemList? _polygonList;
        private MapPolygonCanvas? _canvas;
        private FileDialog? _fileDialog;
        private string _texturePath = string.Empty;

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
            var toolbar = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };

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

            var finishButton = new Button
            {
                Text = "完成多边形",
            };
            finishButton.Pressed += FinishPolygon;
            toolbar.AddChild(finishButton);

            var deleteButton = new Button
            {
                Text = "删除选中",
            };
            deleteButton.Pressed += DeleteSelectedPolygon;
            toolbar.AddChild(deleteButton);

            var saveButton = new Button
            {
                Text = "保存",
            };
            saveButton.Pressed += Save;
            toolbar.AddChild(saveButton);

            var loadButton = new Button
            {
                Text = "加载",
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

            return toolbar;
        }

        private Control BuildBody()
        {
            var split = new HSplitContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            };

            _canvas = new MapPolygonCanvas
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            };
            _canvas.PolygonsChanged += RefreshPolygonList;
            _canvas.SelectionChanged += RefreshPolygonList;
            split.AddChild(_canvas);

            _polygonList = new ItemList
            {
                CustomMinimumSize = new Vector2(180, 0),
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            };
            _polygonList.ItemSelected += OnPolygonSelected;
            split.AddChild(_polygonList);

            return split;
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
            if (_pathLabel != null)
            {
                _pathLabel.Text = path;
            }

            _canvas?.SetTexture(path, texture);
            Load();
        }

        private void OnTypeSelected(long index)
        {
            if (_canvas != null)
            {
                _canvas.CurrentType = index == 1 ? PolygonAreaType.Occlusion : PolygonAreaType.Collision;
            }
        }

        private void FinishPolygon()
        {
            _canvas?.FinishCurrentPolygon();
        }

        private void DeleteSelectedPolygon()
        {
            _canvas?.DeleteSelectedPolygon();
        }

        private void OnPolygonSelected(long index)
        {
            _canvas?.SelectPolygon((int)index);
        }

        private void RefreshPolygonList()
        {
            if (_polygonList == null || _canvas == null)
            {
                return;
            }

            _polygonList.Clear();

            IReadOnlyList<MapPolygonData> polygons = _canvas.Polygons;
            for (int i = 0; i < polygons.Count; i++)
            {
                MapPolygonData polygon = polygons[i];
                _polygonList.AddItem($"{polygon.Name} [{polygon.Type}] {polygon.Points.Count}");

                if (i == _canvas.SelectedPolygonIndex)
                {
                    _polygonList.Select(i);
                }
            }
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(_texturePath))
            {
                GD.PushWarning("请先选择图片。");
                return;
            }

            string savePath = GetJsonPath(_texturePath);
            string globalPath = ProjectSettings.GlobalizePath(savePath);
            string directory = Path.GetDirectoryName(globalPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (_canvas == null)
            {
                return;
            }

            MapPolygonDocument document = _canvas.CreateDocument();
            string json = JsonSerializer.Serialize(document, MapPolygonJson.Options);
            File.WriteAllText(globalPath, json, Encoding.UTF8);
            GD.Print($"保存多边形区域: {savePath}");
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(_texturePath))
            {
                GD.PushWarning("请先选择图片。");
                return;
            }

            string savePath = GetJsonPath(_texturePath);
            string globalPath = ProjectSettings.GlobalizePath(savePath);
            if (!File.Exists(globalPath))
            {
                _canvas?.ClearPolygons();
                RefreshPolygonList();
                return;
            }

            string json = File.ReadAllText(globalPath, Encoding.UTF8);
            MapPolygonDocument document = JsonSerializer.Deserialize<MapPolygonDocument>(json, MapPolygonJson.Options);
            if (document == null)
            {
                GD.PushWarning($"无法读取多边形区域: {savePath}");
                return;
            }

            _canvas?.LoadDocument(document);
            RefreshPolygonList();
            GD.Print($"加载多边形区域: {savePath}");
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
    }

    public sealed partial class MapPolygonCanvas : Control
    {
        private readonly List<MapPolygonData> _polygons = new List<MapPolygonData>();
        private readonly List<Vector2> _currentPoints = new List<Vector2>();
        private readonly List<Polygon2D> _polygonNodes = new List<Polygon2D>();
        private Texture2D? _texture;
        private string _texturePath = string.Empty;
        private Vector2 _textureSize;
        private Rect2 _imageRect;

        public event Action? PolygonsChanged;
        public event Action? SelectionChanged;

        public PolygonAreaType CurrentType { get; set; }
        public int SelectedPolygonIndex { get; private set; } = -1;
        public IReadOnlyList<MapPolygonData> Polygons => _polygons;

        public MapPolygonCanvas()
        {
            CustomMinimumSize = new Vector2(480, 360);
            MouseDefaultCursorShape = CursorShape.Cross;
            Resized += OnCanvasResized;
        }

        public void SetTexture(string path, Texture2D texture)
        {
            _texturePath = path;
            _texture = texture;
            _textureSize = texture.GetSize();
            _polygons.Clear();
            _currentPoints.Clear();
            SelectedPolygonIndex = -1;
            RefreshPolygonNodes();
            QueueRedraw();
            PolygonsChanged?.Invoke();
        }

        public void FinishCurrentPolygon()
        {
            if (_currentPoints.Count < 3)
            {
                return;
            }

            _polygons.Add(new MapPolygonData
            {
                Name = $"Polygon_{_polygons.Count + 1}",
                Type = CurrentType,
                Points = new List<Vector2>(_currentPoints),
            });

            _currentPoints.Clear();
            SelectedPolygonIndex = _polygons.Count - 1;
            RefreshPolygonNodes();
            QueueRedraw();
            PolygonsChanged?.Invoke();
        }

        public void DeleteSelectedPolygon()
        {
            if (SelectedPolygonIndex < 0 || SelectedPolygonIndex >= _polygons.Count)
            {
                return;
            }

            _polygons.RemoveAt(SelectedPolygonIndex);
            SelectedPolygonIndex = -1;
            RefreshPolygonNodes();
            QueueRedraw();
            PolygonsChanged?.Invoke();
        }

        public void SelectPolygon(int index)
        {
            if (index < 0 || index >= _polygons.Count)
            {
                return;
            }

            SelectedPolygonIndex = index;
            RefreshPolygonNodes();
            QueueRedraw();
            SelectionChanged?.Invoke();
        }

        public void ClearPolygons()
        {
            _polygons.Clear();
            _currentPoints.Clear();
            SelectedPolygonIndex = -1;
            RefreshPolygonNodes();
            QueueRedraw();
            PolygonsChanged?.Invoke();
        }

        public MapPolygonDocument CreateDocument()
        {
            return new MapPolygonDocument
            {
                Texture = _texturePath,
                Width = (int)_textureSize.X,
                Height = (int)_textureSize.Y,
                Polygons = new List<MapPolygonData>(_polygons),
            };
        }

        public void LoadDocument(MapPolygonDocument document)
        {
            _polygons.Clear();
            _currentPoints.Clear();
            SelectedPolygonIndex = -1;

            if (document.Polygons != null)
            {
                _polygons.AddRange(document.Polygons);
            }

            QueueRedraw();
            RefreshPolygonNodes();
            PolygonsChanged?.Invoke();
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (_texture == null)
            {
                return;
            }

            if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                Vector2 imagePoint = ToImagePoint(mouseButton.Position);
                if (!IsPointInTexture(imagePoint))
                {
                    return;
                }

                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    if (_currentPoints.Count >= 3 && imagePoint.DistanceTo(_currentPoints[0]) <= 8 / GetImageScale())
                    {
                        FinishCurrentPolygon();
                    }
                    else
                    {
                        _currentPoints.Add(imagePoint);
                        SelectedPolygonIndex = -1;
                        RefreshPolygonNodes();
                        QueueRedraw();
                        SelectionChanged?.Invoke();
                    }
                }
                else if (mouseButton.ButtonIndex == MouseButton.Right)
                {
                    if (!DeletePointAt(imagePoint))
                    {
                        SelectPolygonAt(imagePoint);
                    }
                }
            }
        }

        public override void _Draw()
        {
            if (_texture == null)
            {
                DrawString(ThemeDB.FallbackFont, new Vector2(24, 40), "请选择地图图片", HorizontalAlignment.Left, -1, 16, Colors.Gray);
                return;
            }

            UpdateImageRect();
            DrawTextureRect(_texture, _imageRect, false);

            for (int i = 0; i < _polygons.Count; i++)
            {
                DrawPolygonData(_polygons[i], i == SelectedPolygonIndex);
            }

            DrawCurrentPolygon();
        }

        private void DrawPolygonData(MapPolygonData polygon, bool selected)
        {
            if (polygon.Points == null || polygon.Points.Count == 0)
            {
                return;
            }

            Vector2[] screenPoints = ToScreenPoints(polygon.Points);
            Color color = GetAreaColor(polygon.Type);
            DrawPolyline(screenPoints, color, selected ? 3 : 2, true);

            foreach (Vector2 point in screenPoints)
            {
                DrawCircle(point, selected ? 5 : 4, color);
            }
        }

        private void DrawCurrentPolygon()
        {
            if (_currentPoints.Count == 0)
            {
                return;
            }

            Vector2[] screenPoints = ToScreenPoints(_currentPoints);
            DrawPolyline(screenPoints, Colors.Yellow, 2, false);

            foreach (Vector2 point in screenPoints)
            {
                DrawCircle(point, 4, Colors.Yellow);
            }
        }

        private void SelectPolygonAt(Vector2 imagePoint)
        {
            for (int i = _polygons.Count - 1; i >= 0; i--)
            {
                if (Geometry2D.IsPointInPolygon(imagePoint, _polygons[i].Points.ToArray()))
                {
                    SelectedPolygonIndex = i;
                    RefreshPolygonNodes();
                    QueueRedraw();
                    SelectionChanged?.Invoke();
                    return;
                }
            }

            SelectedPolygonIndex = -1;
            RefreshPolygonNodes();
            QueueRedraw();
            SelectionChanged?.Invoke();
        }

        private bool DeletePointAt(Vector2 imagePoint)
        {
            float radius = 8 / GetImageScale();

            for (int polygonIndex = _polygons.Count - 1; polygonIndex >= 0; polygonIndex--)
            {
                MapPolygonData polygon = _polygons[polygonIndex];
                for (int pointIndex = 0; pointIndex < polygon.Points.Count; pointIndex++)
                {
                    if (imagePoint.DistanceTo(polygon.Points[pointIndex]) <= radius)
                    {
                        polygon.Points.RemoveAt(pointIndex);
                        SelectedPolygonIndex = polygonIndex;

                        if (polygon.Points.Count < 3)
                        {
                            _polygons.RemoveAt(polygonIndex);
                            SelectedPolygonIndex = -1;
                        }

                        RefreshPolygonNodes();
                        QueueRedraw();
                        PolygonsChanged?.Invoke();
                        return true;
                    }
                }
            }

            return false;
        }

        private void OnCanvasResized()
        {
            RefreshPolygonNodes();
            QueueRedraw();
        }

        private void RefreshPolygonNodes()
        {
            ClearPolygonNodes();

            if (_texture == null)
            {
                return;
            }

            UpdateImageRect();

            for (int i = 0; i < _polygons.Count; i++)
            {
                MapPolygonData polygon = _polygons[i];
                if (polygon.Points == null || polygon.Points.Count < 3)
                {
                    continue;
                }

                Color color = GetAreaColor(polygon.Type);
                var polygonNode = new Polygon2D
                {
                    Polygon = ToScreenPoints(polygon.Points),
                    Color = new Color(color.R, color.G, color.B, i == SelectedPolygonIndex ? 0.35f : 0.18f),
                };

                _polygonNodes.Add(polygonNode);
                AddChild(polygonNode);
            }
        }

        private void ClearPolygonNodes()
        {
            foreach (Polygon2D node in _polygonNodes)
            {
                RemoveChild(node);
                node.QueueFree();
            }

            _polygonNodes.Clear();
        }

        private Vector2[] ToScreenPoints(IReadOnlyList<Vector2> points)
        {
            Vector2[] result = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                result[i] = ToScreenPoint(points[i]);
            }

            return result;
        }

        private Vector2 ToScreenPoint(Vector2 imagePoint)
        {
            return _imageRect.Position + imagePoint * GetImageScale();
        }

        private Vector2 ToImagePoint(Vector2 screenPoint)
        {
            UpdateImageRect();
            return (screenPoint - _imageRect.Position) / GetImageScale();
        }

        private bool IsPointInTexture(Vector2 imagePoint)
        {
            return imagePoint.X >= 0 && imagePoint.Y >= 0 && imagePoint.X <= _textureSize.X && imagePoint.Y <= _textureSize.Y;
        }

        private float GetImageScale()
        {
            if (_textureSize.X <= 0 || _textureSize.Y <= 0)
            {
                return 1;
            }

            float scaleX = Size.X / _textureSize.X;
            float scaleY = Size.Y / _textureSize.Y;
            return Mathf.Min(scaleX, scaleY);
        }

        private void UpdateImageRect()
        {
            float scale = GetImageScale();
            Vector2 drawSize = _textureSize * scale;
            _imageRect = new Rect2((Size - drawSize) * 0.5f, drawSize);
        }

        private static Color GetAreaColor(PolygonAreaType type)
        {
            return type == PolygonAreaType.Occlusion ? Colors.DeepSkyBlue : Colors.OrangeRed;
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
