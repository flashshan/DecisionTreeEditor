using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;


public class DecisionTreeEditor : EditorWindow
{
    class LayerInfo
    {
        public string layerName;
        public DTNode treeNode;
        public uint updateInterval;
        public bool active;

        public LayerInfo(string i_layerName, DTNode i_treeNode, uint i_updateInterval = 1, bool i_active = true)
        {
            layerName = i_layerName;
            treeNode = i_treeNode;
            updateInterval = i_updateInterval;
            active = i_active;
        }
    }

    static List<Type> enterConditionTypes = new List<Type>();
    static List<Type> resultTypes = new List<Type>();

    int curModId = -1;
    int curLayerId = -1;
    int nextLayerId = -1;
    int curTreeNodeId = -1;

    static string[] modNames = { "Base" };
    string[] layerGUIs = null;
    string[] treeNodeGUIs = null;
    string[] enterConditionGUIs = null;
    string[] resultGUIs = null;

    Vector2 decisionTreeScrollPos = Vector2.zero;
    Vector2 mainTreeScrollPos = Vector2.zero;

    XmlDocument xmlDoc = null;

    List<LayerInfo> layers = new List<LayerInfo>();
    //string renameLayerName;

    List<DTNode> currentNodes;
    List<bool> nodeExpandState = new List<bool>();

    string newLayerName = "";
    string newTreeNodeName = "";
    const string treeRootName = "Root";

    List<int> treePathIds = new List<int>();
    List<string> treePathNames = new List<string>();

    DTNode copiedNode = null;

    #region Style
    // button styles
    GUIStyle largeLabelStyle = null;
    GUIStyle middleLabelStyle = null;
    GUIStyle miniLabelStyle = null;
    GUIStyle inputStyle = null;
    GUIStyle buttonStyle = null;
    GUIStyle DisabledInputStyle = null;
    GUIStyle indexStyle = null;
    #endregion

    [MenuItem("Utilities/Decision Tree/Clean DT Backup Files")]
    public static void CleanBackup()
    {
        for (int i = 0; i < modNames.Length; i++)
        {
            string path = Application.dataPath + "/Resources/DecisionTree/Backup/";

            string[] filePaths = Directory.GetFiles(path);
            foreach (string filePath in filePaths)
            {
                if (filePath.Contains(".bak"))
                    File.Delete(filePath);
            }
        }
    }

    [MenuItem("Utilities/Decision Tree/Decision Tree Editor")]
    public static void ShowWindow()
    {
        System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type[] types = assemblies[i].GetTypes();

            for (int j = 0; j < types.Length; j++)
            {
                if (!types[j].IsAbstract)
                {
                    string className = types[j].Name;
                    if (types[j].IsSubclassOf(typeof(BasicCondition)))
                        enterConditionTypes.Add(types[j]);
                    else if (types[j].IsSubclassOf(typeof(BasicResult)))
                        resultTypes.Add(types[j]);
                }
            }
        }

        EditorWindow window = EditorWindow.GetWindow(typeof(DecisionTreeEditor), false, "DecisionTree Editor");
        window.autoRepaintOnSceneChange = true;
    }

    void OnDestroy()
    {
        if (EditorUtility.DisplayDialog("Notification", "Save changes?", "OK", "Cancel"))
        {
            SaveToFile();
        }
    }


    void OnGUI()
    {
        #region create styles

        if (largeLabelStyle == null)
        {
            largeLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            largeLabelStyle.alignment = TextAnchor.MiddleLeft;
            largeLabelStyle.fontSize = 20;
        }
        if (middleLabelStyle == null)
        {
            middleLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            largeLabelStyle.alignment = TextAnchor.MiddleLeft;
            largeLabelStyle.fontSize = 15;
        }
        if (miniLabelStyle == null)
        {
            miniLabelStyle = new GUIStyle(EditorStyles.miniLabel);
            miniLabelStyle.alignment = TextAnchor.MiddleLeft;
            miniLabelStyle.fontSize = 12;
        }
        if (inputStyle == null)
        {
            inputStyle = new GUIStyle(EditorStyles.textField);
            inputStyle.alignment = TextAnchor.MiddleLeft;
            inputStyle.fontSize = 12;
        }
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(EditorStyles.miniButtonMid);
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.fontSize = 12;
        }
        if (DisabledInputStyle == null)
        {
            DisabledInputStyle = new GUIStyle(EditorStyles.textField);
            DisabledInputStyle.alignment = TextAnchor.MiddleLeft;
            DisabledInputStyle.fontSize = 12;
            DisabledInputStyle.fontStyle = FontStyle.Bold;
        }
        if (indexStyle == null)
        {
            indexStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            indexStyle.alignment = TextAnchor.MiddleCenter;
            indexStyle.fontSize = 12;
        }

        if (layerGUIs == null)
        {
            layerGUIs = new string[5];
            layerGUIs[0] = "Layer Options";
            layerGUIs[1] = "";
            layerGUIs[2] = "GetName";
            layerGUIs[3] = "Rename";
            layerGUIs[4] = "Add Layer";
        }
        if (treeNodeGUIs == null)
        {
            treeNodeGUIs = new string[5];
            treeNodeGUIs[0] = "Node Options";
            treeNodeGUIs[1] = "";
            treeNodeGUIs[2] = "GetName";
            treeNodeGUIs[3] = "Rename";
            treeNodeGUIs[4] = "Add Node";
        }
        if (enterConditionGUIs == null)
        {
            enterConditionGUIs = new string[enterConditionTypes.Count + 2];
            enterConditionGUIs[0] = " + Condition";
            enterConditionGUIs[1] = "";
            for (int i = 0; i < enterConditionTypes.Count; i++)
            {
                enterConditionGUIs[i + 2] = enterConditionTypes[i].Name;
            }
        }
        if (resultGUIs == null)
        {
            resultGUIs = new string[resultTypes.Count + 2];
            resultGUIs[0] = " + Result";
            resultGUIs[1] = "";
            for (int i = 0; i < resultTypes.Count; i++)
            {
                resultGUIs[i + 2] = resultTypes[i].Name;
            }
        }

        if (xmlDoc == null)
        {
            xmlDoc = new XmlDocument();
        }

        #endregion

        const float leftColumeWidth = 550.0f;

        Color defaultGuiColor = GUI.backgroundColor;
        EditorGUILayout.BeginVertical(GUILayout.Width(leftColumeWidth), GUILayout.Height(Screen.height));
        {
            #region LayerTitle
            GUILayout.BeginHorizontal();
            {
                GUI.backgroundColor = Color.cyan;
                GUILayout.Label("Mod", buttonStyle, GUILayout.Height(20), GUILayout.Width(150));

                GUI.backgroundColor = Color.white;
                int prevModId = curModId;
                curModId = EditorGUILayout.Popup(curModId, modNames, buttonStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true));
                if (prevModId != curModId || curModId == -1)
                    InitializeLayersByMod();

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Save", buttonStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    for (int i = 0; i < layers.Count; ++i)
                    {
                        DTNode.Init(layers[i].treeNode);
                    }
                    SaveToFile();
                }

                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
            #endregion

            decisionTreeScrollPos = EditorGUILayout.BeginScrollView(decisionTreeScrollPos, false, true);
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    #region Each Layer Line
                    GUILayout.BeginHorizontal();
                    {
                        GUI.backgroundColor = defaultGuiColor;
                        GUILayout.Label(i.ToString() + " )", indexStyle, GUILayout.Width(40), GUILayout.Height(20));

                        if (curLayerId == i)
                            GUI.backgroundColor = Color.blue;
                        else
                            GUI.backgroundColor = Color.gray;

                        layers[i].layerName = GUILayout.TextField(layers[i].layerName, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
                        GUILayout.Space(3);

                        layers[i].active = GUILayout.Toggle(layers[i].active, "", GUILayout.Width(20), GUILayout.Height(20));

                        GUILayout.Space(3);
                        GUILayout.Label("Interval: ",  miniLabelStyle, GUILayout.Width(60), GUILayout.Height(20));
                        layers[i].updateInterval = uint.Parse(GUILayout.TextField(layers[i].updateInterval.ToString(), inputStyle, GUILayout.Width(50), GUILayout.Height(20)));
                        
                        GUILayout.Space(10);
                        if (curLayerId != i)
                            GUI.backgroundColor = Color.green;
                        else
                            GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(curLayerId != i ? "Open" : "Close", buttonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                        {
                            if (curLayerId == i)
                            {
                                nextLayerId = -1;
                            }
                            else
                            {
                                nextLayerId = i;
                            }
                            mainTreeScrollPos.y = 0.0f;
                        }
                        GUILayout.Space(3);
                        GUI.backgroundColor = Color.cyan;
                        if (GUILayout.Button("∧", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                        {
                            if (i > 0)
                            {
                                int upId = i - 1;
                                var item = layers[i];
                                layers[i] = layers[upId];
                                layers[upId] = item;
                                if (curLayerId == i)
                                {
                                    curLayerId = upId;
                                    nextLayerId = upId;
                                }
                                else if (curLayerId == upId)
                                {
                                    curLayerId = i;
                                    nextLayerId = i;
                                }
                            }
                        }

                        if (GUILayout.Button("∨", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                        {
                            if (i < layers.Count - 1)
                            {
                                int downId = i + 1;
                                var item = layers[i];
                                layers[i] = layers[downId];
                                layers[downId] = item;
                                if (curLayerId == i)
                                {
                                    curLayerId = downId;
                                    nextLayerId = curLayerId;
                                }
                                else if (curLayerId == downId)
                                {
                                    curLayerId = i;
                                    nextLayerId = curLayerId;
                                }
                            }
                        }

                        GUILayout.Space(3);
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                        {
                            if (EditorUtility.DisplayDialog("Notification", "Delete layer: " + layers[i].layerName + " ?", "OK", "Cancel"))
                            {
                                layers.RemoveAt(i);
                                if (curLayerId == i)
                                {
                                    nextLayerId = -1;
                                }
                                else if (curLayerId > i)
                                {
                                    --curLayerId;
                                    nextLayerId = curLayerId;
                                }
                            }
                        }
                        GUI.backgroundColor = defaultGuiColor;
                    }
                    GUILayout.EndHorizontal();
                    #endregion

                    GUILayout.Space(5);
                }

                // handle layer change
                if (nextLayerId != curLayerId)
                {
                    ShowLayer(nextLayerId);
                    curLayerId = nextLayerId;
                }

                GUILayout.Space(15);
                GUILayout.Label("", GUI.skin.horizontalSlider);
                GUILayout.Space(10);

                #region Add Layer
                EditorGUILayout.BeginHorizontal();
                {
                    GUI.backgroundColor = defaultGuiColor;
                    newLayerName = EditorGUILayout.TextField(newLayerName, inputStyle, GUILayout.Height(27), GUILayout.ExpandWidth(true));

                    GUI.contentColor = Color.green;
                    if (GUILayout.Button("Add Layer", buttonStyle, GUILayout.MinHeight(27), GUILayout.Width(80)))
                    {
                        if (newLayerName.Length == 0)
                        {
                            if (curLayerId == -1)
                            {
                                layers.Add(new LayerInfo("newLayer", new DTNode(treeRootName)));
                            }
                            else
                            {
                                layers.Insert(curLayerId + 1, new LayerInfo("newLayer", new DTNode(treeRootName)));
                            }
                            newLayerName = "";
                        }
                        else
                        {
                            if (curLayerId == -1)
                            {
                                layers.Add(new LayerInfo(newLayerName, new DTNode(treeRootName)));
                            }
                            else
                            {
                                layers.Insert(curLayerId + 1, new LayerInfo(newLayerName, new DTNode(treeRootName)));
                            }
                            newLayerName = "";
                        }
                    }
                    GUI.contentColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();
                #endregion

                GUILayout.Label("", GUILayout.Height(Screen.height));
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = defaultGuiColor;

        #region Tree Path
        GUILayout.BeginArea(new Rect(leftColumeWidth + 20, 0, Screen.width - leftColumeWidth - 20, 30));
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(Screen.width - leftColumeWidth - 42), GUILayout.MinHeight(25));
            {
                for (int i = 0; i < treePathNames.Count; i++)
                {
                    if (i != 0)
                    {
                        GUILayout.Label("->", miniLabelStyle, GUILayout.Width(20), GUILayout.MinHeight(20));
                    }
                    if (GUILayout.Button(new GUIContent(treePathNames[i]), buttonStyle, GUILayout.Width(16 + treePathNames[i].Length * 8), GUILayout.MinHeight(20)))
                    {
                        treePathIds.RemoveRange(i, treePathIds.Count - i);
                        UpdateTreePath();
                        mainTreeScrollPos.y = 0.0f;
                    }
                }
            }
            GUILayout.EndHorizontal();
        };
        GUILayout.EndArea();
        #endregion

        mainTreeScrollPos = GUI.BeginScrollView(new Rect(leftColumeWidth + 20, 30, Screen.width - leftColumeWidth - 20, Screen.height - 30), mainTreeScrollPos, new Rect(0, Screen.height - 30, Screen.width - leftColumeWidth - 20, Screen.height * 3), false, true);
        {
            GUILayout.BeginVertical(GUILayout.Width(Screen.width - leftColumeWidth - 42));
            {
                if (currentNodes != null)
                {
                    GUILayout.Label("Tree Node(s):", largeLabelStyle);
                    for (int i = 0; i < currentNodes.Count; i++)
                    {
                        #region Each Tree Node Line
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(i.ToString() + " )", indexStyle, GUILayout.Width(40), GUILayout.Height(20));

                            if (curTreeNodeId == i)
                                GUI.backgroundColor = Color.blue;
                            else if (nodeExpandState[i])
                                GUI.backgroundColor = Color.green;
                            else
                                GUI.backgroundColor = Color.gray;
                            currentNodes[i].nodeName_ = GUILayout.TextField(currentNodes[i].nodeName_, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));

                            if (!nodeExpandState[i])
                                GUI.backgroundColor = Color.green;
                            else
                                GUI.backgroundColor = Color.red;

                            GUILayout.Space(5);
                            if (GUILayout.Button((!nodeExpandState[i] ? "Expand" : "Eclipse") + " || Condition(s): " + currentNodes[i].conditions_.Count.ToString() + " | Result(s): " + currentNodes[i].results_.Count.ToString(), buttonStyle, GUILayout.Width(270), GUILayout.Height(20)))
                            {
                                nodeExpandState[i] = !nodeExpandState[i];
                                curTreeNodeId = i;
                            }

                            GUILayout.Space(5);
                            GUI.backgroundColor = Color.yellow;
                            if (GUILayout.Button("Verify", buttonStyle, GUILayout.Width(60), GUILayout.Height(20)))
                            {
                                DTNode.Init(currentNodes[i]);
                            }

                            GUILayout.Space(5);
                            GUI.backgroundColor = Color.green;
                            if (GUILayout.Button("Child Node(s): " + currentNodes[i].subNodes_.Count.ToString(), buttonStyle, GUILayout.Width(150), GUILayout.Height(20)))
                            {
                                treePathIds.Add(i);
                                UpdateTreePath();
                                curTreeNodeId = -1;
                                mainTreeScrollPos.y = 0.0f;
                            }

                            GUILayout.Space(5);
                            GUI.backgroundColor = Color.yellow;
                            if (GUILayout.Button("Copy", buttonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                copiedNode = currentNodes[i];
                            }

                            GUILayout.Space(5);
                            GUI.backgroundColor = Color.cyan;
                            if (GUILayout.Button("∧", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                            {
                                if (i > 0)
                                {
                                    int upId = i - 1;
                                    var item = currentNodes[i];
                                    currentNodes[i] = currentNodes[upId];
                                    currentNodes[upId] = item;
                                    var state = nodeExpandState[i];
                                    nodeExpandState[i] = nodeExpandState[upId];
                                    nodeExpandState[upId] = state;

                                    if (curTreeNodeId == i)
                                    {
                                        curTreeNodeId = upId;
                                    }
                                }
                            }

                            if (GUILayout.Button("∨", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                            {
                                if (i < currentNodes.Count - 1)
                                {
                                    int downId = i + 1;
                                    var item = currentNodes[i];
                                    currentNodes[i] = currentNodes[downId];
                                    currentNodes[downId] = item;
                                    var state = nodeExpandState[i];
                                    nodeExpandState[i] = nodeExpandState[downId];
                                    nodeExpandState[downId] = state;

                                    if (curTreeNodeId == i)
                                    {
                                        curTreeNodeId = downId;
                                    }
                                }
                            }

                            GUILayout.Space(5);
                            GUI.backgroundColor = Color.red;
                            if (GUILayout.Button("X", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                            {
                                if (EditorUtility.DisplayDialog("Notification", "Delete Tree Node: " + currentNodes[i].nodeName_ + " ?", "OK", "Cancel"))
                                {
                                    currentNodes.RemoveAt(i);
                                    nodeExpandState.RemoveAt(i);
                                    if (curTreeNodeId == i)
                                    {
                                        curTreeNodeId = -1;
                                    }
                                    else if (curTreeNodeId > i)
                                    {
                                        --curTreeNodeId;
                                    }
                                }
                            }
                            GUI.backgroundColor = defaultGuiColor;
                        }
                        GUILayout.EndHorizontal();
                        #endregion

                        #region Expandable region
                        if (nodeExpandState.Count > i && nodeExpandState[i])
                        {
                            GUILayout.BeginVertical(GUILayout.Width(Screen.width - leftColumeWidth - 50));
                            {
                                GUILayout.Space(10);

                                GUI.backgroundColor = defaultGuiColor;
                                GUILayout.Label("Enter Condition(s):", middleLabelStyle);

                                if (currentNodes[i].conditions_.Count == 0)
                                {
                                    GUILayout.Label("Empty!", middleLabelStyle);
                                }
                                else
                                {
                                    for (int j = 0; j < currentNodes[i].conditions_.Count; j++)
                                    {
                                        bool removed = false;
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUILayout.Label("Condition Type: ", miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                                            GUILayout.Label(currentNodes[i].conditions_[j].GetType().ToString(), DisabledInputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));

                                            GUI.backgroundColor = Color.red;
                                            if (GUILayout.Button("X", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                                            {
                                                currentNodes[i].conditions_.RemoveAt(j);
                                                removed = true;
                                            }
                                        }
                                        GUILayout.EndHorizontal();
                                        if (!removed)
                                        {
                                            #region Handle EnterCondition
                                            BasicCondition tempCondition = currentNodes[i].conditions_[j];

                                            FieldInfo[] variables = tempCondition.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                            for (int vi = 0; vi < variables.Length; ++vi)
                                            {
                                                FieldInfo var = variables[vi];
                                                if (!Attribute.IsDefined(var, typeof(DtVariable)))
                                                    continue;

                                                DtVariable varAtr = Attribute.GetCustomAttribute(var, typeof(DtVariable)) as DtVariable;
                                                Type varType = var.FieldType;

                                                object varValue = var.GetValue(tempCondition);

                                                GUI.backgroundColor = defaultGuiColor;
                                                if (varType == typeof(bool))
                                                {
                                                    GUILayout.BeginHorizontal();
                                                    {
                                                        GUILayout.Label(varAtr.atrNameStr + ": ", miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                                                        var.SetValue(tempCondition, GUILayout.Toggle((bool)varValue, "", GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                    }
                                                    GUILayout.EndHorizontal();
                                                }
                                                if (varType == typeof(string))
                                                {
                                                    if (varValue == null)
                                                        varValue = "";

                                                    GUILayout.BeginHorizontal();
                                                    {
                                                        GUILayout.Label(varAtr.atrNameStr + ": ", miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                                                        var.SetValue(tempCondition, GUILayout.TextField(varValue as string, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                    }
                                                    GUILayout.EndHorizontal();
                                                }
                                                else if (varType == typeof(int))
                                                {
                                                    var.SetValue(tempCondition, EditorGUILayout.IntField(varAtr.atrNameStr + ": ", (int)varValue, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                }
                                                else if (varType == typeof(float))
                                                {
                                                    var.SetValue(tempCondition, EditorGUILayout.FloatField(varAtr.atrNameStr + ": ", (float)varValue, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                }
                                                else if (varType == typeof(Vector2))
                                                {
                                                    var.SetValue(tempCondition, DrawVector2(varAtr.atrNameStr + ": ", (Vector2)varValue));
                                                }
                                                else if (varType == typeof(Vector3))
                                                {
                                                    var.SetValue(tempCondition, DrawVector3(varAtr.atrNameStr + ": ", (Vector3)varValue));
                                                }
                                                else if (varType == typeof(Vector4))
                                                {
                                                    var.SetValue(tempCondition, DrawVector4(varAtr.atrNameStr + ": ", (Vector4)varValue));
                                                }
                                                else if (varType == typeof(Color))
                                                {
                                                    var.SetValue(tempCondition, EditorGUILayout.ColorField(varAtr.atrNameStr + ": ", (Color)varValue, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                }
                                                else if (varType.IsEnum)
                                                {
                                                    var.SetValue(tempCondition, EditorGUILayout.EnumPopup(varAtr.atrNameStr + ": ", varValue as Enum, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                }
                                                else if (varType.IsArray)
                                                {
                                                    IList tempList = (IList)varValue;
                                                    HandleArray(varAtr.atrNameStr + ": ", ref tempList, varType.GetElementType());
                                                    var.SetValue(tempCondition, tempList);
                                                }
                                            }
                                            #endregion
                                        }
                                        GUILayout.Space(5);
                                    }
                                }

                                GUI.backgroundColor = defaultGuiColor;
                                GUI.contentColor = Color.yellow;
                                int newConditionChoose = EditorGUILayout.Popup(0, enterConditionGUIs, buttonStyle, GUILayout.MinHeight(20), GUILayout.Width(120));

                                if (newConditionChoose >= 2)   // Add Condition
                                {
                                    currentNodes[i].conditions_.Add(Activator.CreateInstance(enterConditionTypes[newConditionChoose - 2]) as BasicCondition);
                                }
                                GUI.contentColor = Color.white;

                                GUILayout.Label("", GUI.skin.horizontalSlider);

                                GUILayout.Label("Result(s):", middleLabelStyle);
                                if (currentNodes[i].results_.Count == 0)
                                {
                                    GUILayout.Label("Empty!", middleLabelStyle);
                                }
                                else
                                {
                                    for (int j = 0; j < currentNodes[i].results_.Count; j++)
                                    {
                                        bool removed = false;
                                        GUILayout.BeginHorizontal();
                                        {
                                            GUI.backgroundColor = defaultGuiColor;
                                            GUILayout.Label("Result Type: ", miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                                            GUILayout.Label(currentNodes[i].results_[j].GetType().ToString(), DisabledInputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));

                                            GUI.backgroundColor = Color.red;
                                            if (GUILayout.Button("X", buttonStyle, GUILayout.Width(24), GUILayout.Height(20)))
                                            {
                                                currentNodes[i].results_.RemoveAt(j);
                                                removed = true;
                                            }
                                        }
                                        GUILayout.EndHorizontal();
                                        if (!removed)
                                        {
                                            GUI.backgroundColor = defaultGuiColor;
                                            BasicResult tempResult = currentNodes[i].results_[j];

                                            FieldInfo[] variables = tempResult.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                            for (int vi = 0; vi < variables.Length; ++vi)
                                            {
                                                FieldInfo var = variables[vi];
                                                if (!Attribute.IsDefined(var, typeof(DtVariable)))
                                                    continue;

                                                DtVariable varAtr = Attribute.GetCustomAttribute(var, typeof(DtVariable)) as DtVariable;
                                                Type varType = var.FieldType;

                                                object varValue = var.GetValue(tempResult);

                                                GUILayout.BeginHorizontal();
                                                {
                                                    GUI.backgroundColor = defaultGuiColor;
                                                    if (varType == typeof(bool))
                                                    {
                                                        GUILayout.BeginHorizontal();
                                                        {
                                                            GUILayout.Label(varAtr.atrNameStr + ": ", miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                                                            var.SetValue(tempResult, GUILayout.Toggle((bool)varValue, "", GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                        }
                                                        GUILayout.EndHorizontal();
                                                    }
                                                    else if (varType == typeof(string))
                                                    {
                                                        if (varValue == null)
                                                            varValue = "";
                                                        GUILayout.BeginHorizontal();
                                                        {
                                                            GUILayout.Label(varAtr.atrNameStr + ": ", miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                                                            var.SetValue(tempResult, GUILayout.TextField(varValue as string, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                        }
                                                        GUILayout.EndHorizontal();
                                                    }
                                                    else if (varType == typeof(int))
                                                    {
                                                        var.SetValue(tempResult, EditorGUILayout.IntField(varAtr.atrNameStr + ": ", (int)varValue, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                    }
                                                    else if (varType == typeof(float))
                                                    {
                                                        var.SetValue(tempResult, EditorGUILayout.FloatField(varAtr.atrNameStr + ": ", (float)varValue, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                    }
                                                    else if (varType == typeof(Vector2))
                                                    {
                                                        var.SetValue(tempResult, DrawVector2(varAtr.atrNameStr + ": ", (Vector2)varValue));
                                                    }
                                                    else if (varType == typeof(Vector3))
                                                    {
                                                        var.SetValue(tempResult, DrawVector3(varAtr.atrNameStr + ": ", (Vector3)varValue));
                                                    }
                                                    else if (varType == typeof(Vector4))
                                                    {
                                                        var.SetValue(tempResult, DrawVector4(varAtr.atrNameStr + ": ", (Vector4)varValue));
                                                    }
                                                    else if (varType == typeof(Color))
                                                    {
                                                        var.SetValue(tempResult, EditorGUILayout.ColorField(varAtr.atrNameStr + ": ", (Color)varValue, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                    }
                                                    else if (varType.IsEnum)
                                                    {
                                                        var.SetValue(tempResult, EditorGUILayout.EnumPopup(varAtr.atrNameStr + ": ", varValue as Enum, GUILayout.ExpandWidth(true), GUILayout.Height(20)));
                                                    }
                                                    else if (varType.IsArray)
                                                    {
                                                        IList tempList = (IList)varValue;
                                                        HandleArray(varAtr.atrNameStr + ": ", ref tempList, varType.GetElementType());
                                                        var.SetValue(tempResult, tempList);
                                                    }
                                                }
                                                GUILayout.EndHorizontal();
                                            }
                                            GUILayout.Space(5);
                                        }
                                    }
                                }

                                GUI.backgroundColor = defaultGuiColor;
                                GUI.contentColor = Color.green;
                                int newResultChoose = EditorGUILayout.Popup(0, resultGUIs, buttonStyle, GUILayout.MinHeight(20), GUILayout.Width(120));

                                if (newResultChoose >= 2)   // Add Result
                                {
                                    currentNodes[i].results_.Add(Activator.CreateInstance(resultTypes[newResultChoose - 2]) as BasicResult);
                                }
                                GUI.contentColor = Color.white;

                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.Space(10);
                        GUILayout.Label("", GUI.skin.horizontalSlider);
                        #endregion
                    }

                    GUILayout.Space(10);

                    #region Add Tree Node
                    EditorGUILayout.BeginHorizontal();
                    {
                        newTreeNodeName = EditorGUILayout.TextField(newTreeNodeName, inputStyle, GUILayout.Height(27), GUILayout.ExpandWidth(true));

                        GUI.contentColor = Color.green;
                        if (GUILayout.Button("Add TreeNode", buttonStyle, GUILayout.Height(27), GUILayout.Width(120)))
                        {
                            if (treePathNames.Count == 1 && currentNodes.Count != 0)
                            {
                                EditorUtility.DisplayDialog("Notification", "At most one root node on first level!", "OK");
                            }
                            else if (newTreeNodeName.Length == 0)
                            {
                                if (curTreeNodeId == -1)
                                {
                                    currentNodes.Add(new DTNode("newTreeNode"));
                                    nodeExpandState.Add(false);
                                }
                                else
                                {
                                    currentNodes.Insert(curTreeNodeId + 1, new DTNode("newTreeNode"));
                                    nodeExpandState.Insert(curTreeNodeId + 1, false);
                                }
                            }
                            else
                            {
                                if (curTreeNodeId == -1)
                                {
                                    currentNodes.Add(new DTNode(newTreeNodeName));
                                    nodeExpandState.Add(false);
                                }
                                else
                                {
                                    currentNodes.Insert(curTreeNodeId + 1, new DTNode(newTreeNodeName));
                                    nodeExpandState.Insert(curTreeNodeId + 1, false);
                                }
                                newTreeNodeName = "";
                            }
                        }
                        GUI.contentColor = Color.white;

                        if (copiedNode != null)
                        {
                            GUI.backgroundColor = Color.yellow;
                            if (GUILayout.Button("Paste", buttonStyle, GUILayout.Height(27), GUILayout.Width(60)))
                            {
                                if (treePathNames.Count == 1 && currentNodes.Count != 0)
                                {
                                    EditorUtility.DisplayDialog("Notification", "At most root node on first level!", "OK");
                                }
                                else
                                {
                                    if (curTreeNodeId == -1)
                                    {
                                        currentNodes.Add(DTNode.DeepCopy(copiedNode));
                                        nodeExpandState.Add(false);
                                    }
                                    else
                                    {
                                        currentNodes.Insert(curTreeNodeId + 1, DTNode.DeepCopy(copiedNode));
                                        nodeExpandState.Insert(curTreeNodeId + 1, false);
                                    }
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    #endregion
                }
            }
            GUILayout.EndVertical();
        }
        GUI.EndScrollView();
    }

    void HandleArray(string i_title, ref IList io_list, Type i_elementType)
    {
        int counters = 0;
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(i_title, miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
            GUILayout.Label("Size :", miniLabelStyle, GUILayout.Width(100), GUILayout.Height(20));
            counters = EditorGUILayout.IntField("", io_list.Count, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            counters = counters > 0 ? counters : 0;
            if (counters != io_list.Count)
            {
                IList newList = Array.CreateInstance(i_elementType, counters);
                for (int i = 0; i < io_list.Count && i < newList.Count; ++i)
                {
                    newList[i] = io_list[i];
                }
                io_list = newList;
            }
        }
        GUILayout.EndHorizontal();

        for (int iList = 0; iList < io_list.Count; ++iList)
        {
            if (i_elementType == typeof(string))
            {
                if (io_list[iList] == null)
                    io_list[iList] = "";

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("                         " + (iList + 1).ToString(), miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                    io_list[iList] = GUILayout.TextField(io_list[iList] as string, inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
                }
                GUILayout.EndHorizontal();
            }
            else if (i_elementType == typeof(bool))
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("                         " + (iList + 1).ToString(), miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
                    io_list[iList] = GUILayout.Toggle((bool)io_list[iList], "", GUILayout.ExpandWidth(true), GUILayout.Height(20));
                }
                GUILayout.EndHorizontal();
            }
            else if (i_elementType == typeof(int))
            {
                io_list[iList] = EditorGUILayout.IntField("                         " + (iList + 1).ToString(), (int)io_list[iList], inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            }
            else if (i_elementType == typeof(float))
            {
                io_list[iList] = EditorGUILayout.FloatField("                         " + (iList + 1).ToString(), (float)io_list[iList], inputStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            }
            else if (i_elementType == typeof(Vector2))
            {
                io_list[iList] = DrawVector2("                         " + (iList + 1).ToString(), (Vector2)io_list[iList]);
            }
            else if (i_elementType == typeof(Vector3))
            {
                io_list[iList] = DrawVector3("                         " + (iList + 1).ToString(), (Vector3)io_list[iList]);
            }
            else if (i_elementType == typeof(Vector4))
            {
                io_list[iList] = DrawVector4("                         " + (iList + 1).ToString(), (Vector4)io_list[iList]);
            }
            else if (i_elementType == typeof(Color))
            {
                io_list[iList] = EditorGUILayout.ColorField("                         " + (iList + 1).ToString(), (Color)io_list[iList], GUILayout.ExpandWidth(true), GUILayout.Height(20));
            }
            else if (i_elementType.IsEnum)
            {
                io_list[iList] = EditorGUILayout.EnumPopup("                         " + (iList + 1).ToString(), io_list[iList] as Enum, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            }
        }
        GUILayout.Space(5);
    }

    Vector2 DrawVector2(string i_name, Vector2 i_vector2)
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(i_name, miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
            GUILayout.Label("x:", miniLabelStyle, GUILayout.Width(30), GUILayout.Height(20));
            i_vector2.x = EditorGUILayout.FloatField("", i_vector2.x, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
            GUILayout.Label("      y:", miniLabelStyle, GUILayout.Width(60), GUILayout.Height(20));
            i_vector2.y = EditorGUILayout.FloatField("", i_vector2.y, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
        }
        GUILayout.EndHorizontal();
        return i_vector2;
    }

    Vector3 DrawVector3(string i_name, Vector3 i_vector3)
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(i_name, miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
            GUILayout.Label("x:", miniLabelStyle, GUILayout.Width(30), GUILayout.Height(20));
            i_vector3.x = EditorGUILayout.FloatField("", i_vector3.x, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
            GUILayout.Label("      y:", miniLabelStyle, GUILayout.Width(60), GUILayout.Height(20));
            i_vector3.y = EditorGUILayout.FloatField("", i_vector3.y, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
            GUILayout.Label("      z:", miniLabelStyle, GUILayout.Width(60), GUILayout.Height(20));
            i_vector3.z = EditorGUILayout.FloatField("", i_vector3.z, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
        }
        GUILayout.EndHorizontal();
        return i_vector3;
    }

    Vector4 DrawVector4(string i_name, Vector4 i_vector4)
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(i_name, miniLabelStyle, GUILayout.Width(150), GUILayout.Height(20));
            GUILayout.Label("x:", miniLabelStyle, GUILayout.Width(30), GUILayout.Height(20));
            i_vector4.x = EditorGUILayout.FloatField("", i_vector4.x, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
            GUILayout.Label("      y:", miniLabelStyle, GUILayout.Width(60), GUILayout.Height(20));
            i_vector4.y = EditorGUILayout.FloatField("", i_vector4.y, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
            GUILayout.Label("      z:", miniLabelStyle, GUILayout.Width(60), GUILayout.Height(20));
            i_vector4.z = EditorGUILayout.FloatField("", i_vector4.z, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
            GUILayout.Label("      w:", miniLabelStyle, GUILayout.Width(60), GUILayout.Height(20));
            i_vector4.w = EditorGUILayout.FloatField("", i_vector4.w, inputStyle, GUILayout.Width(100), GUILayout.Height(20));
        }
        GUILayout.EndHorizontal();
        return i_vector4;
    }

    void ShowLayer(int i_layerId)
    {
        treePathIds.Clear();
        nodeExpandState.Clear();
        if (i_layerId == -1)
        {
            currentNodes = null;
        }
        else
        {
            currentNodes = new List<DTNode>();
            currentNodes.Add(layers[i_layerId].treeNode);
            nodeExpandState.Add(false);
            //renameLayerName = layers[i_layerId].layerName;
        }
        UpdatePathNames(i_layerId);
    }

    void UpdateTreePath()
    {
        if (treePathIds.Count == 0)
        {
            ShowLayer(curLayerId);
        }
        else
        {
            DTNode tempNode = layers[curLayerId].treeNode;
            for (int i = 1; i < treePathIds.Count; i++)
            {
                tempNode = tempNode.subNodes_[treePathIds[i]];
            }
            currentNodes = tempNode.subNodes_;
            nodeExpandState.Clear();
            for (int i = 0; i < currentNodes.Count; i++)
            {
                nodeExpandState.Add(false);
            }
            UpdatePathNames(curLayerId);
        }
    }

    void UpdatePathNames(int i_layerId)
    {
        treePathNames.Clear();

        if (i_layerId == -1)
            return;

        treePathNames.Add(layers[i_layerId].layerName);
        DTNode tempNode = layers[i_layerId].treeNode;
        if (treePathIds.Count > 0)
            treePathNames.Add(tempNode.nodeName_);
        for (int i = 1; i < treePathIds.Count; i++)
        {
            tempNode = tempNode.subNodes_[treePathIds[i]];
            treePathNames.Add(tempNode.nodeName_);
        }
    }


    void InitializeLayersByMod()
    {
        if (curModId == -1)
            curModId = 0;

        layers.Clear();

        TextAsset textAsset = Resources.Load<TextAsset>("DecisionTree/DecisionTrees");
        if(textAsset == null)
        {
            string path = Application.dataPath + "/Resources/DecisionTree/DecisionTrees.xml";
            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            XmlDocument tempDoc = new XmlDocument();
            XmlElement basicTable = tempDoc.CreateElement("DecisionTreeTable");
            tempDoc.AppendChild(basicTable);

            tempDoc.Save(fileStream);
            fileStream.Close();
        }
       
        AssetDatabase.Refresh();
        textAsset = Resources.Load<TextAsset>("DecisionTree/DecisionTrees");

        //load all entity nodes
        if (textAsset != null && textAsset.text[0] == '<')
        {
            xmlDoc.LoadXml(textAsset.text);
            XmlNode xmlRootNode = xmlDoc.SelectSingleNode("DecisionTreeTable");
            if (xmlRootNode == null)
                return;

            for (int j = 0; j < xmlRootNode.ChildNodes.Count; j++)
            {
                XmlNode layerNode = xmlRootNode.ChildNodes[j];
                XmlAttribute nameAttr = layerNode.Attributes["layerName"];
                string layerName = nameAttr != null ? nameAttr.Value : "";

                XmlAttribute activeAttr = layerNode.Attributes["active"];
                bool active = activeAttr != null ? bool.Parse(activeAttr.Value) : false;

                XmlAttribute intervalAttr = layerNode.Attributes["UpdateInterval"];
                uint updateInterval = intervalAttr != null ? uint.Parse(intervalAttr.Value) : 1;

                layers.Add(new LayerInfo(layerName, DTNode.LoadSubTree(layerNode.FirstChild), updateInterval, active));
            }
        }
    }

    void SaveAllLayers()
    {
        XmlNode xmlRootNode = xmlDoc.SelectSingleNode("DecisionTreeTable");
        xmlRootNode.RemoveAll();

        for (int i = 0; i < layers.Count; i++)
        {
            XmlElement newLayer = xmlDoc.CreateElement("Layer");
            newLayer.SetAttribute("layerName", layers[i].layerName);
            newLayer.SetAttribute("active", layers[i].active.ToString());
            newLayer.SetAttribute("UpdateInterval", layers[i].updateInterval.ToString());
            newLayer.AppendChild(DTNode.SaveSubTree(layers[i].treeNode, xmlDoc));
            xmlRootNode.AppendChild(newLayer);
        }
    }

    void SaveToFile()
    {
        SaveAllLayers();

        TextAsset textAsset = Resources.Load<TextAsset>("DecisionTree/DecisionTrees");
        string path = Application.dataPath + "/Resources/DecisionTree/DecisionTrees.xml";

        string dateTime = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString();
        dateTime = dateTime.Replace('/', '_').Replace(':', '_').Replace(' ', '_');
        string backupPath = Application.dataPath + "/Resources/DecisionTree/Backup/DecisionTrees" + dateTime + ".bak";

        //remove read-only attribute
        FileAttributes attributes = File.GetAttributes(path);
        attributes &= (~FileAttributes.ReadOnly);
        File.SetAttributes(path, attributes);

        if (!File.Exists(backupPath))
            File.Copy(path, backupPath, false);

        FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        xmlDoc.Save(fileStream);
        fileStream.Close();

        EditorUtility.SetDirty(textAsset);
        AssetDatabase.Refresh();
    }
}