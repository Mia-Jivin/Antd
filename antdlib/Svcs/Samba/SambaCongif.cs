﻿
using antdlib.MountPoint;
///-------------------------------------------------------------------------------------
///     Copyright (c) 2014, Anthilla S.r.l. (http://www.anthilla.com)
///     All rights reserved.
///
///     Redistribution and use in source and binary forms, with or without
///     modification, are permitted provided that the following conditions are met:
///         * Redistributions of source code must retain the above copyright
///           notice, this list of conditions and the following disclaimer.
///         * Redistributions in binary form must reproduce the above copyright
///           notice, this list of conditions and the following disclaimer in the
///           documentation and/or other materials provided with the distribution.
///         * Neither the name of the Anthilla S.r.l. nor the
///           names of its contributors may be used to endorse or promote products
///           derived from this software without specific prior written permission.
///
///     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
///     ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
///     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
///     DISCLAIMED. IN NO EVENT SHALL ANTHILLA S.R.L. BE LIABLE FOR ANY
///     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
///     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
///     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
///     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
///     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
///     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
///
///     20141110
///-------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace antdlib.Svcs.Samba {
    public class SambaConfig {

        private static string serviceGuid = "65754563-A529-4017-A7AF-AB2A6B64F56A";

        private static string dir = "/etc/samba";

        private static string DIR = Mount.SetDIRSPath(dir);

        private static string mainFile = "smb.conf";

        public class MapRules {
            public static char CharComment { get { return ';'; } }

            public static string VerbInclude { get { return "include"; } }

            public static char CharKevValueSeparator { get { return '='; } }

            public static char CharValueArraySeparator { get { return ','; } }

            public static char CharEndOfLine { get { return '\n'; } }

            public static char CharSectionOpen { get { return '['; } }

            public static char CharSectionClose { get { return ']'; } }
        }

        public class LineModel {
            public string FilePath { get; set; }

            public string Key { get; set; }

            public string Value { get; set; }

            public ServiceDataType Type { get; set; }

            public KeyValuePair<string, string> BooleanVerbs { get; set; }

            public bool IsShare { get; set; }
        }

        public class SambaModel {
            public string _Id { get; set; }

            public string Guid { get; set; }

            public string Timestamp { get; set; }

            public List<LineModel> Data { get; set; } = new List<LineModel>() { };
        }

        public class MapFile {

            private static string CleanText(string text) {
                var clean = text.Replace("\t", " ");
                return clean;
            }

            private static IEnumerable<LineModel> ReadFile(string path) {
                var text = FileSystem.ReadFile(path);
                var cleanText = CleanText(text);
                var lines = text.Split(MapRules.CharEndOfLine);
                foreach (var line in lines) {
                    if (line != "") {
                        yield return ReadLine(path, line);
                    }
                }
            }

            private static LineModel ReadLine(string path, string line) {
                var keyValuePair = line.Split(new String[] { MapRules.CharKevValueSeparator.ToString() }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                ServiceDataType type;
                var isShare = false;
                var key = (keyValuePair.Length > 0) ? keyValuePair[0] : "";
                var value = "";
                if (line.StartsWith(MapRules.CharComment.ToString())) {
                    type = ServiceDataType.Disabled;
                }
                else if (line.StartsWith(MapRules.CharSectionOpen.ToString())) {
                    type = ServiceDataType.Disabled;
                    isShare = true;
                }
                else {
                    type = SupposeDataType(value);
                    value = (keyValuePair.Length > 1) ? keyValuePair[1] : "";
                }
                KeyValuePair<string, string> booleanVerbs;
                if (type == ServiceDataType.Boolean) {
                    booleanVerbs = SupposeBooleanVerbs(value);
                }
                else {
                    booleanVerbs = new KeyValuePair<string, string>("", "");
                }
                var model = new LineModel() {
                    FilePath = path,
                    Key = key.Trim(),
                    Value = value.Trim(),
                    Type = type,
                    BooleanVerbs = booleanVerbs,
                    IsShare = isShare
                };
                return model;
            }

            private static ServiceDataType SupposeDataType(string value) {
                if (value == "true" || value == "True" ||
                    value == "false" || value == "False" ||
                    value == "yes" || value == "Yes" ||
                    value == "no" || value == "No") {
                    return ServiceDataType.Boolean;
                }
                else if (value.Length > 5 && value.Contains(",")) {
                    return ServiceDataType.StringArray;
                }
                else {
                    return ServiceDataType.String;
                }
            }

            private static KeyValuePair<string, string> SupposeBooleanVerbs(string value) {
                if (value == "true" || value == "false") {
                    return new KeyValuePair<string, string>("true", "false");
                }
                else if (value == "True" || value == "False") {
                    return new KeyValuePair<string, string>("True", "False");
                }
                else if (value == "yes" || value == "no") {
                    return new KeyValuePair<string, string>("yes", "no");
                }
                else if (value == "Yes" || value == "No") {
                    return new KeyValuePair<string, string>("Yes", "No");
                }
                else {
                    return new KeyValuePair<string, string>("", "");
                }
            }

            private static void Create() {
                var samba = new SambaModel() {
                    _Id = serviceGuid,
                    Guid = serviceGuid,
                    Timestamp = Timestamp.Now
                };
                DeNSo.Session.New.Set(samba);
            }

            private static void AddLines(string path) {
                var samba = DeNSo.Session.New.Get<SambaModel>(s => s.Guid == serviceGuid).FirstOrDefault();
                samba.Timestamp = Timestamp.Now;
                foreach (var data in ReadFile(path)) {
                    samba.Data.Add(data);
                }
                DeNSo.Session.New.Set(samba);
            }

            private static void FlushData() {
                var samba = DeNSo.Session.New.Get<SambaModel>(s => s.Guid == serviceGuid).FirstOrDefault();
                samba.Timestamp = Timestamp.Now;
                samba.Data = new List<LineModel>() { };
                DeNSo.Session.New.Set(samba);
            }

            public static void Render() {
                if (DeNSo.Session.New.Get<SambaModel>(s => s.Guid == serviceGuid).FirstOrDefault() == null) {
                    Create();
                }
                FlushData();

                foreach (var file in SimpleStructure) {
                    AddLines(file);
                }
            }

            public static SambaModel Get() {
                return DeNSo.Session.New.Get<SambaModel>(s => s.Guid == serviceGuid).FirstOrDefault();
            }
        }

        public static void SetReady() {
            Terminal.Execute($"cp {dir} {DIR}");
            FileSystem.CopyDirectory(dir, DIR);
            Mount.Dir(dir);
        }

        private static bool CheckIsActive() {
            var mount = MountRepository.Get(dir);
            return (mount == null) ? false : true;
        }

        public static bool IsActive { get { return CheckIsActive(); } }

        private static List<KeyValuePair<string, List<string>>> GetServiceStructure() {
            var list = new List<KeyValuePair<string, List<string>>>() { };
            var files = Directory.EnumerateFiles(DIR, "*.conf", SearchOption.AllDirectories).ToArray();
            for (int i = 0; i < files.Length; i++) {
                if (File.ReadLines(files[i]).Any(line => line.Contains("include"))) {
                    var lines = File.ReadLines(files[i]).Where(line => line.Contains("include")).ToList();
                    var dump = new List<string>() { };
                    foreach (var line in lines) {
                        dump.Add(line.Split('=')[1].Trim().Replace(dir, DIR));
                    }
                    list.Add(new KeyValuePair<string, List<string>>(files[i].Replace("\\", "/"), dump));
                }
            }
            if (list.Count() < 1) {
                list.Add(new KeyValuePair<string, List<string>>($"{DIR}/{mainFile}", new List<string>() { }));
            }
            return list;
        }

        public static List<KeyValuePair<string, List<string>>> Structure { get { return GetServiceStructure(); } }

        private static List<string> GetServiceSimpleStructure() {
            var list = new List<string>() { };
            var files = Directory.EnumerateFiles(DIR, "*.conf", SearchOption.AllDirectories).ToArray();
            for (int i = 0; i < files.Length; i++) {
                if (File.ReadLines(files[i]).Any(line => line.Contains("include"))) {
                    var lines = File.ReadLines(files[i]).Where(line => line.Contains("include")).ToList();
                    foreach (var line in lines) {
                        list.Add(line.Split('=')[1].Trim().Replace(dir, DIR));
                    }
                }
            }
            if (list.Count() < 1) {
                list.Add($"{DIR}/{mainFile}");
            }
            return list;
        }

        public static List<string> SimpleStructure { get { return GetServiceSimpleStructure(); } }

    }
}
