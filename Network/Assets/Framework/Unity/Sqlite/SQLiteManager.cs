﻿using Framework.Unity.Pattern;
using Mono.Data.Sqlite;
using System;
using UnityEngine;

namespace Framework.Unity.Sqlite
{
    public class SQLiteManager : SingletonMono<SQLiteManager>, IDisposable
    {

        SQLiteHelper dbAccess;

        public bool InitFinish
        {
            get
            {
                return dbAccess != null;
            }
        }

        public void Init()
        {
            string dbName = "data.db";

#if UNITY_EDITOR || UNITY_IOS || UNITY_STANDALONE
            string appDbPath = string.Format("{0}/{1}", GetStreamingPath(), dbName);
#else
            string appDbPath = string.Format("{0}/{1}", Application.persistentDataPath, dbName);
            if (!File.Exists(appDbPath))
            {
                //用www先从Unity中下载到数据库
                string tempProjectDbPath = string.Format("{0}/{1}", GetStreamingPath(), dbName);
                WWW loadDB = new WWW(tempProjectDbPath);
                while (!loadDB.isDone) { }

                //拷贝至规定的地方
                File.WriteAllBytes(appDbPath, loadDB.bytes);
                loadDB.Dispose();
            }
            else
            {
                //用www先从Unity中下载到数据库
                string tempProjectDbPath = string.Format("{0}/{1}", GetStreamingPath(), dbName);
                WWW loadDB = new WWW(tempProjectDbPath);
                while (!loadDB.isDone) { }
       
                string tempStr1 = CustomFile.GetFileHash(appDbPath);
                string tempStr2 = CustomFile.GetBytesHash(loadDB.bytes);

                if (tempStr1 != tempStr2)
                {
                    Debuger.Log("字节不同，重新写入数据");
                    File.WriteAllBytes(appDbPath, loadDB.bytes);
                }
                loadDB.Dispose();
            }
#endif
            if (Application.isMobilePlatform && Application.platform == RuntimePlatform.Android)
            {
                string tempPath = string.Format("URI = file:{0}", appDbPath);
                dbAccess = new SQLiteHelper(tempPath);
            }
            else
            {
                string tempPath = string.Format("data source={0}", appDbPath);
                dbAccess = new SQLiteHelper(tempPath);
            }
        }

        public SqliteDataReader GetTable(string varTableName)
        {
            if (dbAccess == null)
            {
                Init();
            }
            //读取整张表
            SqliteDataReader reader = dbAccess.ReadFullTable(varTableName);
            return reader;
        }

        private void OnApplicationQuit()
        {
            CloseConnection();
        }

        public void CloseConnection()
        {
            if (dbAccess != null)
            {
                dbAccess.Dispose();
            }
        }

        public string GetStreamingPath()
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                return string.Format("{0}/StreamingAssets", Application.dataPath);
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                return string.Format("jar:file://{0}!/assets", Application.dataPath);
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return string.Format("{0}/Raw", Application.dataPath);
            }
            return null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseConnection();
            }
        }
    }
}
