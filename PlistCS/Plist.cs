

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace PlistCS
{
  public static class Plist
  {
    private static List<int> offsetTable = new List<int>();
    private static List<byte> objectTable = new List<byte>();
    private static int refCount;
    private static int objRefSize;
    private static int offsetByteSize;
    private static long offsetTableOffset;

    public static object readPlist(string path)
    {
      using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        return Plist.readPlist((Stream) fileStream, plistType.Auto);
    }

    public static object readPlistSource(string source)
    {
      return Plist.readPlist(Encoding.UTF8.GetBytes(source));
    }

    public static object readPlist(byte[] data)
    {
      return Plist.readPlist((Stream) new MemoryStream(data), plistType.Auto);
    }

    public static plistType getPlistType(Stream stream)
    {
      byte[] buffer = new byte[8];
      stream.Read(buffer, 0, 8);
      return BitConverter.ToInt64(buffer, 0) == 3472403351741427810L ? plistType.Binary : plistType.Xml;
    }

    public static object readPlist(Stream stream, plistType type)
    {
      if (type == plistType.Auto)
      {
        type = Plist.getPlistType(stream);
        stream.Seek(0L, SeekOrigin.Begin);
      }
      if (type == plistType.Binary)
      {
        using (BinaryReader binaryReader = new BinaryReader(stream))
          return Plist.readBinary(binaryReader.ReadBytes((int) binaryReader.BaseStream.Length));
      }
      else
      {
        XmlDocument xml = new XmlDocument();
        xml.XmlResolver = (XmlResolver) null;
        Stream inStream = stream;
        xml.Load(inStream);
        return Plist.readXml(xml);
      }
    }

    public static void writeXml(object value, string path)
    {
      using (StreamWriter streamWriter = new StreamWriter(path))
        streamWriter.Write(Plist.writeXml(value));
    }

    public static void writeXml(object value, Stream stream)
    {
      using (StreamWriter streamWriter = new StreamWriter(stream))
        streamWriter.Write(Plist.writeXml(value));
    }

    public static string writeXml(object value)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        using (XmlWriter writer = XmlWriter.Create((Stream) memoryStream, new XmlWriterSettings()
        {
          Encoding = (Encoding) new UTF8Encoding(false),
          ConformanceLevel = ConformanceLevel.Document,
          Indent = true
        }))
        {
          writer.WriteStartDocument();
          writer.WriteDocType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", (string) null);
          writer.WriteStartElement("plist");
          writer.WriteAttributeString("version", "1.0");
          Plist.compose(value, writer);
          writer.WriteEndElement();
          writer.WriteEndDocument();
          writer.Flush();
          writer.Close();
          return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
      }
    }

    public static void writeBinary(object value, string path)
    {
      using (BinaryWriter binaryWriter = new BinaryWriter((Stream) new FileStream(path, FileMode.Create)))
        binaryWriter.Write(Plist.writeBinary(value));
    }

    public static void writeBinary(object value, Stream stream)
    {
      using (BinaryWriter binaryWriter = new BinaryWriter(stream))
        binaryWriter.Write(Plist.writeBinary(value));
    }

    public static byte[] writeBinary(object value)
    {
      Plist.offsetTable.Clear();
      Plist.objectTable.Clear();
      Plist.refCount = 0;
      Plist.objRefSize = 0;
      Plist.offsetByteSize = 0;
      Plist.offsetTableOffset = 0L;
      int num = Plist.countObject(value) - 1;
      Plist.refCount = num;
      Plist.objRefSize = Plist.RegulateNullBytes(BitConverter.GetBytes(Plist.refCount)).Length;
      Plist.composeBinary(value);
      Plist.writeBinaryString("bplist00", false);
      Plist.offsetTableOffset = (long) Plist.objectTable.Count;
      Plist.offsetTable.Add(Plist.objectTable.Count - 8);
      Plist.offsetByteSize = Plist.RegulateNullBytes(BitConverter.GetBytes(Plist.offsetTable[Plist.offsetTable.Count - 1])).Length;
      List<byte> byteList = new List<byte>();
      Plist.offsetTable.Reverse();
      for (int index = 0; index < Plist.offsetTable.Count; ++index)
      {
        Plist.offsetTable[index] = Plist.objectTable.Count - Plist.offsetTable[index];
        byte[] numArray = Plist.RegulateNullBytes(BitConverter.GetBytes(Plist.offsetTable[index]), Plist.offsetByteSize);
        Array.Reverse((Array) numArray);
        byteList.AddRange((IEnumerable<byte>) numArray);
      }
      Plist.objectTable.AddRange((IEnumerable<byte>) byteList);
      Plist.objectTable.AddRange((IEnumerable<byte>) new byte[6]);
      Plist.objectTable.Add(Convert.ToByte(Plist.offsetByteSize));
      Plist.objectTable.Add(Convert.ToByte(Plist.objRefSize));
      byte[] bytes1 = BitConverter.GetBytes((long) num + 1L);
      Array.Reverse((Array) bytes1);
      Plist.objectTable.AddRange((IEnumerable<byte>) bytes1);
      Plist.objectTable.AddRange((IEnumerable<byte>) BitConverter.GetBytes(0L));
      byte[] bytes2 = BitConverter.GetBytes(Plist.offsetTableOffset);
      Array.Reverse((Array) bytes2);
      Plist.objectTable.AddRange((IEnumerable<byte>) bytes2);
      return Plist.objectTable.ToArray();
    }

    private static object readXml(XmlDocument xml)
    {
      return (object) (Dictionary<string, object>) Plist.parse(xml.DocumentElement.ChildNodes[0]);
    }

    private static object readBinary(byte[] data)
    {
      Plist.offsetTable.Clear();
      List<byte> byteList1 = new List<byte>();
      Plist.objectTable.Clear();
      Plist.refCount = 0;
      Plist.objRefSize = 0;
      Plist.offsetByteSize = 0;
      Plist.offsetTableOffset = 0L;
      List<byte> byteList2 = new List<byte>((IEnumerable<byte>) data);
      Plist.parseTrailer(byteList2.GetRange(byteList2.Count - 32, 32));
      Plist.objectTable = byteList2.GetRange(0, (int) Plist.offsetTableOffset);
      Plist.parseOffsetTable(byteList2.GetRange((int) Plist.offsetTableOffset, byteList2.Count - (int) Plist.offsetTableOffset - 32));
      return Plist.parseBinary(0);
    }

    private static Dictionary<string, object> parseDictionary(XmlNode node)
    {
      XmlNodeList childNodes = node.ChildNodes;
      if (childNodes.Count % 2 != 0)
        throw new DataMisalignedException("Dictionary elements must have an even number of child nodes");
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      int index = 0;
      while (index < childNodes.Count)
      {
        XmlNode xmlNode = childNodes[index];
        XmlNode node1 = childNodes[index + 1];
        if (xmlNode.Name != "key")
          throw new ApplicationException("expected a key node");
        object obj = Plist.parse(node1);
        if (obj != null)
          dictionary.Add(xmlNode.InnerText, obj);
        index += 2;
      }
      return dictionary;
    }

    private static List<object> parseArray(XmlNode node)
    {
      List<object> objectList = new List<object>();
      foreach (XmlNode childNode in node.ChildNodes)
      {
        object obj = Plist.parse(childNode);
        if (obj != null)
          objectList.Add(obj);
      }
      return objectList;
    }

    private static void composeArray(List<object> value, XmlWriter writer)
    {
      writer.WriteStartElement("array");
      foreach (object obj in value)
        Plist.compose(obj, writer);
      writer.WriteEndElement();
    }

    private static object parse(XmlNode node)
    {
      string name = node.Name;
      // ISSUE: reference to a compiler-generated method
      uint stringHash = (uint.Parse(name));
      if (stringHash <= 1996966820U)
      {
        if (stringHash <= 398550328U)
        {
          if ((int) stringHash != 184981848)
          {
            if ((int) stringHash == 398550328 && name == "string")
              return (object) node.InnerText;
          }
          else if (name == "false")
            return (object) false;
        }
        else if ((int) stringHash != 1278716217)
        {
          if ((int) stringHash != 1303515621)
          {
            if ((int) stringHash == 1996966820 && name == "null")
              return (object) null;
          }
          else if (name == "true")
            return (object) true;
        }
        else if (name == "dict")
          return (object) Plist.parseDictionary(node);
      }
      else if (stringHash <= 3218261061U)
      {
        if ((int) stringHash != -1973899994)
        {
          if ((int) stringHash == -1076706235 && name == "integer")
            return (object) Convert.ToInt32(node.InnerText, (IFormatProvider) NumberFormatInfo.InvariantInfo);
        }
        else if (name == "array")
          return (object) Plist.parseArray(node);
      }
      else if ((int) stringHash != -730669991)
      {
        if ((int) stringHash != -689983395)
        {
          if ((int) stringHash == -663559515 && name == "data")
            return (object) Convert.FromBase64String(node.InnerText);
        }
        else if (name == "real")
          return (object) Convert.ToDouble(node.InnerText, (IFormatProvider) NumberFormatInfo.InvariantInfo);
      }
      else if (name == "date")
        return (object) XmlConvert.ToDateTime(node.InnerText, XmlDateTimeSerializationMode.Utc);
      throw new ApplicationException(string.Format("Plist Node `{0}' is not supported", (object) node.Name));
    }

    private static void compose(object value, XmlWriter writer)
    {
      if (value == null || value is string)
        writer.WriteElementString("string", value as string);
      else if (value is int || value is long)
        writer.WriteElementString("integer", ((int) value).ToString((IFormatProvider) NumberFormatInfo.InvariantInfo));
      else if (value is Dictionary<string, object> || value.GetType().ToString().StartsWith("System.Collections.Generic.Dictionary`2[System.String"))
      {
        Dictionary<string, object> dictionary1 = value as Dictionary<string, object>;
        if (dictionary1 == null)
        {
          dictionary1 = new Dictionary<string, object>();
          IDictionary dictionary2 = (IDictionary) value;
          foreach (object key in (IEnumerable) dictionary2.Keys)
            dictionary1.Add(key.ToString(), dictionary2[key]);
        }
        Plist.writeDictionaryValues(dictionary1, writer);
      }
      else if (value is List<object>)
        Plist.composeArray((List<object>) value, writer);
      else if (value is byte[])
        writer.WriteElementString("data", Convert.ToBase64String((byte[]) value));
      else if (value is float || value is double)
        writer.WriteElementString("real", ((double) value).ToString((IFormatProvider) NumberFormatInfo.InvariantInfo));
      else if (value is DateTime)
      {
        string str = XmlConvert.ToString((DateTime) value, XmlDateTimeSerializationMode.Utc);
        writer.WriteElementString("date", str);
      }
      else
      {
        if (!(value is bool))
          throw new Exception(string.Format("Value type '{0}' is unhandled", (object) value.GetType().ToString()));
        writer.WriteElementString(value.ToString().ToLower(), "");
      }
    }

    private static void writeDictionaryValues(Dictionary<string, object> dictionary, XmlWriter writer)
    {
      writer.WriteStartElement("dict");
      foreach (string key in dictionary.Keys)
      {
        object obj = dictionary[key];
        writer.WriteElementString("key", key);
        XmlWriter writer1 = writer;
        Plist.compose(obj, writer1);
      }
      writer.WriteEndElement();
    }

    private static int countObject(object value)
    {
      int num1 = 0;
      string str = value.GetType().ToString();
      int num2;
      if (!(str == "System.Collections.Generic.Dictionary`2[System.String,System.Object]"))
      {
        if (str == "System.Collections.Generic.List`1[System.Object]")
        {
          foreach (object obj in (List<object>) value)
            num1 += Plist.countObject(obj);
          num2 = num1 + 1;
        }
        else
          num2 = num1 + 1;
      }
      else
      {
        Dictionary<string, object> dictionary = (Dictionary<string, object>) value;
        foreach (string key in dictionary.Keys)
          num1 += Plist.countObject(dictionary[key]);
        num2 = num1 + dictionary.Keys.Count + 1;
      }
      return num2;
    }

    private static byte[] writeBinaryDictionary(Dictionary<string, object> dictionary)
    {
      List<byte> byteList1 = new List<byte>();
      List<byte> byteList2 = new List<byte>();
      List<int> intList = new List<int>();
      for (int index = dictionary.Count - 1; index >= 0; --index)
      {
        object[] array = new object[dictionary.Count];
        dictionary.Values.CopyTo(array, 0);
        Plist.composeBinary(array[index]);
        Plist.offsetTable.Add(Plist.objectTable.Count);
        intList.Add(Plist.refCount);
        --Plist.refCount;
      }
      for (int index = dictionary.Count - 1; index >= 0; --index)
      {
        string[] array = new string[dictionary.Count];
        dictionary.Keys.CopyTo(array, 0);
        Plist.composeBinary((object) array[index]);
        Plist.offsetTable.Add(Plist.objectTable.Count);
        intList.Add(Plist.refCount);
        --Plist.refCount;
      }
      if (dictionary.Count < 15)
      {
        byteList2.Add(Convert.ToByte(208 | (int) Convert.ToByte(dictionary.Count)));
      }
      else
      {
        byteList2.Add((byte) 223);
        byteList2.AddRange((IEnumerable<byte>) Plist.writeBinaryInteger(dictionary.Count, false));
      }
      foreach (int num in intList)
      {
        byte[] numArray = Plist.RegulateNullBytes(BitConverter.GetBytes(num), Plist.objRefSize);
        Array.Reverse((Array) numArray);
        byteList1.InsertRange(0, (IEnumerable<byte>) numArray);
      }
      byteList1.InsertRange(0, (IEnumerable<byte>) byteList2);
      Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList1);
      return byteList1.ToArray();
    }

    private static byte[] composeBinaryArray(List<object> objects)
    {
      List<byte> byteList1 = new List<byte>();
      List<byte> byteList2 = new List<byte>();
      List<int> intList = new List<int>();
      for (int index = objects.Count - 1; index >= 0; --index)
      {
        Plist.composeBinary(objects[index]);
        Plist.offsetTable.Add(Plist.objectTable.Count);
        intList.Add(Plist.refCount);
        --Plist.refCount;
      }
      if (objects.Count < 15)
      {
        byteList2.Add(Convert.ToByte(160 | (int) Convert.ToByte(objects.Count)));
      }
      else
      {
        byteList2.Add((byte) 175);
        byteList2.AddRange((IEnumerable<byte>) Plist.writeBinaryInteger(objects.Count, false));
      }
      foreach (int num in intList)
      {
        byte[] numArray = Plist.RegulateNullBytes(BitConverter.GetBytes(num), Plist.objRefSize);
        Array.Reverse((Array) numArray);
        byteList1.InsertRange(0, (IEnumerable<byte>) numArray);
      }
      byteList1.InsertRange(0, (IEnumerable<byte>) byteList2);
      Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList1);
      return byteList1.ToArray();
    }

    private static byte[] composeBinary(object obj)
    {
      string s = obj.GetType().ToString();
      // ISSUE: reference to a compiler-generated method
      uint stringHash = (uint.Parse(s));
      if (stringHash <= 1541528931U)
      {
        if (stringHash <= 848225627U)
        {
          if ((int) stringHash != 347085918)
          {
            if ((int) stringHash == 848225627 && s == "System.Double")
              return Plist.writeBinaryDouble((double) obj);
          }
          else if (s == "System.Boolean")
            return Plist.writeBinaryBool((bool) obj);
        }
        else if ((int) stringHash != 1461188995)
        {
          if ((int) stringHash == 1541528931 && s == "System.DateTime")
            return Plist.writeBinaryDate((DateTime) obj);
        }
        else if (s == "System.Collections.Generic.List`1[System.Object]")
          return Plist.composeBinaryArray((List<object>) obj);
      }
      else if (stringHash <= 4180476474U)
      {
        if ((int) stringHash != -1520294252)
        {
          if ((int) stringHash == -114490822 && s == "System.Int32")
            return Plist.writeBinaryInteger((int) obj, true);
        }
        else if (s == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
          return Plist.writeBinaryDictionary((Dictionary<string, object>) obj);
      }
      else if ((int) stringHash != -93602905)
      {
        if ((int) stringHash == -38277272 && s == "System.Byte[]")
          return Plist.writeBinaryByteArray((byte[]) obj);
      }
      else if (s == "System.String")
        return Plist.writeBinaryString((string) obj, true);
      return new byte[0];
    }

    public static byte[] writeBinaryDate(DateTime obj)
    {
      List<byte> byteList = new List<byte>((IEnumerable<byte>) Plist.RegulateNullBytes(BitConverter.GetBytes(PlistDateConverter.ConvertToAppleTimeStamp(obj)), 8));
      byteList.Reverse();
      byteList.Insert(0, (byte) 51);
      Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList);
      return byteList.ToArray();
    }

    public static byte[] writeBinaryBool(bool obj)
    {
      List<byte> byteList = new List<byte>((IEnumerable<byte>) new byte[1]
      {
        (byte) (obj ? 9 : 8)
      });
      Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList);
      return byteList.ToArray();
    }

    private static byte[] writeBinaryInteger(int value, bool write)
    {
      List<byte> byteList = new List<byte>((IEnumerable<byte>) Plist.RegulateNullBytes(new List<byte>((IEnumerable<byte>) BitConverter.GetBytes((long) value)).ToArray()));
      while ((double) byteList.Count != Math.Pow(2.0, Math.Log((double) byteList.Count) / Math.Log(2.0)))
        byteList.Add((byte) 0);
      int num = 16 | (int) (Math.Log((double) byteList.Count) / Math.Log(2.0));
      byteList.Reverse();
      byteList.Insert(0, Convert.ToByte(num));
      if (write)
        Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList);
      return byteList.ToArray();
    }

    private static byte[] writeBinaryDouble(double value)
    {
      List<byte> byteList = new List<byte>((IEnumerable<byte>) Plist.RegulateNullBytes(BitConverter.GetBytes(value), 4));
      while ((double) byteList.Count != Math.Pow(2.0, Math.Log((double) byteList.Count) / Math.Log(2.0)))
        byteList.Add((byte) 0);
      int num = 32 | (int) (Math.Log((double) byteList.Count) / Math.Log(2.0));
      byteList.Reverse();
      byteList.Insert(0, Convert.ToByte(num));
      Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList);
      return byteList.ToArray();
    }

    private static byte[] writeBinaryByteArray(byte[] value)
    {
      List<byte> byteList1 = new List<byte>((IEnumerable<byte>) value);
      List<byte> byteList2 = new List<byte>();
      if (value.Length < 15)
      {
        byteList2.Add(Convert.ToByte(64 | (int) Convert.ToByte(value.Length)));
      }
      else
      {
        byteList2.Add((byte) 79);
        byteList2.AddRange((IEnumerable<byte>) Plist.writeBinaryInteger(byteList1.Count, false));
      }
      byteList1.InsertRange(0, (IEnumerable<byte>) byteList2);
      Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList1);
      return byteList1.ToArray();
    }

    private static byte[] writeBinaryString(string value, bool head)
    {
      List<byte> byteList1 = new List<byte>();
      List<byte> byteList2 = new List<byte>();
      foreach (char ch in value.ToCharArray())
        byteList1.Add(Convert.ToByte(ch));
      if (head)
      {
        if (value.Length < 15)
        {
          byteList2.Add(Convert.ToByte(80 | (int) Convert.ToByte(value.Length)));
        }
        else
        {
          byteList2.Add((byte) 95);
          byteList2.AddRange((IEnumerable<byte>) Plist.writeBinaryInteger(byteList1.Count, false));
        }
      }
      byteList1.InsertRange(0, (IEnumerable<byte>) byteList2);
      Plist.objectTable.InsertRange(0, (IEnumerable<byte>) byteList1);
      return byteList1.ToArray();
    }

    private static byte[] RegulateNullBytes(byte[] value)
    {
      return Plist.RegulateNullBytes(value, 1);
    }

    private static byte[] RegulateNullBytes(byte[] value, int minBytes)
    {
      Array.Reverse((Array) value);
      List<byte> byteList = new List<byte>((IEnumerable<byte>) value);
      for (int index = 0; index < byteList.Count && ((int) byteList[index] == 0 && byteList.Count > minBytes); index = index - 1 + 1)
        byteList.Remove(byteList[index]);
      if (byteList.Count < minBytes)
      {
        int num = minBytes - byteList.Count;
        for (int index = 0; index < num; ++index)
          byteList.Insert(0, (byte) 0);
      }
      value = byteList.ToArray();
      Array.Reverse((Array) value);
      return value;
    }

    private static void parseTrailer(List<byte> trailer)
    {
      Plist.offsetByteSize = BitConverter.ToInt32(Plist.RegulateNullBytes(trailer.GetRange(6, 1).ToArray(), 4), 0);
      Plist.objRefSize = BitConverter.ToInt32(Plist.RegulateNullBytes(trailer.GetRange(7, 1).ToArray(), 4), 0);
      byte[] array1 = trailer.GetRange(12, 4).ToArray();
      Array.Reverse((Array) array1);
      int startIndex1 = 0;
      Plist.refCount = BitConverter.ToInt32(array1, startIndex1);
      byte[] array2 = trailer.GetRange(24, 8).ToArray();
      Array.Reverse((Array) array2);
      int startIndex2 = 0;
      Plist.offsetTableOffset = BitConverter.ToInt64(array2, startIndex2);
    }

    private static void parseOffsetTable(List<byte> offsetTableBytes)
    {
      int index = 0;
      while (index < offsetTableBytes.Count)
      {
        byte[] array = offsetTableBytes.GetRange(index, Plist.offsetByteSize).ToArray();
        Array.Reverse((Array) array);
        Plist.offsetTable.Add(BitConverter.ToInt32(Plist.RegulateNullBytes(array, 4), 0));
        index += Plist.offsetByteSize;
      }
    }

    private static object parseBinaryDictionary(int objRef)
    {
      Dictionary<string, object> dictionary = new Dictionary<string, object>();
      List<int> intList = new List<int>();
      int num1 = (int) Plist.objectTable[Plist.offsetTable[objRef]];
      int newBytePosition;
      int count = Plist.getCount(Plist.offsetTable[objRef], out newBytePosition);
      int num2 = count >= 15 ? Plist.offsetTable[objRef] + 2 + Plist.RegulateNullBytes(BitConverter.GetBytes(count), 1).Length : Plist.offsetTable[objRef] + 1;
      int index1 = num2;
      while (index1 < num2 + count * 2 * Plist.objRefSize)
      {
        byte[] array = Plist.objectTable.GetRange(index1, Plist.objRefSize).ToArray();
        Array.Reverse((Array) array);
        intList.Add(BitConverter.ToInt32(Plist.RegulateNullBytes(array, 4), 0));
        index1 += Plist.objRefSize;
      }
      for (int index2 = 0; index2 < count; ++index2)
        dictionary.Add((string) Plist.parseBinary(intList[index2]), Plist.parseBinary(intList[index2 + count]));
      return (object) dictionary;
    }

    private static object parseBinaryArray(int objRef)
    {
      List<object> objectList = new List<object>();
      List<int> intList = new List<int>();
      int num1 = (int) Plist.objectTable[Plist.offsetTable[objRef]];
      int newBytePosition;
      int count = Plist.getCount(Plist.offsetTable[objRef], out newBytePosition);
      int num2 = count >= 15 ? Plist.offsetTable[objRef] + 2 + Plist.RegulateNullBytes(BitConverter.GetBytes(count), 1).Length : Plist.offsetTable[objRef] + 1;
      int index1 = num2;
      while (index1 < num2 + count * Plist.objRefSize)
      {
        byte[] array = Plist.objectTable.GetRange(index1, Plist.objRefSize).ToArray();
        Array.Reverse((Array) array);
        intList.Add(BitConverter.ToInt32(Plist.RegulateNullBytes(array, 4), 0));
        index1 += Plist.objRefSize;
      }
      for (int index2 = 0; index2 < count; ++index2)
        objectList.Add(Plist.parseBinary(intList[index2]));
      return (object) objectList;
    }

    private static int getCount(int bytePosition, out int newBytePosition)
    {
      byte num1 = Convert.ToByte((int) Plist.objectTable[bytePosition] & 15);
      int num2;
      if ((int) num1 < 15)
      {
        num2 = (int) num1;
        newBytePosition = bytePosition + 1;
      }
      else
        num2 = (int) Plist.parseBinaryInt(bytePosition + 1, out newBytePosition);
      return num2;
    }

    private static object parseBinary(int objRef)
    {
      switch ((int) Plist.objectTable[Plist.offsetTable[objRef]] & 240)
      {
        case 96:
          return Plist.parseBinaryUnicodeString(Plist.offsetTable[objRef]);
        case 160:
          return Plist.parseBinaryArray(objRef);
        case 208:
          return Plist.parseBinaryDictionary(objRef);
        case 64:
          return Plist.parseBinaryByteArray(Plist.offsetTable[objRef]);
        case 80:
          return Plist.parseBinaryAsciiString(Plist.offsetTable[objRef]);
        case 32:
          return Plist.parseBinaryReal(Plist.offsetTable[objRef]);
        case 48:
          return Plist.parseBinaryDate(Plist.offsetTable[objRef]);
        case 0:
          if ((int) Plist.objectTable[Plist.offsetTable[objRef]] != 0)
            return (object) ((int) Plist.objectTable[Plist.offsetTable[objRef]] == 9);
          return (object) null;
        case 16:
          return Plist.parseBinaryInt(Plist.offsetTable[objRef]);
        default:
          throw new Exception("This type is not supported");
      }
    }

    public static object parseBinaryDate(int headerPosition)
    {
      byte[] array = Plist.objectTable.GetRange(headerPosition + 1, 8).ToArray();
      Array.Reverse((Array) array);
      int startIndex = 0;
      return (object) PlistDateConverter.ConvertFromAppleTimeStamp(BitConverter.ToDouble(array, startIndex));
    }

    private static object parseBinaryInt(int headerPosition)
    {
      int newHeaderPosition;
      return Plist.parseBinaryInt(headerPosition, out newHeaderPosition);
    }

    private static object parseBinaryInt(int headerPosition, out int newHeaderPosition)
    {
      int count = (int) Math.Pow(2.0, (double) ((int) Plist.objectTable[headerPosition] & 15));
      byte[] array = Plist.objectTable.GetRange(headerPosition + 1, count).ToArray();
      Array.Reverse((Array) array);
      newHeaderPosition = headerPosition + count + 1;
      int minBytes = 4;
      return (object) BitConverter.ToInt32(Plist.RegulateNullBytes(array, minBytes), 0);
    }

    private static object parseBinaryReal(int headerPosition)
    {
      int count = (int) Math.Pow(2.0, (double) ((int) Plist.objectTable[headerPosition] & 15));
      byte[] array = Plist.objectTable.GetRange(headerPosition + 1, count).ToArray();
      Array.Reverse((Array) array);
      int minBytes = 8;
      return (object) BitConverter.ToDouble(Plist.RegulateNullBytes(array, minBytes), 0);
    }

    private static object parseBinaryAsciiString(int headerPosition)
    {
      int newBytePosition;
      int count = Plist.getCount(headerPosition, out newBytePosition);
      List<byte> range = Plist.objectTable.GetRange(newBytePosition, count);
      if (range.Count <= 0)
        return (object) string.Empty;
      return (object) Encoding.ASCII.GetString(range.ToArray());
    }

    private static object parseBinaryUnicodeString(int headerPosition)
    {
      int newBytePosition;
      int length = Plist.getCount(headerPosition, out newBytePosition) * 2;
      byte[] bytes = new byte[length];
      int index = 0;
      while (index < length)
      {
        byte num1 = Plist.objectTable.GetRange(newBytePosition + index, 1)[0];
        byte num2 = Plist.objectTable.GetRange(newBytePosition + index + 1, 1)[0];
        if (BitConverter.IsLittleEndian)
        {
          bytes[index] = num2;
          bytes[index + 1] = num1;
        }
        else
        {
          bytes[index] = num1;
          bytes[index + 1] = num2;
        }
        index += 2;
      }
      return (object) Encoding.Unicode.GetString(bytes);
    }

    private static object parseBinaryByteArray(int headerPosition)
    {
      int newBytePosition;
      int count = Plist.getCount(headerPosition, out newBytePosition);
      return (object) Plist.objectTable.GetRange(newBytePosition, count).ToArray();
    }
  }
}
