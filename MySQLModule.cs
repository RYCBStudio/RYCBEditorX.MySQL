using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using RYCBEditorX.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RYCBEditorX.MySQL;
public class MySQLModule : IModule
{
    [MaybeNull]
    public static ISQLConnectionUtils ConnectionUtils { get; set; }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        MySQLConnectionUtils mySQLConnection = new();
        mySQLConnection.Init();
        if (mySQLConnection.MySqlConnection is null)
        {
            GlobalConfig.CurrentLogger.Error(new NullReferenceException("`MySqlConnection`为null。已停止连接。"), "", EnumLogType.WARN);
            return;
        }
        if (!mySQLConnection.ConnectionOpened)
        {
            GlobalConfig.CurrentLogger.Error(new SQLConnectionException("`MySqlConnection`连接失败。已停止连接。"), "", EnumLogType.WARN);
            return;
        }
        ConnectionUtils = mySQLConnection;
        var _ = mySQLConnection.Select("update_info");
        List<object> LatestVersion = [], PatchLevel = [], LatestRevision = [], LTSVersions = [], ComingToEOLVersions = [], EOLVersions = [], VulnerableVersions = [];
        foreach (var item in _)
        {
            LatestVersion.AddNotNullAndNoRepeat(item["latest_ver"]);
            LatestRevision.AddNotNullAndNoRepeat(item["latest_rev"]);
            PatchLevel.AddNotNullAndNoRepeat(item["patch_level"]);
            LTSVersions.AddNotNullAndNoRepeat(item["lts_ver"]);
            ComingToEOLVersions.AddNotNullAndNoRepeat(item["coming_to_eol_ver"]);
            EOLVersions.AddNotNullAndNoRepeat(item["eol_ver"]);
            VulnerableVersions.AddNotNullAndNoRepeat(item["vulnerable_ver"]);
        }
        if (!LatestVersion.Contains(GlobalConfig.Version) || (LatestVersion.Contains(GlobalConfig.Version) & !IsRevisionNumberNewest(GlobalConfig.Revision, LatestRevision[0].ToString())))
        {
            GlobalConfig.CurrentLogger.Log("RYCB Editor 有更新，最新版本: {0}-{1}_{4}  当前版本: {2}-{3}"
                .FormatEx(LatestVersion[0].ToString(), LatestRevision[0].ToString(), GlobalConfig.Version
                , GlobalConfig.Revision, PatchLevel[0].ToString()), EnumLogType.WARN);
            GlobalConfig.CurrentLogger.Log("RYCB Editor has been updated, the latest version: {0}-{1}_{4}  Current Version: {2}-{3}"
                .FormatEx(LatestVersion[0].ToString(), LatestRevision[0].ToString(), 
                GlobalConfig.Version, GlobalConfig.Revision, PatchLevel[0].ToString()), EnumLogType.WARN);
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
        GlobalConfig.CurrentLogger.Log("MySQL模块初始化完成.", module: EnumLogModule.SQL);
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {

    }

    public static bool IsRevisionNumberNewest(string revisionNumber, string latstRevisionNumber)
    {
        List<string> revisionNumbers = ["p", "o", "rc4", "rc3", "rc2", "rc1", "rc0", "b", "a"];
        return revisionNumbers.IndexOf(revisionNumber) <= revisionNumbers.IndexOf(latstRevisionNumber);
    }
}