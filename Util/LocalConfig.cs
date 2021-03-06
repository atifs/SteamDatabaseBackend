﻿/*
 * Copyright (c) 2013-2015, SteamDB. All rights reserved.
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SteamKit2;

namespace SteamDatabaseBackend
{
    static class LocalConfig
    {
        private static readonly JsonSerializerSettings JsonFormatted = new JsonSerializerSettings { Formatting = Formatting.Indented };

        [JsonObject(MemberSerialization.OptIn)]
        public class CDNAuthToken
        {
            [JsonProperty]
            public string Server { get; set; }

            [JsonProperty]
            public string Token { get; set; }

            [JsonProperty]
            public DateTime Expiration { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        class LocalConfigJson
        {
            [JsonProperty]
            public uint CellID { get; set; } 

            [JsonProperty]
            public string SentryFileName { get; set; } 

            [JsonProperty]
            public byte[] Sentry { get; set; } 

            [JsonProperty]
            public Dictionary<uint, CDNAuthToken> CDNAuthTokens { get; set; } 

            public LocalConfigJson()
            {
                CDNAuthTokens = new Dictionary<uint, CDNAuthToken>();
            }
        }

        public static uint CellID { get; set; }

        public static string SentryFileName { get; set; }

        public static byte[] Sentry { get; set; }

        public static Dictionary<uint, CDNAuthToken> CDNAuthTokens { get; private set; }

        public static void Load()
        {
            var path = GetPath();

            LocalConfigJson current;

            if (File.Exists(path))
            {
                current = JsonConvert.DeserializeObject<LocalConfigJson>(File.ReadAllText(path));
            }
            else
            {
                current = new LocalConfigJson();
            }

            CellID = current.CellID;
            Sentry = current.Sentry;
            SentryFileName = current.SentryFileName;
            CDNAuthTokens = current.CDNAuthTokens;

            if (!File.Exists(path))
            {
                Save();
            }
        }

        public static void Save()
        {
            Log.WriteDebug("Local Config", "Saving...");

            var current = new LocalConfigJson
            {
                CellID = CellID,
                Sentry = Sentry,
                SentryFileName = SentryFileName,
                CDNAuthTokens = CDNAuthTokens,
            };

            string json;

            lock (CDNAuthTokens)
            {
                json = JsonConvert.SerializeObject(current, JsonFormatted);
            }

            File.WriteAllText(GetPath(), json);
        }

        private static string GetPath()
        {
            return Path.Combine(Application.Path, "files", ".support", "localconfig.json");
        }
    }
}
