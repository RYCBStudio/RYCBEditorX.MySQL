﻿using System;
using System.Collections.Generic;
using RYCBEditorX.Utils;

namespace RYCBEditorX.MySQL;

public class CommentLoader(ISQLConnectionUtils sqlUtils, string condition = "")
{
    public List<Comment> LoadCommentsAsync()
    {
        var comments = new List<Comment>();

        // 查询数据库中的评论数据
        var rows = sqlUtils.Select("comments", condition: condition);

        foreach (var row in rows)
        {
            var comment = new Comment
            {
                User = row["usr"].ToString(),
                Uid = row["uid"].ToString(),
                CommentText = row["commentText"].ToString(),
                Time = row["time"] != DBNull.Value ? Convert.ToDateTime(row["time"]).ToString("yyyy-MM-dd HH:mm:ss") : "Unknown",
                Likes = row["likes"] != DBNull.Value ? Convert.ToInt32(row["likes"]) : 0
            };

            comments.Add(comment);
        }

        return comments;
    }

    public bool UpdateLikes(string uid, int newLikes)
    {
        var fields = new List<string> { "likes" };
        var values = new List<string> { newLikes.ToString() };
        var condition = $"uid = '{uid}'";

        var success = sqlUtils.Update("comments", fields, values, condition);

        if (success)
        {
            GlobalConfig.CurrentLogger.Log($"成功将UID为'{uid}'的评论点赞数更新为{newLikes}。", module:EnumLogModule.SQL);
        }
        else
        {
            GlobalConfig.CurrentLogger.Log($"将UID为'{uid}'的评论点赞数更新为{newLikes}失败。", module: EnumLogModule.SQL);
        }
        return success;
    }

    public bool AddComment(Comment newComment)
    {
        // 定义表名
        var tableName = "comments";

        // 定义字段列表
        var fieldsList = "usr, uid, commentText, time, likes";

        // 定义值列表，并对特殊字符进行转义（例如引号）
        var valuesList = $"'{newComment.User}', '{newComment.Uid}', '{newComment.CommentText}', '{newComment.Time}', {newComment.Likes}";

        // 执行插入操作
        var success = sqlUtils.Insert(tableName, fieldsList, valuesList);

        if (success)
        {
            GlobalConfig.CurrentLogger.Log($"成功插入新的评论，UID为'{newComment.Uid}'。", module: EnumLogModule.SQL);
        }
        else
        {
            GlobalConfig.CurrentLogger.Log($"插入新的评论失败，UID为'{newComment.Uid}'。", module: EnumLogModule.SQL);
        }
        return success;
    }

    public bool AddComment(string markdownedCommentText, string user)
    {
        var newComment = new Comment()
        {
            CommentText = markdownedCommentText,
            User = user,
            Uid= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").ComputeMd5(),
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Likes = 0
        };

        // 定义表名
        var tableName = "comments";

        // 定义字段列表
        var fieldsList = "usr, uid, commentText, time, likes";

        // 定义值列表，并对特殊字符进行转义（例如引号）
        var valuesList = $"'{newComment.User}', '{newComment.Uid}', '{newComment.CommentText}', '{newComment.Time}', {newComment.Likes}";

        // 执行插入操作
        var success = sqlUtils.Insert(tableName, fieldsList, valuesList);

        if (success)
        {
            GlobalConfig.CurrentLogger.Log($"成功插入新的评论，UID为'{newComment.Uid}'。", module: EnumLogModule.SQL);
        }
        else
        {
            GlobalConfig.CurrentLogger.Log($"插入新的评论失败，UID为'{newComment.Uid}'。", module: EnumLogModule.SQL);
        }
        return success;
    }
}
