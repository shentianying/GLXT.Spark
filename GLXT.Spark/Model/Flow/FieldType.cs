using System.Collections.Generic;

namespace GLXT.Spark.Model.Flow
{
    /// <summary>
    /// 字段数据类型
    /// </summary>
    public class FieldType
    {
        /// <summary>
        /// 字段数据类型及可用的运算符
        /// </summary>
        /// <param name="type">字段数据类型</param>
        public FieldType(string type)
        {
            this.Type = type;
        }
        /// <summary>
        /// 字段数据类型及可用的运算符
        /// </summary>
        /// <param name="type">字段数据类型</param>
        /// <param name="dicType">字典类型</param>
        public FieldType(string type, string dicType)
        {
            this.Type = type;
            this.DicType = dicType;
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 字典类型（数据类型为dictionary时有效）
        /// </summary>
        public string DicType { get; set; }
        /// <summary>
        /// 可用的运算符
        /// </summary>
        public List<KeyValuePair<string, string>> Operator
        {
            get
            {
                switch (Type)
                {
                    case "int":
                    case "decimal":
                    case "datetime":
                        return new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("＞","＞"),
                            new KeyValuePair<string, string>("≥","≥"),
                            new KeyValuePair<string, string>("＜","＜"),
                            new KeyValuePair<string, string>("≤","≤"),
                            new KeyValuePair<string, string>("＝","＝"),
                            new KeyValuePair<string, string>("≠","≠")
                        };
                    case "string":
                        return new List<KeyValuePair< string, string>> ()
                        {
                            new KeyValuePair<string, string>("包含","包含"),
                            new KeyValuePair<string, string>("不包含","不包含"),
                            new KeyValuePair<string, string>("＝","＝"),
                            new KeyValuePair<string, string>("≠","≠")
                        };
                    case "bool":
                        return new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("＝","＝")
                        };
                    case "organization":
                    case "dictionary":
                        return new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("属于","属于"),
                            new KeyValuePair<string, string>("不属于","不属于")
                        };
                    default:
                        return new List<KeyValuePair<string, string>>();
                }
            }
        }
    }
}
