using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GLXT.Spark.Model;

namespace GLXT.Spark.Utils
{
    public class Common
    {
        public Common() { }

        static string entityNamespace = "GLXT.Spark.Entity";
        private static char[] constant =
     {
        '0','1','2','3','4','5','6','7','8','9',
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
      };
        public static string GenerateRandomNumber(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }
        #region md5加密  
        /// <summary>
        /// 生成MD5加密字串
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>string</returns>
        public static string GetMD5(string str)
        {
            using var md5 = new MD5CryptoServiceProvider();
            var data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var result = BitConverter.ToString(data);
            return result.Replace("-", "");
        }

        /// <summary>
        /// 对密码进行加密
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>string</returns>
        public static string GetEncryptPassword(string str)
        {
            str += "★密★";
            return GetMD5(str);
        }
        #endregion

        #region DES 加密/解密


        private static string DESKey = "Z_N_Z_S_8888";
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Encrypt(string Text)
        {
            return Encrypt(Text, DESKey);
        }
        /// <summary> 
        /// 加密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Encrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);
            string value = GetMD5(sKey).Substring(0, 8);
            des.Key = Encoding.ASCII.GetBytes(value);
            des.IV = Encoding.ASCII.GetBytes(value);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Decrypt(string Text)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                return Decrypt(Text, DESKey);
            }
            else
            {
                return "";
            }
        }
        /// <summary> 
        /// 解密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Decrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len;
            len = Text.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            string value = GetMD5(sKey).Substring(0, 8);
            des.Key = Encoding.ASCII.GetBytes(value);
            des.IV = Encoding.ASCII.GetBytes(value);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }
        #endregion

        #region 上传文件
        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <param name="formFile">文件流</param>
        /// <param name="dirName">上传到的目录名称</param>
        /// <param name="dirPath">文件磁盘路径</param>
        /// <param name="newName">新名称</param>
        /// <returns></returns>
        public static string UpLoadSingleFile(IFormFile formFile, string dirName, string dirPath = "", string newName = "")
        {
            string result = "";
            Path.GetFileName("");
            string fileName = formFile.FileName; // 获取文件名
            string fileSuffix = Path.GetExtension(fileName);//  获取文件名后缀
            if (string.IsNullOrEmpty(newName))
            {
                newName = Guid.NewGuid() + "";// 设置新的name
            }
            string newFileName = newName + fileSuffix; // 新的文件名=名字+后缀

            dirPath = CreateDirectory(dirPath, dirName);
            string filePath = dirPath + $"\\{newFileName}";

            using (FileStream fs = File.Create(filePath))
            {
                // 复制文件
                formFile.CopyTo(fs);
                // 清空缓冲区数据
                fs.Flush();
            }
            result = $"\\{dirName}\\{newFileName}";
            return result;
        }
        public static string CreateDirectory(string dirPath, string dirName)
        {
            if (!string.IsNullOrEmpty(dirPath))
            {
                dirPath = dirPath + $"\\{dirName}";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
            return dirPath;
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        #endregion


        #region 数据类型转换
        /// <summary>
        /// 返回Double类型数据(若不能转换成double类型返回0)
        /// </summary>
        /// <param name="number">要转换的数据</param>
        /// <returns>double</returns>
        public static double GetDouble(object number)
        {
            if (!double.TryParse(number.ToString(), out double result))
                result = 0;
            return result;
        }

        /// <summary>
        /// 返回Int类型数据(若不能转换成Int类型返回0)
        /// </summary>
        /// <param name="number">要转换的数据</param>
        /// <returns>int</returns>
        public static int GetInt(object number)
        {
            if (number == null || !double.TryParse(number.ToString(), out double result))
                result = 0;
            return (int)result;
        }

        /// <summary>
        /// 返回两个数字相除后的百分比
        /// </summary>
        /// <param name="number1">分子数据</param>
        /// <param name="number2">分母数据</param>
        /// <returns></returns>
        public static string GetStrRate(object number1, object number2)
        {
            double dblNum1, dblNum2;
            dblNum1 = GetDouble(number1);
            dblNum2 = GetDouble(number2);

            if (dblNum1 == 0)
                return "0";
            if (dblNum2 == 0)
                return "∞";
            return (dblNum1 / dblNum2).ToString("P");
        }

        /// <summary>
        /// 返回DateTime类型数据（若不能转换，返回当前日期）
        /// </summary>
        /// <param name="date">要转换的数据</param>
        /// <returns>DateTime</returns>
        public static DateTime GetDate(object date)
        {
            if (date == null || !DateTime.TryParse(date.ToString(), out DateTime result)) result = DateTime.Today;
            return result;
        }

        public static string GetString(object str)
        {
            return str == null ? string.Empty : str.ToString();
        }
        #endregion

        #region 与对象相关函数
        /// <summary>
        /// 判断两个对象是否相等(包含null)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool IsEquals(object v1 = null, object v2 = null)
        {
            if (v1 == null)
            {
                if (v2 == null)
                    return true;
                else
                    return false;
            }
            else
            {
                if (v2 == null)
                    return false;
                else
                    return v1.Equals(v2);
            }
        }

        /// <summary>
        /// 获取对象的属性值（已知对象类型时效率高）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <typeparam name="TValue">属性类型</typeparam>
        /// <param name="billObj">对象数据</param>
        /// <param name="propName">属性名称</param>
        /// <returns>属性值</returns>
        public static TValue GetPropertyValue<T, TValue>(T billObj, string propName)
        {
            var prop = typeof(T).GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(billObj);
                if (value != null)
                    return (TValue)value;
            }
            return default;
        }

        /// <summary>
        /// 获取对象的属性值（未知对象类型）
        /// </summary>
        /// <typeparam name="TValue">属性类型</typeparam>
        /// <param name="billObj">对象数据</param>
        /// <param name="propName">属性名称</param>
        /// <returns>属性值</returns>
        public static TValue GetPropertyValue<TValue>(object billObj, string propName)
        {
            var prop = billObj.GetType().GetProperty(propName);
            if (prop != null)
                return (TValue)prop.GetValue(billObj);

            return default;
        }

        /// <summary>
        /// 设置对象的属性值（已知对象类型时效率高）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="billObj">对象数据</param>
        /// <param name="propName">属性名称</param>
        /// <param name="propValue">属性值</param>
        public static void SetPropertyValue<T>(T billObj, string propName,object propValue)
        {
            var prop = typeof(T).GetProperty(propName);

            if (prop != null)
                prop.SetValue(billObj, propValue);
        }

        /// <summary>
        /// 设置对象的属性值（未知对象类型）
        /// </summary>
        /// <param name="billObj">对象数据</param>
        /// <param name="propName">属性名称</param>
        /// <param name="propValue">属性值</param>
        public static void SetPropertyValue(object billObj, string propName, object propValue)
        {
            var prop = billObj.GetType().GetProperty(propName);
            if (prop != null)
                prop.SetValue(billObj, propValue);
        }
        /// <summary>
        /// 获取 注解 TableAttribute 中的 name(获取实体类中映射的数据库中的表名)
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <returns>表名</returns>
        public static string GetTableName<T>() where T:class
        {
            return typeof(T).GetAttributeValue((System.ComponentModel.DataAnnotations.Schema.TableAttribute ta) => ta.Name);
        }
        /// <summary>
        /// 根据类名获取表名
        /// </summary>
        /// <param name="className">命名空间+类名</param>
        /// <returns></returns>
        public static string GetTableName(string className)
        {
            var type = Type.GetType(className);
            if (type == null)
                return "";
            else
                return type.GetAttributeValue((System.ComponentModel.DataAnnotations.Schema.TableAttribute ta) => ta.Name);
        }
        public static object GetTableNameAndDisplayName(string className)
        {
            var ns = entityNamespace + "." + className;
            var type = Type.GetType(ns);
            if (type == null)
                return null;
            else
            {
                string tableEnName = type.GetAttributeValue((TableAttribute ta) => ta.Name);
                string tableCnName = type.GetAttributeValue((DisplayNameAttribute d) => d.DisplayName);

                List<ValueTuple<string, string, string>> list = new List<ValueTuple<string, string, string>>();
                foreach (var p in type.GetProperties())
                {
                    // 判断该字段是否有NotMapped的注解
                    var isNotMapped = p.CustomAttributes.Any(a => a.AttributeType.Name.Equals(typeof(NotMappedAttribute).Name));
                    if (!isNotMapped)
                    {
                        string displayName = p.GetAttributeValue((DisplayNameAttribute ta) => ta.DisplayName);
                        string propertyName = p.Name;
                        var propertyType = p.PropertyType;
                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            propertyType = propertyType.GetGenericArguments()[0];
                        }
                        string pt = propertyType.Name.ToLower();
                        if (pt.Equals("int32")) pt = "int";
                        if (pt.Equals("boolean")) pt = "bool";
                        list.Add((displayName, propertyName, pt));
                    }
                }
                return new { 
                    tableEnName = tableEnName, 
                    tableCnName = tableCnName, 
                    property = list 
                };
            }
        }
        #endregion

    }
}
