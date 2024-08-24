using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.TeamFoundation.Common;
using MySql.Data.MySqlClient;
using RYCBEditorX.Utils;
using static IronPython.SQLite.PythonSQLite;
using static RYCBEditorX.GlobalConfig;

namespace RYCBEditorX.MySQL;

[System.Security.SecurityCritical]
public class MySQLConnectionUtils : ISQLConnectionUtils
{
    private readonly MySqlCommand _command;

    /// <inheritdoc/>
    public MySqlConnection MySqlConnection
    {
        get; set;
    }

    /// <inheritdoc/>
    public bool ConnectionOpened
    {
        get; set;
    }

    public MySQLConnectionUtils()
    {
        var connectionString =
            "df4NtPRw/QiuAehTF7Buu5Cqky7tI0+mdoTqD58vNPE/Q7H7mYo8w6tKkE05xqDPXw7HSuXcVnKUIJnTLW1Ss/iHYsfG2a0qHgmTlC+LBsFi7EkUY1tsxLJTCEF1i726AyUZNqYZ5oNQMupasy5nxeKBDFiEsxxMiQNl4mPmVn0=";
        connectionString = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            Arguments = "-d " + connectionString,
            FileName = StartupPath + @"\Tools\crypto-helper.exe",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
        })?.StandardOutput.ReadToEnd();
        MySqlConnection = new MySqlConnection(connectionString);
        _command = new MySqlCommand
        {
            Connection = MySqlConnection
        };
    }

    /// <inheritdoc/>
    public void Init()
    {
        CurrentLogger.Log("正在尝试连接MySQL数据库...", module: EnumLogModule.SQL);
        try
        {
            MySqlConnection.Open();
            CurrentLogger.Log("MySQL数据库连接成功。", module: EnumLogModule.SQL);
            ConnectionOpened = true;
        }
        catch (MySqlException ex)
        {
            CurrentLogger.Error(ex, module: EnumLogModule.SQL);
            CurrentLogger.Log("MySQL数据库断开连接。正在尝试重连...");
            MySqlConnection.Close();
            TryReconnent();
        }
        catch (Exception ex)
        {
            CurrentLogger.Log("MySQL数据库连接失败。", EnumLogType.WARN, module: EnumLogModule.SQL);
            CurrentLogger.Error(ex, type: EnumLogType.WARN, module: EnumLogModule.SQL);
            ConnectionOpened = false;
        }
    }

    private void TryReconnent()
    {
        try
        {
            MySqlConnection.Open();
            CurrentLogger.Log("MySQL数据库重连成功。", module: EnumLogModule.SQL);
            ConnectionOpened = true;
        }
        catch (Exception ex2)
        {
            CurrentLogger.Error(ex2, module: EnumLogModule.SQL);
            CurrentLogger.Log("MySQL数据库重连失败。", module: EnumLogModule.SQL);
            ConnectionOpened = false;
        }
    }
    
    /// <inheritdoc/>
    public List<Dictionary<string, object>> Select(string table_name, string field_name = "*",
        string condition = "", SQL_ORDER_BY_KEYWORDS order_by = SQL_ORDER_BY_KEYWORDS.ASC, string order_by_field = "",
        string group_by = "", string having = "", string limit = "")
    {
        if (!ConnectionOpened)
        {
            return [];
        }
        List<Dictionary<string, object>> results = [];
        var SQL = $"SELECT {field_name} FROM {table_name}";
        if (field_name != "*")
        {
            SQL += $" ORDER BY {field_name} {order_by.OrderBy_Value()}";
        }
        else if (!order_by_field.IsNullOrEmpty())
        {
            SQL += $" ORDER BY {order_by_field} {order_by.OrderBy_Value()}";
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
        MySqlDataReader reader = null;
        try
        {
            reader = _command.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> row = [];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.GetValue(i);
                    row[columnName] = value;
                }
                results.Add(row);
            }
        }
        catch (MySqlException ex)
        {
            CurrentLogger.Error(ex, module: EnumLogModule.SQL);
            CurrentLogger.Log("MySQL数据库断开连接。正在尝试重连...");
            MySqlConnection.Close();
            TryReconnent();
        }
        catch (Exception ex)
        {
            CurrentLogger.Error(ex, module: EnumLogModule.SQL);
        }
        finally
        {
            reader?.Close();
            CurrentLogger.Log("当前SQL命令: " + SQL, module: EnumLogModule.SQL);
        }
        return results;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
        catch (MySqlException ex)
        {
            CurrentLogger.Error(ex, module: EnumLogModule.SQL);
            CurrentLogger.Log("MySQL数据库断开连接。正在尝试重连...");
            MySqlConnection.Close();
            TryReconnent();
            return false;
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool Delete(string table_name, string condition = "")
    {
        var SQL = $"DELETE FROM {table_name} ";
        if (condition.IsNullOrEmpty())
        {
            CurrentLogger.Log("当前的SQL语句会影响所有行。", EnumLogType.WARN);
            CurrentLogger.Error(new FormatException("当前的SQL语句会影响所有行。"));
        }
        else
        {
            SQL += $"WHERE {condition}";
        }
        return ExecuteNonQuery(SQL);
    }

    [SQLPrivileges.Execute]
    private bool ExecuteNonQuery(string CommandText)
    {
        _command.CommandText = CommandText;
        try
        {
            var affected_rows = _command.ExecuteNonQuery();
            return affected_rows > 0;
        }

        catch (MySqlException ex)
        {
            CurrentLogger.Error(ex, module: EnumLogModule.SQL);
            CurrentLogger.Log("MySQL数据库断开连接。正在尝试重连...");
            MySqlConnection.Close();
            TryReconnent();
            return false;
        }
        catch (Exception ex)
        {
            CurrentLogger.Error(ex, module: EnumLogModule.SQL);
            return false;
        }
        finally
        {
            CurrentLogger.Log("当前SQL命令: " + CommandText, module: EnumLogModule.SQL);
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
    /// <summary>
    /// 数据库
    /// </summary>
    DATABASE,
    /// <summary>
    /// 表
    /// </summary>
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