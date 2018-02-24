using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Reflection;

public class DtVariable: Attribute
{
    public string xmlAtrName{ get; protected set; }
    public string atrNameStr { get; protected set; }
    public DtVariable(string _xmlAtrName, string _atrName)
    {
        xmlAtrName = _xmlAtrName;
        atrNameStr = _atrName;
    }
}

[Serializable]
public abstract class BasicCondition // rules to decide if you can enter the node in a tree
{
    public const string xmlNodeName = "Condition";

    public BasicCondition() { }      // for editor only

    [DtVariable("Nor", "reverse Condition")]
    public bool nor = false;

    virtual public void Initialize()
    {
    }

    virtual public bool IsConditionMet()
    {
        return true;
    }

    public static void Serialize(BasicCondition _condition, ref XmlElement result)
    {
        if (result == null || _condition == null) return;
        if (result.Name != BasicCondition.xmlNodeName) return;

        Type cType = _condition.GetType();
        result.SetAttribute("Type", cType.Name);
        FieldInfo[] variables = cType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for (int vi = 0; vi < variables.Length; ++vi)
        {
            FieldInfo var = variables[vi];
            if (!Attribute.IsDefined(var, typeof(DtVariable)))
                continue;

            object value = var.GetValue(_condition);
            string valueStr = ParseUtil.SerializeValue(value);
            DtVariable varAtr = (DtVariable)Attribute.GetCustomAttribute(var, typeof(DtVariable));
            result.SetAttribute(varAtr.xmlAtrName, valueStr);
        }
    }

    private static string qName = null;
    protected static string GetQualifiedTypeName(string _typeName)
    {
        if(qName == null)
        {
            qName = typeof(BasicCondition).AssemblyQualifiedName;
        }

        string tName = typeof(BasicCondition).Name;
        int idx = qName.IndexOf(tName);
        string result = qName.Remove(idx, tName.Length);
        result = result.Insert(idx, _typeName);
        return result;
    }

    public static BasicCondition Deserialize(XmlElement _node)
    {
        if (_node == null)
            return null;
        if (_node.Name != BasicCondition.xmlNodeName)
            return null;

        string typeStr = _node.Attributes["Type"].Value;
        if (typeStr == null)
            return null;

        // should use qualified assemble name for reflection if type is included in namespace:
        Type cType = Type.GetType(GetQualifiedTypeName(typeStr));
        if (cType == null)
            return null;

        // start parse:
        BasicCondition result = (BasicCondition)Activator.CreateInstance(cType);
        FieldInfo[] variables = cType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for(int vi = 0; vi < variables.Length; ++vi)
        {
            FieldInfo var = variables[vi];
            if (!Attribute.IsDefined(var, typeof(DtVariable)))
                continue;

            DtVariable varAtr = (DtVariable)Attribute.GetCustomAttribute(var, typeof(DtVariable));

            XmlAttribute tempAttr = _node.Attributes[varAtr.xmlAtrName];
            if (tempAttr == null)
                continue;
   
            string valueStr = tempAttr.Value;
            if (valueStr == null)
            {
                Debug.LogError("Can't find condition value " + varAtr.xmlAtrName + "in condition node.");
                continue;
            }

            Type _fType = var.FieldType;
            object value = ParseUtil.ParseValue(_fType, valueStr);
            if (value == null)
            {
                Debug.LogError("Can't parse condition value " + varAtr.xmlAtrName + "in condition node.");
                continue;
            }

            var.SetValue(result, value);
        }

        return result;
    }
}

