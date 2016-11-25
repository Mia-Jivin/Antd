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

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using antd.commands;
using antdlib.common;
using antdlib.common.Tool;
using antdlib.views;
using Antd.Bind;
using Antd.Configuration;
using Antd.Database;
using Antd.Dhcpd;
using Antd.Discovery;
using Antd.Firewall;
using Antd.Gluster;
using Antd.Host;
using Antd.Info;
using Antd.Log;
using Antd.MountPoint;
using Antd.Network;
using Antd.Overlay;
using Antd.Samba;
using Antd.Ssh;
using Antd.Storage;
using Antd.SystemdTimer;
using Antd.Users;
using Nancy.Security;

namespace Antd.Modules {
    public class PartialHomeModule : CoreModule {

        public PartialHomeModule() {
            this.RequiresAuthentication();

            #region [    Page - Config    ]
            Get["/part/info"] = x => {
                try {
                    var bash = new Bash();
                    var launcher = new CommandLauncher();
                    var machineInfo = new MachineInfo();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.VersionOS = bash.Execute("uname -a");
                    viewModel.AosInfo = machineInfo.GetAosrelease();
                    viewModel.Uptime = machineInfo.GetUptime();
                    viewModel.GentooRelease = launcher.Launch("cat-etc-gentoorel").JoinToString("<br />");
                    viewModel.LsbRelease = launcher.Launch("cat-etc-lsbrel").JoinToString("<br />");
                    viewModel.OsRelease = launcher.Launch("cat-etc-osrel").JoinToString("<br />");
                    return View["antd/part/page-antd-info", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/info/memory"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var machineInfo = new MachineInfo();
                    viewModel.Meminfo = machineInfo.GetMeminfo();
                    viewModel.Free = machineInfo.GetFree();
                    return View["antd/part/page-antd-info-memory", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/info/cpu"] = x => {
                try {
                    var machineInfo = new MachineInfo();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.Cpuinfo = machineInfo.GetCpuinfo();
                    return View["antd/part/page-antd-info-cpu", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/info/services"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var machineInfo = new MachineInfo();
                    viewModel.Services = machineInfo.GetServices();
                    return View["antd/part/page-antd-info-services", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/system"] = x => {
                try {
                    var machineInfo = new MachineInfo();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.SystemComponents = machineInfo.GetSystemComponentModels();
                    return View["antd/part/page-antd-system", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/system/losetup"] = x => {
                try {
                    var machineInfo = new MachineInfo();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.LosetupInfo = machineInfo.GetLosetup();
                    return View["antd/part/page-antd-system-losetup", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/system/update"] = x => {
                try {
                    var launcher = new CommandLauncher();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.AntdUpdateCheck = launcher.Launch("mono-antdsh-update-check").JoinToString("<br />");
                    return View["antd/part/page-antd-system-update", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/host"] = x => {
                try {
                    var launcher = new CommandLauncher();
                    dynamic viewModel = new ExpandoObject();
                    var hostnamectl = launcher.Launch("hostnamectl").ToList();
                    var ssoree = StringSplitOptions.RemoveEmptyEntries;
                    viewModel.StaticHostname = hostnamectl.First(_ => _.Contains("Transient hostname:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.IconName = hostnamectl.First(_ => _.Contains("Icon name:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Chassis = hostnamectl.First(_ => _.Contains("Chassis:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Deployment = hostnamectl.First(_ => _.Contains("Deployment:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Location = hostnamectl.First(_ => _.Contains("Location:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.MachineID = hostnamectl.First(_ => _.Contains("Machine ID:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.BootID = hostnamectl.First(_ => _.Contains("Boot ID:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Virtualization = hostnamectl.First(_ => _.Contains("Virtualization:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.OS = hostnamectl.First(_ => _.Contains("Operating System:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Kernel = hostnamectl.First(_ => _.Contains("Kernel:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Architecture = hostnamectl.First(_ => _.Contains("Architecture:")).Split(new[] { ":" }, 2, ssoree)[1];
                    return View["antd/part/page-antd-host", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/time"] = x => {
                try {
                    var bash = new Bash();
                    var launcher = new CommandLauncher();
                    dynamic viewModel = new ExpandoObject();
                    var timezones = bash.Execute("timedatectl list-timezones --no-pager").SplitBash();
                    viewModel.Timezones = timezones;
                    var timedatectl = launcher.Launch("timedatectl").ToList();
                    var ssoree = StringSplitOptions.RemoveEmptyEntries;
                    viewModel.LocalTime = timedatectl.First(_ => _.Contains("Local time:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.UnivTime = timedatectl.First(_ => _.Contains("Universal time:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.RTCTime = timedatectl.First(_ => _.Contains("RTC time:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Timezone = timedatectl.First(_ => _.Contains("Time zone:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Nettimeon = timedatectl.First(_ => _.Contains("Network time on:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Ntpsync = timedatectl.First(_ => _.Contains("NTP synchronized:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.Rtcintz = timedatectl.First(_ => _.Contains("RTC in local TZ:")).Split(new[] { ":" }, 2, ssoree)[1];
                    return View["antd/part/page-antd-time", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/ns"] = x => {
                try {
                    var launcher = new CommandLauncher();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.Hostname = launcher.Launch("cat-etc-hostname").JoinToString("<br />");
                    viewModel.Hosts = launcher.Launch("cat-etc-hosts").JoinToString("<br />");
                    viewModel.Networks = launcher.Launch("cat-etc-networks").JoinToString("<br />");
                    viewModel.Resolv = launcher.Launch("cat-etc-resolv").JoinToString("<br />");
                    viewModel.Nsswitch = launcher.Launch("cat-etc-nsswitch").JoinToString("<br />");
                    return View["antd/part/page-antd-ns", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/named"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var bindConfiguration = new BindConfiguration();
                    var bindIsActive = bindConfiguration.IsActive();
                    viewModel.BindIsActive = bindIsActive;
                    viewModel.BindOptions = bindConfiguration.Get() ?? new BindConfigurationModel();
                    viewModel.BindZones = bindConfiguration.Get()?.Zones;
                    return View["antd/part/page-antd-bind", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/dhcp"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var dhcpdConfiguration = new DhcpdConfiguration();
                    var dhcpdIsActive = dhcpdConfiguration.IsActive();
                    viewModel.DhcpdIsActive = dhcpdIsActive;
                    viewModel.DhcpdOptions = dhcpdConfiguration.Get() ?? new DhcpdConfigurationModel();
                    viewModel.DhcpdClass = dhcpdConfiguration.Get()?.Classes;
                    viewModel.DhcpdPools = dhcpdConfiguration.Get()?.Pools;
                    viewModel.DhcpdReservation = dhcpdConfiguration.Get()?.Reservations;
                    return View["antd/part/page-antd-dhcp", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/dhcp/leases"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var dhcpdLeases = new DhcpdLeases();
                    var list = dhcpdLeases.List();
                    viewModel.DhcpdLeases = list;
                    viewModel.EmptyList = !list.Any();
                    return View["antd/part/page-antd-dhcp-leases", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/samba"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var sambaConfiguration = new SambaConfiguration();
                    var sambaIsActive = sambaConfiguration.IsActive();
                    viewModel.SambaIsActive = sambaIsActive;
                    viewModel.SambaOptions = sambaConfiguration.Get() ?? new SambaConfigurationModel();
                    viewModel.SambaResources = sambaConfiguration.Get()?.Resources;
                    return View["antd/part/page-antd-samba", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/net"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var nif = new NetworkInterfaces();
                    var networkInterfaces = nif.GetAll().ToList();
                    var phyIf =
                        networkInterfaces.Where(
                                _ =>
                                    _.Value ==
                                    NetworkInterfaces.NetworkInterfaceType.Physical)
                            .OrderBy(_ => _.Key);
                    viewModel.NetworkPhysicalIf = phyIf;
                    var brgIf =
                        networkInterfaces.Where(
                                _ =>
                                    _.Value ==
                                    NetworkInterfaces.NetworkInterfaceType.Bridge)
                            .OrderBy(_ => _.Key);
                    viewModel.NetworkBridgeIf = brgIf;
                    var bndIf =
                        networkInterfaces.Where(
                                _ =>
                                    _.Value ==
                                    NetworkInterfaces.NetworkInterfaceType.Bond)
                            .OrderBy(_ => _.Key);
                    viewModel.NetworkBondIf = bndIf;
                    var vrtIf =
                        networkInterfaces.Where(
                                _ =>
                                    _.Value ==
                                    NetworkInterfaces.NetworkInterfaceType.Virtual)
                            .OrderBy(_ => _.Key)
                            .ToList();
                    foreach(var v in vrtIf) {
                        if(phyIf.Contains(v) || brgIf.Contains(v) || bndIf.Contains(v)) {
                            vrtIf.Remove(v);
                        }
                    }
                    viewModel.NetworkVirtualIf = vrtIf;
                    return View["antd/part/page-antd-network", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/fw"] = x => {
                try {
                    var nfTables = new NfTables();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.NftTables = nfTables.Tables();
                    viewModel.MacAddressList = new MacAddressRepository().GetAll();
                    return View["antd/part/page-antd-firewall", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error(
                        $"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/cron"] = x => {
                try {
                    var timers = new Timers();
                    dynamic viewModel = new ExpandoObject();
                    var scheduledJobs = timers.GetAll();
                    viewModel.Jobs = scheduledJobs?.ToList().OrderBy(_ => _.Alias);
                    return View["antd/part/page-antd-scheduler", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/storage"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var disks = new Disks();
                    viewModel.DisksList = disks.GetList();
                    return View["antd/part/page-antd-storage", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/storage/zfs"] = x => {
                try {
                    var zpool = new Zpool();
                    var zfsSnap = new ZfsSnap();
                    var zfs = new Zfs();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.ZpoolList = zpool.List();
                    viewModel.ZfsList = zfs.List();
                    viewModel.ZfsSnap = zfsSnap.List();
                    viewModel.ZpoolHistory = zpool.History();
                    return View["antd/part/page-antd-storage-zfs", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/storage/usage"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var diskUsage = new DiskUsage();
                    viewModel.DisksUsage = diskUsage.GetInfo();
                    return View["antd/part/page-antd-storage-usage", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/storage/mounts"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    viewModel.Mounts = new Mount().GetAll();
                    return View["antd/part/page-antd-storage-mounts", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/storage/overlay"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    viewModel.Overlay = OverlayWatcher.ChangedDirectories;
                    return View["antd/part/page-antd-storage-overlay", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/sync"] = x => {
                try {
                    var glusterConfiguration = new GlusterConfiguration();
                    dynamic viewModel = new ExpandoObject();
                    var glusterConfig = glusterConfiguration.Get();
                    viewModel.GlusterName = glusterConfig.Name;
                    viewModel.GlusterPath = glusterConfig.Path;
                    viewModel.GlusterNodes = glusterConfig.Nodes;
                    viewModel.GlusterVolumes = glusterConfig.Volumes;
                    return View["antd/part/page-antd-gluster", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/vm"] = x => {
                try {
                    var virsh = new Virsh.Virsh();
                    dynamic viewModel = new ExpandoObject();
                    var vmList = virsh.GetVmList();
                    viewModel.VMListAny = vmList.Any();
                    viewModel.VMList = vmList;
                    return View["antd/part/page-antd-vm", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/users"] = x => {
                try {
                    var userRepository = new UserRepository();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.Master = new ManageMaster().Name;
                    viewModel.Users = userRepository.GetAll().OrderBy(_ => _.Alias);
                    return View["antd/part/page-antd-users", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/ssh"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    viewModel.Keys = null;
                    return View["antd/part/page-antd-ssh", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };
            #endregion

            #region [    Page - C A    ]
            Get["/part/ca/dc"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    return View["antd/part/page-ca-dc", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/ca/dcusers"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    return View["antd/part/page-ca-dcusers", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/ca/setup"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    return View["antd/part/page-ca-setup", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/ca/cert"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    return View["antd/part/page-ca-cert", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/ca/certdc"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    return View["antd/part/page-ca-certdc", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/ca/certsc"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    return View["antd/part/page-ca-certsc", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };
            #endregion

            #region [    Page - Boot    ]
            Get["/part/boot/cmd"] = x => {
                try {
                    var setupConfiguration = new SetupConfiguration();
                    dynamic viewModel = new ExpandoObject();
                    viewModel.HasConfiguration = true;
                    viewModel.Controls = setupConfiguration.Get();
                    if(setupConfiguration.Get().Count < 1) {
                        viewModel.HasConfiguration = false;
                    }
                    return View["antd/part/page-boot-cmd", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/boot/mod"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var hostcfg = new HostConfiguration();
                    viewModel.Modules = string.Join("\r\n", hostcfg.GetHostModprobes());
                    viewModel.RmModules = string.Join("\r\n", hostcfg.GetHostRemoveModules());
                    return View["antd/part/page-boot-mod", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/boot/svc"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var hostcfg = new HostConfiguration();
                    viewModel.Services = string.Join("\r\n", hostcfg.GetHostServices());
                    return View["antd/part/page-boot-svc", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/boot/osp"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var hostcfg = new HostConfiguration();
                    viewModel.OsParam = string.Join("\r\n", hostcfg.GetHostOsParameters().Select(_ => $"{_.Key} {_.Value}").ToList());
                    return View["antd/part/page-boot-osp", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };
            #endregion

            #region [    Page - Asset    ]
            Get["/part/asset/discovery"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var avahiBrowse = new AvahiBrowse();
                    avahiBrowse.DiscoverService("antd");
                    var localServices = avahiBrowse.Locals;
                    var launcher = new CommandLauncher();
                    var list = new List<AssetModule.AvahiServiceViewModel>();
                    var kh = new SshKnownHosts();
                    foreach(var ls in localServices) {
                        var arr = ls.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                        var mo = new AssetModule.AvahiServiceViewModel {
                            HostName = arr[0].Trim(),
                            Ip = arr[1].Trim(),
                            Port = arr[2].Trim(),
                            MacAddress = ""
                        };
                        launcher.Launch("ping-c", new Dictionary<string, string> { { "$ip", arr[1].Trim() } });
                        var result = launcher.Launch("arp", new Dictionary<string, string> { { "$ip", arr[1].Trim() } }).ToList();
                        if(result.Any()) {
                            var mac = result.LastOrDefault().Print(3, " ");
                            mo.MacAddress = mac;
                        }
                        mo.IsKnown = kh.Hosts.Contains(arr[1].Trim());
                        list.Add(mo);
                    }
                    //var hostnamectl = launcher.Launch("hostnamectl").ToList();
                    //var ssoree = StringSplitOptions.RemoveEmptyEntries;
                    //var myHostName = hostnamectl?.First(_ => _.Contains("Transient hostname:")).Split(new[] { ":" }, 2, ssoree)[1];
                    viewModel.AntdAvahiServices = list/*.Where(_ => !_.HostName.ToLower().Contains(myHostName.ToLower())).OrderBy(_ => _.HostName)*/;
                    return View["antd/part/page-asset-discovery", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/asset/setting"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var settings = new NetscanSetting();
                    viewModel.SettingsSubnet = settings.Settings.Subnet;
                    viewModel.SettingsSubnetLabel = settings.Settings.SubnetLabel;
                    viewModel.Settings = settings.Settings.Values;
                    return View["antd/part/page-asset-setting", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };
            #endregion

            #region [    Page - Log    ]
            Get["/part/log"] = x => {
                try {
                    return View["antd/part/page-log"];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/log/system"] = x => {
                try {
                    return View["antd/part/page-log-system"];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/log/report"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var journalctlReport = new Journalctl.Report();
                    viewModel.LogReports = journalctlReport.Get();
                    return View["antd/part/page-log-report", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };

            Get["/part/log/syslog"] = x => {
                try {
                    dynamic viewModel = new ExpandoObject();
                    var syslogRepository = new SyslogRepository();
                    var syslogConfig = syslogRepository.Get();
                    viewModel.SyslogConfig = syslogConfig ?? new SyslogSchema();
                    var syslogNg = new SyslogNg();
                    viewModel.SyslogNgContent = syslogNg.GetAll().OrderBy(_ => _.Host).ThenByDescending(_ => _.DateTime);
                    return View["antd/part/page-log-syslog", viewModel];
                }
                catch(Exception ex) {
                    ConsoleLogger.Error($"{Request.Url} request failed: {ex.Message}");
                    ConsoleLogger.Error(ex);
                    return View["antd/part/page-error"];
                }
            };
            #endregion
        }
    }
}