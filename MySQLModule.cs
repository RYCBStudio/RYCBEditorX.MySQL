using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace RYCBEditorX.MySQL;
public class MySQLModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        MySQLConnectionUtils mySQLConnection = new();
        mySQLConnection.Init();
        var _ = mySQLConnection.Select("private_info");
        GlobalConfig.CurrentLogger.Log(_["username"], module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:Test");
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
        GlobalConfig.CurrentLogger.Log("MySQL模块初始化完成.", module: Utils.EnumLogModule.CUSTOM, customModuleName: "MySQL:初始化");
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }
}