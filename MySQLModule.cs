using Prism.Ioc;
using Prism.Modularity;
using System;

namespace RYCBEditorX.MySQL;
public class MySQLModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        MySQLConnectionUtils mySQLConnection = new();
        mySQLConnection.Init();
        if (mySQLConnection.MySqlConnection is null)
        {
            GlobalConfig.CurrentLogger.Error(new NullReferenceException("`MySqlConnection为null。已停止连接。"), "", Utils.EnumLogType.WARN);
            return;
        }
        var _ = mySQLConnection.Select("private_info");
        GlobalConfig.CurrentLogger.Log(_[0]["username"], module: Utils.EnumLogModule.SQL);
        _ = mySQLConnection.Select("update_info");
        foreach (var item in _)
        {
            foreach (var item2 in item)
            {
                GlobalConfig.CurrentLogger.Log($"{item2.Key}, {item2.Value}", module: Utils.EnumLogModule.SQL);
            }
        }
        //if (mySQLConnection.Create(SQL_CREATION_TYPE.TABLE,"user_data" ,["cookies"], ["varchar(50)"], []))
        //{
        //    GlobalConfig.CurrentLogger.Log("新建表user_data成功。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        //else
        //{
        //    GlobalConfig.CurrentLogger.Log("新建表user_data失败。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        //if (mySQLConnection.Insert("user_data", "cookies", "'yes'"))
        //{
        //    GlobalConfig.CurrentLogger.Log("向表user_data添加数据成功。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        //else
        //{
        //    GlobalConfig.CurrentLogger.Log("向表user_data添加数据失败。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        //if (mySQLConnection.Update("user_data", ["cookies"], ["'no'"]))
        //{
        //    GlobalConfig.CurrentLogger.Log("向表user_data更新数据成功。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        //else
        //{
        //    GlobalConfig.CurrentLogger.Log("向表user_data更新数据失败。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        //if (mySQLConnection.Delete("user_data", "cookies IS NULL"))
        //{
        //    GlobalConfig.CurrentLogger.Log("向表user_data删除NULL数据成功。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        //else
        //{
        //    GlobalConfig.CurrentLogger.Log("向表user_data删除NULL数据失败。", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
        //}
        GlobalConfig.CurrentLogger.Log("MySQL模块初始化完成.", module: Utils.EnumLogModule.SQL);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }
}