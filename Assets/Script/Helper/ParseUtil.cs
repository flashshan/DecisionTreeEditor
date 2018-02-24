using UnityEngine;
using System;
using System.Reflection;
using System.Xml;

public class ParseUtil
{
    // Parse enum
	public static T ParseEnum<T>(string i_str, bool i_ignoreCase = false) where T : struct, IComparable, IFormattable, IConvertible
	{
		T result;

		try
		{
			result = (T)Enum.Parse(typeof(T), i_str);
		}
		catch (Exception)
		{
			Debug.LogError("Error parsing \'" + i_str + "\' to enum type \'" + typeof(T).Name + "\'!");
			result = new T();
		}

		return result;
	}

	public static Enum ParseEnum(Type i_type, string i_str, bool i_ignoreCase = false)
	{
		Enum result;

		try
		{
			result = Enum.Parse(i_type, i_str) as Enum;
		}
		catch (Exception)
		{
			Debug.LogError("Error parsing \'" + i_str + "\' to enum type \'" + i_type.Name + "\'!");
			result = Activator.CreateInstance(i_type) as Enum;
		}

		return result;
	}

    public static string SerializeValue(object value)
    {
        Type _type = value.GetType();

        if (_type == typeof(string))
        {
            return (string)value;
        }
        else if (_type == typeof(bool))
        {
            return XmlConvert.ToString((bool)value);
        }
        else if (_type == typeof(byte))
        {
            return XmlConvert.ToString((byte)value);
        }
        else if (_type == typeof(Int16))
        {
            return XmlConvert.ToString((Int16)value);
        }
        else if (_type == typeof(Int32))
        {
            return XmlConvert.ToString((Int32)value);
        }
        else if (_type == typeof(Int64))
        {
            return XmlConvert.ToString((Int64)value);
        }
        else if (_type == typeof(UInt16))
        {
            return XmlConvert.ToString((UInt16)value);
        }
        else if (_type == typeof(UInt32))
        {
            return XmlConvert.ToString((UInt32)value);
        }
        else if (_type == typeof(UInt64))
        {
            return XmlConvert.ToString((UInt64)value);
        }
        else if (_type == typeof(Single))
        {
            return XmlConvert.ToString((Single)value);
        }
        else if (_type == typeof(Double))
        {
            return XmlConvert.ToString((Double)value);
        }
        else if (_type == typeof(Vector2))   // vector2
        {
            Vector2 v = (Vector2)value;
            return "(" + XmlConvert.ToString(v.x) + "," + XmlConvert.ToString(v.y) + ")";
        }
        else if (_type == typeof(Vector3))   // vector3
        {
            Vector3 v = (Vector3)value;
            return "(" + XmlConvert.ToString(v.x) + "," + XmlConvert.ToString(v.y) + "," + XmlConvert.ToString(v.z) + ")";
        }
        else if (_type == typeof(Vector4))   // vector4
        {
            Vector4 v = (Vector4)value;
            return "(" + XmlConvert.ToString(v.x) + "," + XmlConvert.ToString(v.y) + "," + XmlConvert.ToString(v.z) + "," + XmlConvert.ToString(v.w) + ")";
        }
        else if (_type == typeof(Color))   // vector3
        {
            Color v = (Color)value;
            return "(" + XmlConvert.ToString(v.r) + "," + XmlConvert.ToString(v.g) + "," + XmlConvert.ToString(v.b) + "," + XmlConvert.ToString(v.a) + ")";
        }
        else if (_type.IsEnum)
        {
            return Enum.GetName(_type, value);
        }
        else if (_type.IsArray)
        {
            Array inm = value as Array;
            string result = "";
            //var enm = inm.GetEnumerator();

            for (int e = 0; e < inm.Length; ++e)
            {
                result += SerializeValue(inm.GetValue(e));
                result += ';';
            }

            if (result.Length > 0)
            {
                result = result.Remove(result.LastIndexOf(';'));
            }
            result = "{" + result + "}";
            return result;
        }
        else
        {
            Debug.LogError("Undetected Type " + _type.Name);
            return value.ToString();
        }
    }

    public static object ParseValue(Type _type, string _valueStr)
    {
        if (_type == null || _valueStr == null) return null;

        if (_type == typeof(string))    // string
        {
            return _valueStr;
        }
        else if (_type == typeof(Int16))    // int16
        {
            return XmlConvert.ToInt16(_valueStr);
        }
        else if (_type == typeof(Int32))    // int32
        {
            return XmlConvert.ToInt32(_valueStr);
        }
        else if (_type == typeof(Int64))    // int64
        {
            return XmlConvert.ToInt64(_valueStr);
        }
        else if (_type == typeof(char))     // char
        {
            return XmlConvert.ToChar(_valueStr);
        }
        else if (_type == typeof(byte))     // byte
        {
            return XmlConvert.ToSByte(_valueStr);
        }
        else if (_type == typeof(Single))    // float   
        {
            return XmlConvert.ToSingle(_valueStr);
        }
        else if (_type == typeof(Double))   // double
        {
            return XmlConvert.ToDouble(_valueStr);
        }
        else if (_type == typeof(bool))     // bool 
        {
            return XmlConvert.ToBoolean(_valueStr);
        }
        else if (_type == typeof(UInt64))   // uint64
        {
            return XmlConvert.ToUInt64(_valueStr);
        }
        else if (_type == typeof(UInt32))   // uint32
        {
            return XmlConvert.ToUInt32(_valueStr);
        }
        else if (_type == typeof(UInt16))   // uint16
        {
            return XmlConvert.ToUInt16(_valueStr);
        }
        else if (_type == typeof(Color))   // color
        {
            int lqInd = _valueStr.IndexOf('(');
            if (lqInd <= -1) return null;
            _valueStr = _valueStr.Remove(lqInd, 1);

            int rqInd = _valueStr.LastIndexOf(')');
            if (rqInd <= -1) return null;
            _valueStr = _valueStr.Remove(rqInd, 1);

            string[] v = _valueStr.Split(',');
            if (v.Length != 4) return null;
            return new Color(XmlConvert.ToSingle(v[0]), XmlConvert.ToSingle(v[1]), XmlConvert.ToSingle(v[2]), XmlConvert.ToSingle(v[3]));
        }
        else if (_type == typeof(Vector2))   // vector2
        {
            int lqInd = _valueStr.IndexOf('(');
            if (lqInd <= -1) return null;
            _valueStr = _valueStr.Remove(lqInd, 1);

            int rqInd = _valueStr.LastIndexOf(')');
            if (rqInd <= -1) return null;
            _valueStr = _valueStr.Remove(rqInd, 1);

            string[] v = _valueStr.Split(',');
            if (v.Length != 2) return null;
            return new Vector2(XmlConvert.ToSingle(v[0]), XmlConvert.ToSingle(v[1]));
        }
        else if (_type == typeof(Vector3))   // vector3
        {
            int lqInd = _valueStr.IndexOf('(');
            if (lqInd <= -1) return null;
            _valueStr = _valueStr.Remove(lqInd, 1);

            int rqInd = _valueStr.LastIndexOf(')');
            if (rqInd <= -1) return null;
            _valueStr = _valueStr.Remove(rqInd, 1);

            string[] v = _valueStr.Split(',');
            if (v.Length != 3) return null;
            return new Vector3(XmlConvert.ToSingle(v[0]), XmlConvert.ToSingle(v[1]), XmlConvert.ToSingle(v[2]));
        }
        else if (_type == typeof(Vector4))   // vector4
        {
            int lqInd = _valueStr.IndexOf('(');
            if (lqInd <= -1) return null;
            _valueStr = _valueStr.Remove(lqInd, 1);

            int rqInd = _valueStr.LastIndexOf(')');
            if (rqInd <= -1) return null;
            _valueStr = _valueStr.Remove(rqInd, 1);

            string[] v = _valueStr.Split(',');
            if (v.Length != 4) return null;
            return new Vector4(XmlConvert.ToSingle(v[0]), XmlConvert.ToSingle(v[1]), XmlConvert.ToSingle(v[2]), XmlConvert.ToSingle(v[3]));
        }
        else if (_type.IsEnum)   // Enum Type
        {
            return Enum.Parse(_type, _valueStr);
        }
        else if (_type.IsArray)   // Array Type
        {
            int lqInd = _valueStr.IndexOf('{');
            if (lqInd <= -1) return null;
            _valueStr = _valueStr.Remove(lqInd, 1);

            int rqInd = _valueStr.LastIndexOf('}');
            if (rqInd <= -1) return null;
            _valueStr = _valueStr.Remove(rqInd, 1);

            string[] eStr = _valueStr.Split(';');

            Type elementType = _type.GetElementType();

            Array result = Array.CreateInstance(elementType, eStr.Length);

            for (int e = 0; e < eStr.Length; ++e)
            {
                result.SetValue(ParseValue(elementType, eStr[e]), e);
            }
            return result;
        }
        else
        {
            Debug.LogError("Undetected Type " + _type.Name);
            return null;
        }
    }

    public static T ParseValue<T>(string _valueStr)
    {
        return (T)ParseValue(typeof(T), _valueStr);
    }

    #region test
    void TestSerializeValue()
    {
        #region parse_test

        //string str_float = "1.23";
        //float a = ParseValue<float>(str_float);
        //Debug.Log(a);

        //string str_int = "123";
        //int b = ParseValue<int>(str_int);
        //Debug.Log(b);

        //string str_Double = "123.0";
        //double c = ParseValue<double>(str_Double);
        //Debug.Log(c);

        //string str_vec3 = "(1,2,3)";
        //Vector3 d = ParseValue<Vector3>(str_vec3);
        //Debug.Log(d);

        //string str_vec4 = "(1,2,3,4)";
        //Vector4 e = ParseValue<Vector4>(str_vec4);
        //Debug.Log(e);

        //string str_col = "(0.5,1,0.25,1)";
        //Color f = ParseValue<Color>(str_col);
        //Debug.Log(f);

        //string str_bol = "true";
        //bool g = ParseValue<bool>(str_bol);
        //Debug.Log(g);

        //str_bol = "false";
        //g = ParseValue<bool>(str_bol);
        //Debug.Log(g);

        //string str_arr_int = "{1,2,3,4}";
        //int[] h = ParseValue<int[]>(str_arr_int);

        //string str_arr_vec3 = "{(0.1,0.2,0.3),(0.4,0.5,0.6),(0.7,0.8,0.9)}";
        //Vector3[] i = ParseValue<Vector3[]>(str_arr_vec3);

        //string str_em = "bbb";
        //testE j = ParseValue<testE>(str_em);
        //Debug.Log(j);

        #endregion

        #region serialize_test
        //string result = "";
        //float val_f = 1.002f;
        //result = SerializeValue(val_f);

        //int val_i = 23;
        //result = SerializeValue(val_i);

        //Vector3 val_v3 = new Vector3(1,2,3);
        //result = SerializeValue(val_v3);

        //Vector2 val_v2 = new Vector2(1, 2);
        //result = SerializeValue(val_v2);

        //Color val_col = new Color(0.1f,0.2f,0.3f,0.4f);
        //result = SerializeValue(val_col);

        //string val_string = "Mary has a little lamb.";
        //result = SerializeValue(val_string);

        //testE val_em = testE.ccc;
        //result = SerializeValue(val_em);

        //float[] val_arr_f = { 1.1f, 2.2f, 3.3f };
        //result = SerializeValue(val_arr_f);

        //Vector3[] val_arr_vec3 = { new Vector3(1,2,3), new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
        //result = SerializeValue(val_arr_vec3);
        #endregion
    }
    #endregion
}
