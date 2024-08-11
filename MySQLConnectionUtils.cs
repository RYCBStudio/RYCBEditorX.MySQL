using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using MySql.Data.MySqlClient;
using static RYCBEditorX.GlobalConfig;

namespace RYCBEditorX.MySQL;

[System.Security.SecurityCritical]
public class MySQLConnectionUtils
{
    private string _connectionString;
    private MySqlCommand _command;

    public MySqlConnection MySqlConnection
    {
        get; set;
    }

    public MySQLConnectionUtils()
    {
        _connectionString = "B4w3CnmOhXzkLpgHzq5+oYo6rKKEnt/znwxs7kYaCyI9B3YXyq+gxU52T8fhgytJ1iDcH8Z2cMRHI9eEcMWrgG039Si7Xkvjgn1uBxZ5kVvDvplLUUV7TOHwPc+H+zaPfx1C94iJEeX8rRjlc2G4p8+bnL1TN8JMJvVz0V2GcHo=";
        _connectionString = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            Arguments = "-d " + _connectionString,
            FileName = StartupPath + "\\Tools\\crypto-helper.exe",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
        }).StandardOutput.ReadToEnd();
        MySqlConnection = new MySqlConnection(_connectionString);
        _command = new MySqlCommand
        {
            Connection = MySqlConnection
        };
    }

    public void Init()
    {
        try
        {
            MySqlConnection.Open();
            CurrentLogger.Log("MySQL数据库连接成功。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:初始化");
        }
        catch (Exception ex)
        {
            CurrentLogger.Log("MySQL数据库连接失败。", Utils.EnumLogType.WARN, module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:初始化");
            CurrentLogger.Error(ex, type: Utils.EnumLogType.WARN, module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:初始化");
        }
    }

    /// <summary>
    /// 选择数据库中一张表。
    /// </summary>
    /// <param name="table_name">表名</param>
    /// <param name="field_name">(可选) 字段名</param>
    /// <param name="condition">(可选) 查询条件</param>
    /// <param name="order_by">(可选) 排序规则</param>
    /// <param name="group_by">(可选) 分组字段</param>
    /// <param name="having">(可选) 分组后条件</param>
    /// <param name="limit">(可选) (MySQL方言) 分页参数，用逗号分隔</param>
    /// <returns></returns>
    public Dictionary<string,object> Select(string table_name, string field_name = "*",
        string condition = "", SQL_ORDER_BY_KEYWORDS order_by = SQL_ORDER_BY_KEYWORDS.ASC,
        string group_by = "", string having = "", string limit = "")
    {
        try
        {
            var SQL = $"SELECT {field_name} FROM {table_name}";
            if (field_name != "*")
            {
                SQL += $" ORDER BY {field_name} {order_by.OrderBy_Value()}";
            }
            if (!condition.IsNullOrEmpty())
            {
                SQL += $" WHERE {condition}";
            }
            if (!group_by.IsNullOrEmpty())
            {
                SQL += $" GROUP BY {group_by}";
            }
            if (!having.IsNullOrEmpty())
            {
                SQL += $" HAVING {having}";
            }
            if (!limit.IsNullOrEmpty())
            {
                SQL += $" LIMIT {limit}";
            }
            _command.CommandText = SQL;
            var reader = _command.ExecuteReader();
            if (reader.Read())
            {
                try
                {
                    Dictionary<string, object> row = [];
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var value = reader.GetValue(i);
                        row[columnName] = value;
                    }
                    return row;

                }
                catch (Exception ex)
                {
                    CurrentLogger.Error(ex);
                    return [];
                }
                finally
                {
                    reader.Close();
                    CurrentLogger.Log("当前SQL命令: " + SQL);
                }
            }
            else
            {
                return [];
            }
        }
        catch (Exception ex)
        {
            CurrentLogger.Error(ex);
            return [];
        }
        return [];
    }

    /// <summary>
    /// 向数据库中添加一个类型。目前只支持添加表。
    /// </summary>
    /// <param name="type">添加的类型(目前只支持<see cref="SQL_CREATION_TYPE.TABLE"/>)</param>
    /// <param name="table_name">添加的表的名称</param>
    /// <param name="fields">添加的字段</param>
    /// <param name="types">添加字段的类型</param>
    /// <param name="comment">添加字段的描述(若无只需传一个空列表 <c>[]</c>)</param>
    /// <param name="table_comment">(可选) 添加表的描述</param>
    /// <returns>是否成功</returns>
    /// <exception cref="ArgumentException"></exception>
    public bool Create(SQL_CREATION_TYPE type, string table_name,
        List<string> fields, List<string> types, List<string> comment, string table_comment = "")
    {
        if (comment.Count != 0 & (fields.Count != types.Count || fields.Count != comment.Count))
        {
            CurrentLogger.Error(new ArgumentException("fields、types 和 comment 的数量不匹配"));
            return false;
        }

        var SQL = $"CREATE {type.CreationType_Value()} {table_name} (\n";
        for (var i = 0; i < fields.Count; i++)
        {
            SQL += $"{fields[i]} {types[i]}";
            if (comment.Count > 0 && !string.IsNullOrEmpty(comment[i]))
            {
                SQL += $" COMMENT '{comment[i]}'";
            }
            SQL += " \n";
        }
        SQL += ")";
        if (!string.IsNullOrEmpty(table_comment))
        {
            SQL += $" COMMENT '{table_comment}'";
        }
        SQL += ";";

        return ExecuteNonQuery(SQL);
    }

    /// <summary>
    /// 向数据库中插入若干行数据。
    /// </summary>
    /// <param name="table_name">需插入数据的表名</param>
    /// <param name="fields_list">需插入的字段列表，如果为空，则插入所有字段</param>
    /// <param name="values_list">需插入的字段对应的值，可以是单行值或多行值的集合</param>
    /// <returns>是否成功</returns>
    /// <exception cref="ArgumentException">当字段列表和对应的值列表不匹配时抛出</exception>
    /// <exception cref="InvalidOperationException">当操作数据库时出现异常时抛出</exception>
    public bool Insert(string table_name, string fields_list, string values_list)
    {
        if (string.IsNullOrWhiteSpace(table_name) || string.IsNullOrWhiteSpace(values_list))
        {
            CurrentLogger.Error(new ArgumentException("表名和数据值不能为空"));
            return false;
        }
        var SQL = "";

        try
        {

            if (string.IsNullOrWhiteSpace(fields_list))
            {
                // 插入所有字段
                SQL = $"INSERT INTO {table_name} VALUES ({values_list})";
            }
            else
            {
                // 插入指定字段
                SQL = $"INSERT INTO {table_name} ({fields_list}) VALUES ({values_list})";
            }
            _command.CommandText = SQL;
            var rowsAffected = _command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            // 记录异常信息
            CurrentLogger.Log($"数据库操作失败: {ex.Message}");
            CurrentLogger.Error(ex);
            return false;
        }
        finally
        {
            CurrentLogger.Log("当前SQL命令: " + SQL);
        }
    }

    /// <summary>
    /// 更新指定表中的数据。
    /// </summary>
    /// <param name="table_name">指定的表名</param>
    /// <param name="fields">需更新的字段</param>
    /// <param name="vals">需更新的字段值</param>
    /// <param name="condition">(可选)更新的条件</param>
    /// <returns>是否成功</returns>
    /// <exception cref="ArgumentException"></exception>
    public bool Update(string table_name, List<string> fields, List<string> vals, string condition = "")
    {
        if (fields.Count != vals.Count)
        {
            CurrentLogger.Error(new ArgumentException("fields和types 的数量不匹配"));
        }
        var SQL = $"UPDATE {table_name}\nSET ";
        for (var i = 0; i < fields.Count - 1; i++)
        {
            SQL += $"{fields[i]}={vals[i]}, \n";
        }
        SQL += $"{fields[^1]}={vals[^1]} \n";
        if (!condition.IsNullOrEmpty())
        {
            SQL += "WHERE " + condition;
        }
        return ExecuteNonQuery(SQL);
    }

    /// <summary>
    /// 从表中删除行。
    /// </summary>
    /// <param name="table_name">需删除数据的表</param>
    /// <param name="condition">(可选) 删除条件</param>
    /// <returns>是否成功</returns>
    /// <exception cref="FormatException"></exception>
    public bool Delete(string table_name, string condition = "")
    {
        var SQL = $"DELETE FROM {table_name} ";
        if (condition.IsNullOrEmpty())
        {
            CurrentLogger.Log("当前的SQL语句会影响所有行。", Utils.EnumLogType.WARN);
            CurrentLogger.Error(new FormatException("当前的SQL语句会影响所有行。"));
        }
        else
        {
            SQL += $"WHERE {condition}";
        }
        return ExecuteNonQuery(SQL);
    }

    private bool ExecuteNonQuery(string CommandText)
    {
        _command.CommandText = CommandText;
        try
        {
            var affected_rows = _command.ExecuteNonQuery();
            return affected_rows > 0;
        }
        catch (Exception ex)
        {
            CurrentLogger.Error(ex);
            return false;
        }
        finally
        {
            CurrentLogger.Log("当前SQL命令: " + CommandText);
        }
    }
}

public enum SQL_ORDER_BY_KEYWORDS
{
    /// <summary>
    /// 升序(默认)
    /// </summary>
    ASC,
    /// <summary>
    /// 降序
    /// </summary>
    DESC
}

public enum SQL_CREATION_TYPE
{
    DATABASE,
    TABLE,
    SCHEMA = TABLE
}

public static class SQLExtensions
{
    public static string OrderBy_Value(this SQL_ORDER_BY_KEYWORDS value)
    {
        return value switch
        {
            SQL_ORDER_BY_KEYWORDS.ASC => "ASC",
            SQL_ORDER_BY_KEYWORDS.DESC => "DESC",
            _ => "ASC",
        };
    }

    public static string CreationType_Value(this SQL_CREATION_TYPE value)
    {
        return value switch
        {
            SQL_CREATION_TYPE.DATABASE => "DATABASE",
            SQL_CREATION_TYPE.TABLE => "TABLE",
            _ => "TABLE"
        };
    }
}