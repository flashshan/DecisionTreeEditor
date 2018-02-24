using System;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine;
using System.Reflection;

public abstract class BasicResult
{
    public const string xmlNodeName = "Result";
    protected static string NilValue = "None";

    public uint excuteInterval;

    public BasicResult()
    {
        excuteInterval = 1;
    }  // for editor only

    public virtual void BeOverrided(BasicResult another)
    {
        if (this.GetType() != another.GetType())
            throw new UnityException("cannot override different type of result");
    }

    public virtual void Initialize()
    {
    }

    public virtual void Execute()
    {
    }

    public static void Serialize(BasicResult _condition, ref XmlElement result)
    {
        if (result == null || _condition == null) return;
        if (result.Name != BasicResult.xmlNodeName) return;

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

    public static BasicResult Deserialize(XmlElement _node)
    {
        if (_node == null)
            return null;
        if (_node.Name != BasicResult.xmlNodeName)
            return null;

        string typeStr = _node.Attributes["Type"].Value;
        if (typeStr == null)
            return null;

        Type cType = Type.GetType(typeStr);
        if (cType == null)
            return null;

        // start parse:
        BasicResult result = (BasicResult)Activator.CreateInstance(cType);
        FieldInfo[] variables = cType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for (int vi = 0; vi < variables.Length; ++vi)
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
