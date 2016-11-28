﻿using System;
using System.Linq;
using System.IO;
using antdlib.common;
using Newtonsoft.Json;
using System.Collections.Generic;
using antd.commands;

namespace Antd.Host {
    public class HostConfiguration {

        public string FilePath { get; }
        public string FilePathBackup { get; }
        public HostModel Host { get; private set; }

        public HostConfiguration() {
            FilePath = $"{Parameter.AntdCfg}/host.conf";
            FilePathBackup = $"{Parameter.AntdCfg}/host.conf.bck";
            Host = LoadHostModel();
        }

        private HostModel LoadHostModel() {
            if(!File.Exists(FilePath)) {
                return new HostModel();
            }
            try {
                return JsonConvert.DeserializeObject<HostModel>(File.ReadAllText(FilePath));
            }
            catch(Exception) {
                return new HostModel();
            }
        }

        public void Setup() {
            if(!File.Exists(FilePath)) {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(new HostModel(), Formatting.Indented));
            }
        }

        public void Export(HostModel model) {
            if(File.Exists(FilePath)) {
                File.Copy(FilePath, FilePathBackup, true);
            }
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(model, Formatting.Indented));
        }

        #region [    repo - Modules    ]
        public string[] GetHostModprobes() {
            Host = LoadHostModel();
            var result = new List<string>();
            foreach(Dictionary<string, string> dict in Host.Modprobes.Select(_ => _.StoredValues)) {
                result.AddRange(dict.Select(_ => _.Value));
            }
            return result.ToArray();
        }

        public void SetHostModprobes(IEnumerable<string> modules) {
            Host = LoadHostModel();
            Host.Modprobes = modules.Select(_ => new HostParameter { SetCmd = "modprobe", StoredValues = new Dictionary<string, string> { { "$package", _ } } }).ToArray();
            Export(Host);
        }

        public void ApplyHostModprobes() {
            Host = LoadHostModel();
            var launcher = new CommandLauncher();
            foreach(var modprobe in Host.Modprobes) {
                launcher.Launch(modprobe.SetCmd, modprobe.StoredValues);
            }
        }

        public string[] GetHostRemoveModules() {
            Host = LoadHostModel();
            return Host.RemoveModules.StoredValues.Select(_ => _.Value).First().SplitToList(" ").ToArray();
        }

        public void SetHostRemoveModules(IEnumerable<string> modules) {
            Host = LoadHostModel();
            Host.RemoveModules = new HostParameter { SetCmd = "modprobe", StoredValues = new Dictionary<string, string> { { "$package", string.Join(" ", modules) } } };
            Export(Host);
        }

        public void ApplyHostRemoveModules() {
            Host = LoadHostModel();
            var launcher = new CommandLauncher();
            launcher.Launch(Host.RemoveModules.SetCmd, Host.RemoveModules.StoredValues);
        }
        #endregion

        #region [    repo - Services    ]
        public string[] GetHostServices() {
            Host = LoadHostModel();
            var result = new List<string>();
            foreach(Dictionary<string, string> dict in Host.Services.Select(_ => _.StoredValues)) {
                result.AddRange(dict.Select(_ => _.Value));
            }
            return result.ToArray();
        }

        public void SetHostServices(IEnumerable<string> services) {
            Host = LoadHostModel();
            Host.Services = services.Select(_ => new HostParameter { SetCmd = "systemctl-restart", StoredValues = new Dictionary<string, string> { { "$service", _ } } }).ToArray();
            Export(Host);
        }

        public void ApplyHostServices() {
            Host = LoadHostModel();
            var launcher = new CommandLauncher();
            foreach(var modprobe in Host.Services) {
                launcher.Launch(modprobe.SetCmd, modprobe.StoredValues);
            }
        }
        #endregion

        #region [    repo - OS Parameters    ]
        public Dictionary<string, string> GetHostOsParameters() {
            Host = LoadHostModel();
            var ks = Host.OsParameters.Select(_ => _.StoredValues.Where(__ => __.Key == "$file").Select(__ => __.Value)).SelectMany(x => x).ToArray();
            var vs = Host.OsParameters.Select(_ => _.StoredValues.Where(__ => __.Key == "$value").Select(__ => __.Value)).SelectMany(x => x).ToArray();
            var dict = new Dictionary<string, string>();
            if(ks.Length != vs.Length)
                return dict;
            for(var i = 0; i < ks.Length; i++) {
                if(!dict.ContainsKey(ks[i])) {
                    dict.Add(ks[i], vs[i]);
                }
            }
            return dict;
        }

        public void SetHostOsParameters(Dictionary<string, string> parameters) {
            Host = LoadHostModel();
            Host.OsParameters = parameters.Select(_ => new HostParameter {
                SetCmd = "echo-append",
                StoredValues = new Dictionary<string, string> {
                    { "$file", _.Key }, { "$value", _.Value }
                }
            }).ToArray();
            if(Host.OsParameters.Any()) {
                Host.OsParameters.FirstOrDefault().SetCmd = "echo-write";
            }
            Setup();
        }

        public void ApplyHostOsParameters() {
            Host = LoadHostModel();
            var launcher = new CommandLauncher();
            foreach(var modprobe in Host.OsParameters) {
                launcher.Launch(modprobe.SetCmd, modprobe.StoredValues);
            }
        }
        #endregion

        #region [    repo - Compile /etc/networks    ]
        public Dictionary<string, string> GetHostEtcNetworks() {
            Host = LoadHostModel();
            var dicts = Host.EtcNetworks.Select(_ => _.StoredValues);
            var dict = new Dictionary<string, string>();
            foreach(var d in dicts) {
                dict = dict.Merge(d);
            }
            return dict;
        }

        public void SetHostEtcNetworks(string value) {
            Host = LoadHostModel();
            var etcNetworks = Host.EtcNetworks;
            var dicts = etcNetworks.Select(_ => _.StoredValues);
            var dict = new Dictionary<string, string>();
            foreach(var d in dicts) {
                dict = dict.Merge(d);
            }
            if(dict.ContainsValue(value)) {
                return;
            }
            etcNetworks.ToList().Add(new HostParameter { SetCmd = "echo-write", StoredValues = new Dictionary<string, string> { { "$file", "/etc/networks" }, { "$value", value } } });
            Host.EtcNetworks = etcNetworks.ToArray();
            Export(Host);
        }

        public void ApplyHostEtcNetworks() {
            Host = LoadHostModel();
            var launcher = new CommandLauncher();
            foreach(var modprobe in Host.EtcNetworks) {
                launcher.Launch(modprobe.SetCmd, modprobe.StoredValues);
            }
        }
        #endregion

        #region [    repo - Host Info    ]
        public HostInfoModel GetHostInfo() {
            Host = LoadHostModel();
            var host = new HostInfoModel {
                Name = Host.HostName.StoredValues["$host_name"],
                Chassis = Host.HostName.StoredValues["$host_chassis"],
                Deployment = Host.HostName.StoredValues["$host_deployment"],
                Location = Host.HostName.StoredValues["$host_location"]
            };
            return host;
        }

        public void SetHostInfoName(string name) {
            Host = LoadHostModel();
            Host.HostName.StoredValues["$host_name"] = name;
            Export(Host);
        }

        public void SetHostInfoChassis(string chassis) {
            Host = LoadHostModel();
            Host.HostName.StoredValues["$host_chassis"] = chassis;
            Export(Host);
        }

        public void SetHostInfoDeployment(string deployment) {
            Host = LoadHostModel();
            Host.HostName.StoredValues["$host_deployment"] = deployment;
            Export(Host);
        }

        public void SetHostInfoLocation(string location) {
            Host = LoadHostModel();
            Host.HostName.StoredValues["$host_location"] = location;
            Export(Host);
        }

        public void SetHostInfo(string name, string chassis, string deployment, string location) {
            Host = LoadHostModel();
            Host.HostName.StoredValues["$host_name"] = name;
            Host.HostName.StoredValues["$host_chassis"] = chassis;
            Host.HostName.StoredValues["$host_deployment"] = deployment;
            Host.HostName.StoredValues["$host_location"] = location;
            Export(Host);
        }

        public void ApplyHostInfo() {
            Host = LoadHostModel();
            var launcher = new CommandLauncher();
            launcher.Launch(Host.HostName.SetCmd, Host.HostName.StoredValues);
            launcher.Launch(Host.HostChassis.SetCmd, Host.HostChassis.StoredValues);
            launcher.Launch(Host.HostDeployment.SetCmd, Host.HostDeployment.StoredValues);
            launcher.Launch(Host.HostLocation.SetCmd, Host.HostLocation.StoredValues);
        }
        #endregion

        #region [    repo - Timezone    ]
        public string GetTimezone() {
            Host = LoadHostModel();
            var timezone = Host.Timezone.StoredValues["$host_timezone"];
            return timezone;
        }

        public void SetTimezone(string timezone) {
            Host = LoadHostModel();
            Host.Timezone.StoredValues["$host_timezone"] = timezone;
            Export(Host);
        }

        public void ApplyTimezone() {
            Host = LoadHostModel();
            var launcher = new CommandLauncher();
            launcher.Launch(Host.Timezone.SetCmd, Host.Timezone.StoredValues);
        }
        #endregion
    }
}