﻿//-------------------------------------------------------------------------------------
//     Copyright (c) 2014, Anthilla S.r.l. (http://www.anthilla.com)
//     All rights reserved.
//
//     Redistribution and use in source and binary forms, with or without
//     modification, are permitted provided that the following conditions are met:
//         * Redistributions of source code must retain the above copyright
//           notice, this list of conditions and the following disclaimer.
//         * Redistributions in binary form must reproduce the above copyright
//           notice, this list of conditions and the following disclaimer in the
//           documentation and/or other materials provided with the distribution.
//         * Neither the name of the Anthilla S.r.l. nor the
//           names of its contributors may be used to endorse or promote products
//           derived from this software without specific prior written permission.
//
//     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//     ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//     DISCLAIMED. IN NO EVENT SHALL ANTHILLA S.R.L. BE LIABLE FOR ANY
//     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//     20141110
//-------------------------------------------------------------------------------------

using antdlib.config;
using antdlib.models;
using anthilla.commands;
using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using anthilla.core;

namespace Antd.Modules {
    public class AssetDiscoveryModule : NancyModule {

        public AssetDiscoveryModule() {
            Get["/discovery", true] = async (x, ct) => {
                var devices = await Antd.ServiceDiscovery.Rssdp.Discover();
                var model = new PageAssetDiscoveryModel {
                    List = devices
                };
                return JsonConvert.SerializeObject(model);
            };

            Get["/device/description"] = x => {
                var model = ServiceDiscovery.Rssdp.GetDeviceDescription();
                return JsonConvert.SerializeObject(model);
            };

            Post["/asset/handshake/start"] = x => {
                var hostIp = Request.Form.Host;
                var hostPort = Request.Form.Port;
                const string pathToPrivateKey = "/root/.ssh/id_rsa";
                const string pathToPublicKey = "/root/.ssh/id_rsa.pub";
                if(!File.Exists(pathToPublicKey)) {
                    var k = Bash.Execute($"ssh-keygen -t rsa -N '' -f {pathToPrivateKey}");
                    ConsoleLogger.Log(k);
                }
                var key = File.ReadAllText(pathToPublicKey);
                if(string.IsNullOrEmpty(key)) {
                    return HttpStatusCode.InternalServerError;
                }
                var dict = new Dictionary<string, string> { { "ApplePie", key } };
                var r = new ApiConsumer().Post($"http://{hostIp}:{hostPort}/asset/handshake", dict);
                var kh = new SshKnownHosts();
                kh.Add(hostIp);
                return r;
            };

            Post["/asset/handshake"] = x => {
                string apple = Request.Form.ApplePie;
                var info = apple.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if(info.Length < 2) {
                    return HttpStatusCode.InternalServerError;
                }
                var key = info[0];
                var remoteUser = info[1];
                const string user = "root";
                var model = new AuthorizedKeyModel {
                    RemoteUser = remoteUser,
                    User = user,
                    KeyValue = key
                };
                var authorizedKeysConfiguration = new AuthorizedKeysConfiguration();
                authorizedKeysConfiguration.AddKey(model);
                try {
                    DirectoryWithAcl.CreateDirectory("/root/.ssh");
                    const string authorizedKeysPath = "/root/.ssh/authorized_keys";
                    if(File.Exists(authorizedKeysPath)) {
                        var f = File.ReadAllText(authorizedKeysPath);
                        if(!f.Contains(apple)) {
                            FileWithAcl.AppendAllLines(authorizedKeysPath, new List<string> { apple }, "644", "root", "wheel");
                        }
                    }
                    else {
                        FileWithAcl.WriteAllLines(authorizedKeysPath, new List<string> { apple }, "644", "root", "wheel");
                    }
                    Bash.Execute($"chmod 600 {authorizedKeysPath}", false);
                    Bash.Execute($"chown {user}:{user} {authorizedKeysPath}", false);
                    return HttpStatusCode.OK;
                }
                catch(Exception ex) {
                    ConsoleLogger.Log(ex);
                    return HttpStatusCode.InternalServerError;
                }
            };

            Post["/asset/wol"] = x => {
                string mac = Request.Form.MacAddress;
                CommandLauncher.Launch("wol", new Dictionary<string, string> { { "$mac", mac } });
                return HttpStatusCode.OK;
            };

            Get["/asset/nmap/{ip}"] = x => {
                string ip = x.ip;
                var result = CommandLauncher.Launch("nmap-ip-fast", new Dictionary<string, string> { { "$ip", ip } }).Where(_ => !_.Contains("MAC Address")).Skip(5).Reverse().Skip(1).Reverse();
                var list = new List<NmapScanStatus>();
                foreach(var r in result) {
                    var a = r.SplitToList(" ").ToArray();
                    var mo = new NmapScanStatus {
                        Protocol = a[0],
                        Status = a[1],
                        Type = a[2]
                    };
                    list.Add(mo);
                }
                list = list.OrderBy(_ => _.Protocol).ToList();
                return JsonConvert.SerializeObject(list);
            };
        }
    }
}