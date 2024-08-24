using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using RYCBEditorX.Utils;

namespace RYCBEditorX.MySQL
{
    [SQLOperations]
    public interface ISQLConnectionUtils
    {

        /// <summary>
        /// 数据库连接对象<see cref="MySql.Data.MySqlClient.MySqlConnection"/>
        /// </summary>
        public MySqlConnection MySqlConnection
        {
            get; set;
        }

        /// <summary>
        /// 数据库连接是否打开
        /// </summary>
        public bool ConnectionOpened
        {
            get; set;
        }
        
        /// <summary>
        /// 初始化数据库连接。
        /// </summary>
        void Init();

        /// <summary>
        /// 选择数据库中一张表。
        /// </summary>
        /// <param name="table_name">表名</param>
        /// <param name="field_name">(可选) 字段名</param>
        /// <param name="condition">(可选) 查询条件</param>
        /// <param name="order_by">(可选) 排序规则</param>
        /// <param name="order_by_field">(可选) 排序字段</param>
        /// <param name="group_by">(可选) 分组字段</param>
        /// <param name="having">(可选) 分组后条件</param>
        /// <param name="limit">(可选) 分页参数，用逗号分隔</param>
        /// <returns></returns>
        [SQLPrivileges.Select]
        List<Dictionary<string, object>> Select(string table_name, string field_name = "*",
        string condition = "", SQL_ORDER_BY_KEYWORDS order_by = SQL_ORDER_BY_KEYWORDS.ASC,
        string order_by_field = "", string group_by = "", string having = "", string limit = "");

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
        [SQLPrivileges.Create]
        bool Create(SQL_CREATION_TYPE type, string table_name,
        List<string> fields, List<string> types, List<string> comment, string table_comment = "");

        /// <summary>
        /// 向数据库中插入若干行数据。
        /// </summary>
        /// <param name="table_name">需插入数据的表名</param>
        /// <param name="fields_list">需插入的字段列表，如果为空，则插入所有字段</param>
        /// <param name="values_list">需插入的字段对应的值，可以是单行值或多行值的集合</param>
        /// <returns>是否成功</returns>
        /// <exception cref="ArgumentException">当字段列表和对应的值列表不匹配时抛出</exception>
        /// <exception cref="InvalidOperationException">当操作数据库时出现异常时抛出</exception>
        [SQLPrivileges.Insert]
        [SQLCritical]
        bool Insert(string table_name, string fields_list, string values_list);

        /// <summary>
        /// 更新指定表中的数据。
        /// </summary>
        /// <param name="table_name">指定的表名</param>
        /// <param name="fields">需更新的字段</param>
        /// <param name="vals">需更新的字段值</param>
        /// <param name="condition">(可选)更新的条件</param>
        /// <returns>是否成功</returns>
        /// <exception cref="ArgumentException"></exception>
        [SQLPrivileges.Update]
        bool Update(string table_name, List<string> fields, List<string> vals, string condition = "");

        /// <summary>
        /// 从表中删除行。
        /// </summary>
        /// <param name="table_name">需删除数据的表</param>
        /// <param name="condition">(可选) 删除条件</param>
        /// <returns>是否成功</returns>
        /// <exception cref="FormatException"></exception>
        [SQLPrivileges.Delete]
        [SQLCritical]
        bool Delete(string table_name, string condition = "");
    }
}
