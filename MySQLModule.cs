using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using RYCBEditorX.Utils;
using System.Diagnostics.CodeAnalysis;
using RYCBEditorX.Crossings;
using Google.Protobuf.WellKnownTypes;
using RYCBEditorX.Crossing;

namespace RYCBEditorX.MySQL;
public class MySQLModule : IModule
{
    [MaybeNull]
    public static ISQLConnectionUtils ConnectionUtils
    {
        get; set;
    }

    public static MySql.Data.MySqlClient.MySqlConnection MySQLConnection
    {
        get; set;
    }

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
        MySQLConnection = mySQLConnection.MySqlConnection;
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
        UpdateInfoCrossing.HasNew = !LatestVersion.Contains(GlobalConfig.Version) || (LatestVersion.Contains(GlobalConfig.Version) & !IsRevisionNumberNewest(GlobalConfig.Revision, LatestRevision[0].ToString()));
        UpdateInfoCrossing.ComingToEOL = ComingToEOLVersions.Contains(GlobalConfig.Version);
        UpdateInfoCrossing.EOL = EOLVersions.Contains(GlobalConfig.Version);
        UpdateInfoCrossing.HasSV = VulnerableVersions.Contains(GlobalConfig.Version);
        UpdateInfoCrossing.NewVersion = "{0}-{1}_{2}".Format(LatestVersion[0].ToString(), LatestRevision[0].ToString(), PatchLevel[0].ToString());
        if (UpdateInfoCrossing.HasNew)
        {
            GlobalConfig.CurrentLogger.Log("RYCB Editor 有更新，最新版本: {0}  当前版本: {1}-{2}"
                .Format(UpdateInfoCrossing.NewVersion, GlobalConfig.Revision, PatchLevel[0].ToString()), EnumLogType.WARN);
            GlobalConfig.CurrentLogger.Log("RYCB Editor has been updated, the latest version: {0}  Current Version: {1}-{2}"
                .Format(UpdateInfoCrossing.NewVersion, GlobalConfig.Version, GlobalConfig.Revision, PatchLevel[0].ToString()), EnumLogType.WARN);
        }
        if (UpdateInfoCrossing.ComingToEOL)
        {
            GlobalConfig.CurrentLogger.Log("当前版本即将停止支持，请使用最新版本 " + LatestVersion[0], EnumLogType.WARN);
            GlobalConfig.CurrentLogger.Log("The current version is about to stop supporting, please use the latest version: " + LatestVersion[0], EnumLogType.WARN);
        }
        if (UpdateInfoCrossing.EOL)
        {
            GlobalConfig.CurrentLogger.Log("当前版本已停止支持，请使用最新版本: " + LatestVersion[0], EnumLogType.ERROR);
            GlobalConfig.CurrentLogger.Log("The current version is no longer supported, please use the latest version: " + LatestVersion[0], EnumLogType.ERROR);
        }
        if (UpdateInfoCrossing.HasSV)
        {
            GlobalConfig.CurrentLogger.Log("当前版本包含安全漏洞，请使用已修复的最新版本: " + LatestVersion[0], EnumLogType.FATAL);
            GlobalConfig.CurrentLogger.Log("The current version contains security vulnerabilities, please use the latest version that has been fixed: " + LatestVersion[0], EnumLogType.FATAL);
        }
        GlobalConfig.CurrentLogger.Log("MySQL模块初始化完成.", module: EnumLogModule.SQL);
        RefreshWikis();
    }


    private void RefreshWikis()
    {
        System.Threading.Tasks.Task.Run(() =>
        {
            while (!ConnectionUtils.ConnectionOpened) ;
            GlobalConfig.CurrentLogger.Log("刷新 Wiki 列表", type: EnumLogType.DEBUG, module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
            Dictionary<string, List<string>> OfflineWikis = [];
            var res = IWikiLoader.GetAllTargets();
            GlobalConfig.TotalLoadedOnline = res.Count;
            var index = 0;
            foreach (var item in res)
            {
                GlobalConfig.CurrentLogger.Log($"HIT: {item}", type: EnumLogType.DEBUG, module: EnumLogModule.CUSTOM, customModuleName: "初始化:配置");
                OfflineWikis[item] = IWikiLoader.GetWiki(item);
                index++;
                GlobalConfig.CurrentLoadedOnlineIndex = index;
            }
            GlobalConfig.OnlineWikis = OfflineWikis;
        });
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