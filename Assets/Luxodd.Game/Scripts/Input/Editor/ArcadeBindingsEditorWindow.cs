#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Luxodd.Game.Scripts.Input.Editor
{
    public class ArcadeBindingsEditorWindow : EditorWindow
    {
        private const string PrefKeyBindingsGuid = "Luxodd.Game.BindingsEditor.BindingsGUID";
        private const string PrefKeyPanelGuid    = "Luxodd.Game.BindingsEditor.PanelGUID";
        private const string PrefKeyConfigGuid   = "Luxodd.Game.BindingsEditor.ConfigGUID";
    
        private const string PrefKeyShowKeycodes  = "Luxodd.Game.BindingsEditor.ShowKeyCodes";
        private const string PrefKeyHighlight     = "Luxodd.Game.BindingsEditor.HighlightPressed";
        private const string PrefKeyShowStick     = "Luxodd.Game.BindingsEditor.ShowStick";
        private const string PrefKeyDrawStickArrow= "Luxodd.Game.BindingsEditor.DrawStickArrow";
        private const string PrefKeyArrowScale    = "Luxodd.Game.BindingsEditor.ArrowScale";
    
        // Panel image (controller layout)
        private Texture2D _panelTexture;

        // Bindings asset we edit
        private ArcadeBindingAsset _bindings;

        // Optional input config (axes / deadzone / invert) used only for preview in this editor window
        private ArcadeInputConfigAsset _inputConfig;

        private ArcadeButtonColor _selected = ArcadeButtonColor.Red;
        private Vector2 _scroll;

        // Normalized rects (0..1) relative to the panel image.
        // TODO: adjust to your actual panel image once and commit to SDK.
        private readonly Dictionary<ArcadeButtonColor, Rect> _buttonRects = new()
        {
            // Rect(x, y, w, h) normalized (0..1)
            { ArcadeButtonColor.Black,  new Rect(0.398f, 0.44f, 0.082f, 0.12f) },
            { ArcadeButtonColor.Red,    new Rect(0.488f, 0.41f, 0.082f, 0.12f) },
            { ArcadeButtonColor.Green,  new Rect(0.31f, 0.4f, 0.082f, 0.12f) },
            { ArcadeButtonColor.Yellow, new Rect(0.355f, 0.29f, 0.082f, 0.12f) },
            { ArcadeButtonColor.Blue,   new Rect(0.44f, 0.26f, 0.082f, 0.12f) },
            { ArcadeButtonColor.Purple, new Rect(0.525f, 0.29f, 0.082f, 0.12f) },
            { ArcadeButtonColor.Orange, new Rect(0.59f, 0.13f, 0.082f, 0.12f) },
            { ArcadeButtonColor.White,  new Rect(0.063f, 0.33f, 0.082f, 0.12f) },
        };

        // UI settings
        private bool _showUnityKeycodes = true;
        private bool _highlightPressedInPlayMode = true;

        // Stick preview settings
        private bool _showStickInPlayMode = true;
        private bool _drawStickArrowOnPanel = true;
        private float _stickArrowScalePx = 80f; // arrow length in pixels

        // “Listen & assign” mode
        private bool _listenForPress;
        private string _pendingLabel = "";

        [MenuItem("Luxodd Unity Plugin/Control/Bindings Editor")]
        public static void ShowWindow()
        {
            var w = GetWindow<ArcadeBindingsEditorWindow>("Arcade Bindings");
            w.minSize = new Vector2(980, 560);
        }

        private void OnEnable()
        {
            // Load last selected assets
            _bindings     = LoadAssetFromPrefs<ArcadeBindingAsset>(PrefKeyBindingsGuid);
            _panelTexture = LoadAssetFromPrefs<Texture2D>(PrefKeyPanelGuid);
            _inputConfig  = LoadAssetFromPrefs<ArcadeInputConfigAsset>(PrefKeyConfigGuid);

            if (_panelTexture == null)
            {
                _panelTexture = DefaultAssetResolver.LoadDefaultPanelTexture();
            }

            if (_inputConfig == null)
            {
                _inputConfig = DefaultAssetResolver.LoadDefaultInputConfig();
            }

            if (_bindings == null)
            {
                _bindings = DefaultAssetResolver.LoadDefaultBindings();
            }

            // Optional: load UI toggles
            _showUnityKeycodes = EditorPrefs.GetBool(PrefKeyShowKeycodes, true);
            _highlightPressedInPlayMode = EditorPrefs.GetBool(PrefKeyHighlight, true);
            _showStickInPlayMode = EditorPrefs.GetBool(PrefKeyShowStick, true);
            _drawStickArrowOnPanel = EditorPrefs.GetBool(PrefKeyDrawStickArrow, true);
            _stickArrowScalePx = EditorPrefs.GetFloat(PrefKeyArrowScale, _stickArrowScalePx);
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(PrefKeyShowKeycodes, _showUnityKeycodes);
            EditorPrefs.SetBool(PrefKeyHighlight, _highlightPressedInPlayMode);
            EditorPrefs.SetBool(PrefKeyShowStick, _showStickInPlayMode);
            EditorPrefs.SetBool(PrefKeyDrawStickArrow, _drawStickArrowOnPanel);
            EditorPrefs.SetFloat(PrefKeyArrowScale, _stickArrowScalePx);
        }

        private void OnGUI()
        {
            //DrawTopBar();

            if (_bindings == null)
            {
                EditorGUILayout.HelpBox("Assign an ArcadeBindingsAsset to edit.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanelImage();
                DrawRightInspector();
            }

            // Live repaint in play mode for highlights/stick preview
            if (Application.isPlaying && (_highlightPressedInPlayMode || _showStickInPlayMode))
                Repaint();
        }

        private void DrawTopBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var oldBindings = _bindings;
                
                _bindings = (ArcadeBindingAsset)EditorGUILayout.ObjectField(
                    new GUIContent("Bindings"),
                    _bindings,
                    typeof(ArcadeBindingAsset),
                    false);

                if (oldBindings != _bindings)
                {
                    SaveAssetToPrefs(PrefKeyBindingsGuid, _bindings);
                    Repaint();
                }

                GUILayout.Space(10);

                
                var oldPanel = _panelTexture;
                
                _panelTexture = (Texture2D)EditorGUILayout.ObjectField(
                    new GUIContent("Panel Image"),
                    _panelTexture,
                    typeof(Texture2D),
                    false);

                if (oldPanel != _panelTexture)
                {
                    SaveAssetToPrefs(PrefKeyPanelGuid, _panelTexture);
                    Repaint();
                }

                GUILayout.Space(10);

                var oldConfig = _inputConfig;
                _inputConfig = (ArcadeInputConfigAsset)EditorGUILayout.ObjectField(
                    new GUIContent("Input Config"),
                    _inputConfig,
                    typeof(ArcadeInputConfigAsset),
                    false);

                if (oldConfig != _inputConfig)
                {
                    SaveAssetToPrefs(PrefKeyConfigGuid, _inputConfig);
                    Repaint();
                }

                GUILayout.FlexibleSpace();

                _showUnityKeycodes = GUILayout.Toggle(_showUnityKeycodes, "Show KeyCodes", EditorStyles.toolbarButton);
                // _highlightPressedInPlayMode = GUILayout.Toggle(_highlightPressedInPlayMode, "Highlight Pressed (Play)", EditorStyles.toolbarButton);
                // _showStickInPlayMode = GUILayout.Toggle(_showStickInPlayMode, "Show Stick (Play)", EditorStyles.toolbarButton);

                if (GUILayout.Button("Select Asset", EditorStyles.toolbarButton))
                {
                    if (_bindings != null) Selection.activeObject = _bindings;
                }
            }
        }

        private void DrawLeftPanelImage()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.62f)))
            {
                Rect box = GUILayoutUtility.GetRect(10, 10, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                GUI.Box(box, GUIContent.none);

                if (_panelTexture == null)
                {
                    GUI.Label(box, "Assign Panel Image (Texture2D) to display the controller layout.", CenteredLabel());
                    return;
                }

                // Fit texture into box (keep aspect)
                Rect imgRect = FitRect(box, _panelTexture.width, _panelTexture.height);
                GUI.DrawTexture(imgRect, _panelTexture, ScaleMode.ScaleToFit);

                // Optional stick arrow overlay
                if (Application.isPlaying && _showStickInPlayMode && _drawStickArrowOnPanel)
                {
                    var stick = ReadStickPreview();
                    DrawStickArrow(imgRect, stick.Vector, _stickArrowScalePx);
                }

                // Draw clickable button zones
                foreach (var kv in _buttonRects)
                {
                    ArcadeButtonColor button = kv.Key;
                    Rect n = kv.Value;
                    Rect r = NormalizedToRect(imgRect, n);

                    bool pressed = Application.isPlaying && _highlightPressedInPlayMode && GetPressed(button);

                    DrawButtonZone(r, pressed, button == _selected);

                    // Handle click selection
                    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    {
                        _selected = button;
                        GUI.FocusControl(null);
                        Repaint();
                        Event.current.Use();
                    }

                    // Label overlay (only if assigned)
                    string label = _bindings.GetLabel(button);
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        var labelRect = new Rect(r.x, r.yMax + 2, Mathf.Max(80, r.width), 18);
                        GUI.Label(labelRect, label, SmallTagStyle());
                    }

                    // KeyCode hint
                    if (_showUnityKeycodes)
                    {
                        string kc = ArcadeUnityMapping.GetKeyCode(button).ToString();
                        var kcRect = new Rect(r.x, r.y - 18, Mathf.Max(80, r.width), 18);
                        GUI.Label(kcRect, kc, SmallMutedStyle());
                    }
                }
            }
        }

        private void DrawRightInspector()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);

                EditorGUILayout.LabelField("Selected Button", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(_selected.ToString());

                EditorGUILayout.Space(6);

                EditorGUILayout.LabelField("Unity KeyCode", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(ArcadeUnityMapping.GetKeyCode(_selected).ToString());

                EditorGUILayout.Space(12);

                EditorGUILayout.LabelField("Action Label (shown in overlay/help)", EditorStyles.boldLabel);

                string current = _bindings.GetLabel(_selected);
                string next = EditorGUILayout.TextField(current);

                if (next != current)
                {
                    Undo.RecordObject(_bindings, "Change Arcade Binding Label");
                    _bindings.SetLabel(_selected, next);
                    EditorUtility.SetDirty(_bindings);
                }

                EditorGUILayout.Space(8);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Clear Label"))
                    {
                        Undo.RecordObject(_bindings, "Clear Arcade Binding Label");
                        _bindings.SetLabel(_selected, "");
                        EditorUtility.SetDirty(_bindings);
                        AssetDatabase.SaveAssets();
                    }

                    if (GUILayout.Button(_listenForPress ? "Listening..." : "Listen & Assign"))
                    {
                        _listenForPress = !_listenForPress;
                        _pendingLabel = _bindings.GetLabel(_selected);
                    }
                }

                if (_listenForPress)
                {
                    EditorGUILayout.HelpBox(
                        "Press a physical arcade button now (in Play Mode). The first detected button press will be assigned.\n" +
                        "Tip: Use this to verify which physical button maps to which Unity KeyCode.",
                        MessageType.Info);

                    _pendingLabel = EditorGUILayout.TextField("Label to set", _pendingLabel);

                    if (!Application.isPlaying)
                    {
                        EditorGUILayout.HelpBox("Enter Play Mode to capture button presses.", MessageType.Warning);
                    }
                    else
                    {
                        var detected = DetectFirstDown();
                        if (detected.HasValue)
                        {
                            Undo.RecordObject(_bindings, "Listen & Assign Arcade Binding Label");
                            _bindings.SetLabel(detected.Value, _pendingLabel);
                            EditorUtility.SetDirty(_bindings);

                            _selected = detected.Value;
                            _listenForPress = false;
                            Repaint();
                        }
                    }
                }

                EditorGUILayout.Space(16);

                DrawStickPreviewPanel();

                EditorGUILayout.Space(16);
                EditorGUILayout.LabelField("Quick Fill", EditorStyles.boldLabel);

                if (GUILayout.Button("Fill common labels (example)"))
                {
                    Undo.RecordObject(_bindings, "Quick Fill Arcade Bindings");
                    _bindings.SetLabel(ArcadeButtonColor.Red, "Shoot");
                    _bindings.SetLabel(ArcadeButtonColor.Green, "Jump");
                    _bindings.SetLabel(ArcadeButtonColor.Yellow, "");
                    EditorUtility.SetDirty(_bindings);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawStickPreviewPanel()
        {
            EditorGUILayout.LabelField("Joystick Stick (Play Mode)", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                _drawStickArrowOnPanel = EditorGUILayout.Toggle("Draw Arrow on Panel", _drawStickArrowOnPanel);
                _stickArrowScalePx = EditorGUILayout.Slider("Arrow Scale (px)", _stickArrowScalePx, 20f, 160f);
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see live stick values.", MessageType.Info);
                return;
            }

            var stick = ReadStickPreview();

            EditorGUILayout.LabelField("Axis Names",
                $"{GetAxisXName()} / {GetAxisYName()}");

            EditorGUILayout.LabelField("Config",
                _inputConfig ? $"deadZone={_inputConfig.DeadZone:0.00}, invertX={_inputConfig.InvertX}, invertY={_inputConfig.InvertY}"
                    : "default (deadZone=0.15, invertY=true)");

            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Stick Vector",
                $"X={stick.X:0.000}, Y={stick.Y:0.000}  |  mag={stick.Magnitude:0.000}");
        }

        // Input helpers 

        private bool GetPressed(ArcadeButtonColor b)
        {
            return UnityEngine.Input.GetKey(ArcadeUnityMapping.GetKeyCode(b));
        }

        private ArcadeButtonColor? DetectFirstDown()
        {
            foreach (ArcadeButtonColor b in Enum.GetValues(typeof(ArcadeButtonColor)))
            {
                if (UnityEngine.Input.GetKeyDown(ArcadeUnityMapping.GetKeyCode(b)))
                    return b;
            }
            return null;
        }

        private ArcadeStick ReadStickPreview()
        {
            // This mirrors ArcadeControls.GetStick(), but does NOT modify ArcadeControls.Config globally.
            string xAxis = GetAxisXName();
            string yAxis = GetAxisYName();

            float deadZone = _inputConfig ? _inputConfig.DeadZone : 0.15f;
            bool invX = _inputConfig && _inputConfig.InvertX;
            bool invY = _inputConfig ? _inputConfig.InvertY : true;

            float x = SafeGetAxisRaw(xAxis);
            float y = SafeGetAxisRaw(yAxis);

            if (invX) x = -x;
            if (invY) y = -y;

            var v = ApplyDeadZone(new Vector2(x, y), deadZone);

            return new ArcadeStick(v.x, v.y);
        }

        private string GetAxisXName() => _inputConfig ? _inputConfig.HorizontalAxisName : "Horizontal";
        private string GetAxisYName() => _inputConfig ? _inputConfig.VerticalAxisName : "Vertical";

        private static float SafeGetAxisRaw(string axisName)
        {
            try { return UnityEngine.Input.GetAxisRaw(axisName); }
            catch { return 0f; }
        }

        private static Vector2 ApplyDeadZone(Vector2 v, float deadZone)
        {
            if (deadZone <= 0f) return v;

            float mag = v.magnitude;
            if (mag < deadZone) return Vector2.zero;

            float scaled = (mag - deadZone) / (1f - deadZone);
            return v.normalized * Mathf.Clamp01(scaled);
        }

        // ======== Drawing helpers ========

        private static GUIStyle CenteredLabel()
        {
            var s = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            return s;
        }

        private static GUIStyle SmallTagStyle()
        {
            var s = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };
            s.normal.textColor = Color.white;
            return s;
        }

        private static GUIStyle SmallMutedStyle()
        {
            var s = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
            s.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
            return s;
        }

        private static void DrawButtonZone(Rect r, bool pressed, bool selected)
        {
            Color fill = pressed ? new Color(0f, 1f, 0f, 0.18f) : new Color(0f, 0f, 0f, 0.12f);
            if (selected) fill = new Color(1f, 0.8f, 0f, 0.22f);

            EditorGUI.DrawRect(r, fill);
            DrawRectBorder(r, selected ? Color.yellow : Color.white, 1);
        }

        private static void DrawRectBorder(Rect rect, Color color, int thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static Rect FitRect(Rect outer, float w, float h)
        {
            float aspect = w / h;
            float outerAspect = outer.width / outer.height;

            if (outerAspect > aspect)
            {
                float width = outer.height * aspect;
                float x = outer.x + (outer.width - width) * 0.5f;
                return new Rect(x, outer.y, width, outer.height);
            }
            else
            {
                float height = outer.width / aspect;
                float y = outer.y + (outer.height - height) * 0.5f;
                return new Rect(outer.x, y, outer.width, height);
            }
        }

        private static Rect NormalizedToRect(Rect imageRect, Rect normalized)
        {
            return new Rect(
                imageRect.x + normalized.x * imageRect.width,
                imageRect.y + normalized.y * imageRect.height,
                normalized.width * imageRect.width,
                normalized.height * imageRect.height
            );
        }

        private static void DrawStickArrow(Rect imageRect, Vector2 stick, float scalePx)
        {
            // Arrow from center of the image
            Vector2 center = imageRect.center;
            Vector2 dir = stick;

            // If stick is almost zero — don't draw
            if (dir.sqrMagnitude < 0.0001f) return;

            Vector2 end = center + dir * scalePx;

            Handles.BeginGUI();
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(3f, center, end);

            // small head
            Vector2 headDir = (center - end).normalized;
            Vector2 left = Quaternion.Euler(0, 0, 25f) * headDir;
            Vector2 right = Quaternion.Euler(0, 0, -25f) * headDir;
            Handles.DrawAAPolyLine(3f, end, end + left * 12f);
            Handles.DrawAAPolyLine(3f, end, end + right * 12f);
            Handles.EndGUI();
        }
    
        private static void SaveAssetToPrefs(string key, UnityEngine.Object obj)
        {
            if (obj == null)
            {
                EditorPrefs.DeleteKey(key);
                return;
            }

            string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);
            EditorPrefs.SetString(key, guid);
        }
    
        private static T LoadAssetFromPrefs<T>(string key) where T : UnityEngine.Object
        {
            if (!EditorPrefs.HasKey(key))
                return null;

            string guid = EditorPrefs.GetString(key, "");
            if (string.IsNullOrWhiteSpace(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
#endif
