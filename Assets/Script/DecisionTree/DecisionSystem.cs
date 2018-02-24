using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using UnityEngine;
using System.Reflection;


public class DTNode
{
    public const string xmlNodeName_ = "DTNode";
    public const string xmlNodeNameAtrName_ = "Name";
    public string nodeName_;
    public List<BasicCondition> conditions_ = null;
    public List<DTNode> subNodes_ = null;
    public List<BasicResult> results_ = null;

    public DTNode(string i_name)
    {
        nodeName_ = i_name;
        conditions_ = new List<BasicCondition>();
        subNodes_ = new List<DTNode>();
        results_ = new List<BasicResult>();
    }

    // return root node of this sub tree
    public static DTNode LoadSubTree(XmlNode i_xmlNode)
    {
        XmlAttribute nameAttr = i_xmlNode.Attributes[DTNode.xmlNodeNameAtrName_];
        string nodeName = nameAttr != null ? nameAttr.Value : "";

        DTNode node = new DTNode(nodeName);

        for (int i = 0; i < i_xmlNode.ChildNodes.Count; ++i)
        {
            var curNode = i_xmlNode.ChildNodes[i];
            if (curNode.Name.Equals(DTNode.xmlNodeName_))
            {
                node.subNodes_.Add(LoadSubTree(curNode));
            }
            else if (curNode.Name.Equals(BasicCondition.xmlNodeName))
            {
                BasicCondition condition = BasicCondition.Deserialize(curNode as XmlElement);
                if (condition != null)
                {
                    node.conditions_.Add(condition);
                }
            }
            else if (curNode.Name.Equals(BasicResult.xmlNodeName))
            {
                BasicResult result = BasicResult.Deserialize(curNode as XmlElement);
                if (result != null)
                {
                    node.results_.Add(result);
                }
            }
        }
        return node;
    }

    public static XmlElement SaveSubTree(DTNode i_node, XmlDocument i_ownerDoc)
    {
        XmlElement elem = i_ownerDoc.CreateElement(DTNode.xmlNodeName_);
        elem.SetAttribute(DTNode.xmlNodeNameAtrName_, i_node.nodeName_);
        for (int i = 0; i < i_node.conditions_.Count; ++i)
        {
            XmlElement conditionElem = i_ownerDoc.CreateElement(BasicCondition.xmlNodeName);
            BasicCondition.Serialize(i_node.conditions_[i], ref conditionElem);
            elem.AppendChild(conditionElem);
        }
        for (int i = 0; i < i_node.results_.Count; ++i)
        {
            XmlElement resultElem = i_ownerDoc.CreateElement(BasicResult.xmlNodeName);
            BasicResult.Serialize(i_node.results_[i], ref resultElem);
            elem.AppendChild(resultElem);
        }
        for (int i = 0; i < i_node.subNodes_.Count; ++i)
        {
            elem.AppendChild(SaveSubTree(i_node.subNodes_[i], i_ownerDoc));
        }
        return elem;
    }

    public static void Init(DTNode i_node)
    {
        for (int i = 0; i < i_node.conditions_.Count; ++i)
        {
            i_node.conditions_[i].Initialize();
        }
        for (int i = 0; i < i_node.results_.Count; ++i)
        {
            i_node.results_[i].Initialize();
        }
        for (int i = 0; i < i_node.subNodes_.Count; ++i)
        {
            Init(i_node.subNodes_[i]);
        }
    }

    // not real deep copy, only copy DtVariables for editor
    public static DTNode DeepCopy(DTNode i_node)    
    {
        DTNode retNode = new DTNode(i_node.nodeName_);
        for (int i = 0; i < i_node.conditions_.Count; ++i)
        {
            Type conditionType = i_node.conditions_[i].GetType();
            BasicCondition tempCondition = Activator.CreateInstance(conditionType) as BasicCondition;

            FieldInfo[] conditionVariables = conditionType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int vi = 0; vi < conditionVariables.Length; ++vi)
            {
                FieldInfo var = conditionVariables[vi];
                if (Attribute.IsDefined(var, typeof(DtVariable)))
                {
                    if (var.FieldType == typeof(string) || var.FieldType.IsArray || !var.FieldType.IsClass)
                    {
                        var.SetValue(tempCondition, var.GetValue(i_node.conditions_[i]));
                    }
                    else
                    {
                        System.Object value = Activator.CreateInstance(var.FieldType, var.GetValue(i_node.conditions_[i]));
                        var.SetValue(tempCondition, value);
                    }
                }
            }
            retNode.conditions_.Add(tempCondition);
        }
        for (int i = 0; i < i_node.results_.Count; ++i)
        {
            Type resultType = i_node.results_[i].GetType();
            BasicResult tempResult = Activator.CreateInstance(resultType) as BasicResult;

            FieldInfo[] resultVariables = resultType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int vi = 0; vi < resultVariables.Length; ++vi)
            {
                FieldInfo var = resultVariables[vi];
                if (Attribute.IsDefined(var, typeof(DtVariable)))
                {
                    if (var.FieldType == typeof(string) || var.FieldType.IsArray || !var.FieldType.IsClass)
                    {
                        var.SetValue(tempResult, var.GetValue(i_node.results_[i]));
                    }
                    else
                    {
                        System.Object value = Activator.CreateInstance(var.FieldType, var.GetValue(i_node.results_[i]));
                        var.SetValue(tempResult, value);
                    }
                }
            }
            retNode.results_.Add(tempResult);
        }
        for (int i = 0; i < i_node.subNodes_.Count; ++i)
        {
            retNode.subNodes_.Add(DeepCopy(i_node.subNodes_[i]));
        }
        return retNode;
    }
}


public struct DecisionTree
{
    public DTNode rootNode_;
    public uint updateInterval_;
    public bool active_;

    public DecisionTree(DTNode i_rootNode, uint i_updateInterval, bool i_active)
    {
        rootNode_ = i_rootNode;
        updateInterval_ = i_updateInterval;
        active_ = i_active;
    }
}
 
public class Decisions
{
    Dictionary<Type, BasicResult> type2Decision_ = new Dictionary<Type, BasicResult>();

    // root nodes
    public List<DecisionTree> trees_ = new List<DecisionTree>();

    // for editor use
#if UNITY_EDITOR
    public List<string> nodeNameLists_ = new List<string>();
    public int curTreeIndex_;
    bool searchMarker_;
#endif

    public Decisions()
    {
        type2Decision_.Add(typeof(BGMResult), new BGMResult());
        type2Decision_.Add(typeof(SkyColorResult), new SkyColorResult());
        type2Decision_.Add(typeof(CameraEffectResult), new CameraEffectResult());
        type2Decision_.Add(typeof(CameraStateResult), new CameraStateResult());
    }

    private void OverrideFinalDecision(BasicResult another)
    {
        if (type2Decision_.ContainsKey(another.GetType()))
        {
            type2Decision_[another.GetType()].BeOverrided(another);
        }
    }

    public void Clear()
    {
        trees_.Clear();
    }

    public void Decide(uint frame)
    {
        // reverse the order to get the right priority
        for (int i = trees_.Count - 1; i >= 0; i--)
        {
            if (!trees_[i].active_ || frame % trees_[i].updateInterval_ != 0)
                return;

            bool ableToEnterRoot = true;
            for (int j = 0; j < trees_[i].rootNode_.conditions_.Count; ++j)
            {
                if (!trees_[i].rootNode_.conditions_[j].IsConditionMet())
                {
                    ableToEnterRoot = false;
                    break;
                }
            }
            if (ableToEnterRoot)
            {
#if UNITY_EDITOR
                if (curTreeIndex_ == i)
                {
                    searchMarker_ = true;
                    nodeNameLists_.Clear();
                }
                else
                    searchMarker_ = false;
#endif
                DecideTree(trees_[i].rootNode_);
            }
        }
    }

    public void ExecuteDecision(uint frame)
    {
        var v = type2Decision_.Values.GetEnumerator();
        while (v.MoveNext())
        {
            if (frame % v.Current.excuteInterval == 0)
                v.Current.Execute();
        }
    }

    // every level, only one child will be select to enter or to execute (from the first to the last)
    private void DecideTree(DTNode node)
    {
        DTNode tempNode = node;
        while (true)
        {
#if UNITY_EDITOR
            if (searchMarker_)
                nodeNameLists_.Add(tempNode.nodeName_);
#endif
            for (int i = 0; i < tempNode.results_.Count; ++i)
            {
                OverrideFinalDecision(tempNode.results_[i]);
            }
            bool enterNextLevel = false;
            for (int i = 0; i < tempNode.subNodes_.Count; ++i)
            {
                bool passAllConditions = true;
                for (int j = 0; j < tempNode.subNodes_[i].conditions_.Count; j++)
                {
                    BasicCondition tempCond = tempNode.subNodes_[i].conditions_[j];
                    if ((tempCond.IsConditionMet() && tempCond.nor) || (!tempCond.IsConditionMet() && !tempCond.nor))
                    {
                        passAllConditions = false;
                        break;
                    }
                }
                if (passAllConditions)
                {
                    tempNode = tempNode.subNodes_[i];
                    enterNextLevel = true;
                    break;
                }
            }
            if (!enterNextLevel)
            {
                return;
            }
        }
    }
}



public class DecisionSystem : MonoBehaviour
{
    [HideInInspector]
    public Decisions decisions_ = new Decisions();

    public bool shouldExecute = true;

    uint counter = 0;
    void Awake()
    {
        // clean rules in memory
        decisions_.Clear();

        // prepare xml files reader
        TextAsset xml = Resources.Load<TextAsset>("DecisionTree/DecisionTrees");
        MemoryStream stream = new MemoryStream(xml.bytes);
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.Load(stream);
                           
        // begin parse xml
        LoadDecisions(xmldoc);
        InitDecisions();
    }

    void Update()
    {
        ++counter;
        if (shouldExecute)
            UpdateDecisions(counter);
    }

    // can be used as interface to update all decision at once
    public void UpdateDecisions(uint frameCount = 3600)
    {
        // we assume 3600 is enough for all frame intervals
        decisions_.Decide(frameCount);
        decisions_.ExecuteDecision(frameCount);
    }
 
    void LoadDecisions(XmlDocument xmldoc)
    {
        XmlNode rootNode = xmldoc.SelectSingleNode("DecisionTreeTable");

        foreach (XmlNode layerNode in rootNode.ChildNodes)
        {
            XmlAttribute activeAttr = layerNode.Attributes["active"];
            bool tempActive = activeAttr != null ? bool.Parse(activeAttr.Value) : false;
            XmlAttribute updateIntervalAttr = layerNode.Attributes["UpdateInterval"];
            uint tempUpdateInterval = updateIntervalAttr != null ? uint.Parse(updateIntervalAttr.Value) : 1;

            for (int i = 0; i < layerNode.ChildNodes.Count; ++i)       // normally, a layer only has one childNode
            {
                decisions_.trees_.Add(new DecisionTree(DTNode.LoadSubTree(layerNode.ChildNodes[i]), tempUpdateInterval, tempActive));
            }
        }
    }

    void InitDecisions()
    {
        for (int i = 0; i < decisions_.trees_.Count; ++i)
        {
            DTNode.Init(decisions_.trees_[i].rootNode_);
        }
    }
}
