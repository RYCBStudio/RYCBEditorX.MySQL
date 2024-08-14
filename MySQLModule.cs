using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using RYCBEditorX.Utils;

namespace RYCBEditorX.MySQL;
public class MySQLModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        MySQLConnectionUtils mySQLConnection = new();
        mySQLConnection.Init();
        if (mySQLConnection.MySqlConnection is null)
        {
            GlobalConfig.CurrentLogger.Error(new NullReferenceException("`MySqlConnection`为null。已停止连接。"), "", EnumLogType.WARN);
            return;
        }
        var _ = mySQLConnection.Select("private_info");
        GlobalConfig.CurrentLogger.Log(_[0]["username"], module: EnumLogModule.SQL);
        _ = mySQLConnection.Select("update_info");
        List<object> LatestVersion = [], PatchLevel = [], LatestRevision = [], LTSVersions = [], ComingToEOLVersions = [], EOLVersions = [], VulnerableVersions = [];
        foreach (var item in _)
        {
            LatestVersion.AddNotNullAndNoRepeat(item["latest_ver"]);
            PatchLevel.AddNotNullAndNoRepeat(item["patch_level"]);
            LTSVersions.AddNotNullAndNoRepeat(item["lts_ver"]);
            ComingToEOLVersions.AddNotNullAndNoRepeat(item["coming_to_eol_ver"]);
            EOLVersions.AddNotNullAndNoRepeat(item["eol_ver"]);
            VulnerableVersions.AddNotNullAndNoRepeat(item["vulnerable_ver"]);
        }
        if (!LatestVersion.Contains(GlobalConfig.Version) & IsRevisionNumberNewest(GlobalConfig.Revision, LatestRevision[0].ToString()))
        {
            GlobalConfig.CurrentLogger.Log("RYCB Editor 有更新，最新版本: " + LatestVersion[0], EnumLogType.WARN);
            GlobalConfig.CurrentLogger.Log("RYCB Editor has been updated, the latest version: " + LatestVersion[0], EnumLogType.WARN);
        }
        if (ComingToEOLVersions.Contains(GlobalConfig.Version))
        {
            GlobalConfig.CurrentLogger.Log("当前版本即将停止支持，请使用最新版本 " + LatestVersion[0], EnumLogType.WARN);
            GlobalConfig.CurrentLogger.Log("The current version is about to stop supporting, please use the latest version: " + LatestVersion[0], EnumLogType.WARN);
        }
        if (EOLVersions.Contains(GlobalConfig.Version))
        {
            GlobalConfig.CurrentLogger.Log("当前版本已停止支持，请使用最新版本: " + LatestVersion[0], EnumLogType.ERROR);
            GlobalConfig.CurrentLogger.Log("The current version is no longer supported, please use the latest version: " + LatestVersion[0], EnumLogType.ERROR);
        }
        if (VulnerableVersions.Contains(GlobalConfig.Version))
        {
            GlobalConfig.CurrentLogger.Log("当前版本包含安全漏洞，请使用已修复的最新版本: " + LatestVersion[0], EnumLogType.FATAL);
            GlobalConfig.CurrentLogger.Log("The current version contains security vulnerabilities, please use the latest version that has been fixed: " + LatestVersion[0], EnumLogType.FATAL);
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
        GlobalConfig.CurrentLogger.Log("MySQL模块初始化完成.", module: EnumLogModule.SQL);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }

    public static bool IsRevisionNumberNewest(string revisionNumber, string latstRevisionNumber)
    {
        List<string> revisionNumbers = ["p", "o", "rc4", "rc3", "rc2", "rc1", "rc0", "b", "a"];
        return revisionNumbers.IndexOf(revisionNumber) > revisionNumbers.IndexOf(latstRevisionNumber);
    }
}