﻿///-------------------------------------------------------------------------------------
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

namespace antdlib.Network {
    public class NetworkConfig {
        public class Iproute2 {

            public static string AddNewAddressIPV4(string range, string address, string interfaceName) {
                return Terminal.Execute($"ip addr add {range} broadcast {address} dev {interfaceName}");
            }

            public static string DeleteAddressIPV4(string range, string address, string interfaceName) {
                return Terminal.Execute($"ip addr del {range} broadcast {address} dev {interfaceName}");
            }

            public static string FlushConfigurationIPV4(string interfaceName = null) {
                var i = (interfaceName == null) ? "label \"eth *\"" : "dev {interfaceName}";
                return Terminal.Execute($"ip addr flush {i}");
            }

            public static string ShowInterfaceAddr(string interfaceName) {
                return Terminal.Execute($"ip addr show {interfaceName}");
            }

            public static string ShowInterfaceLink(string interfaceName) {
                return Terminal.Execute($"ip link show {interfaceName}");
            }

            public static string ShowInterfaceStats(string interfaceName) {
                return Terminal.Execute($"ip -s link ls {interfaceName}");
            }

            public static string AddRouteIPV4(string address, string gateway = null) {
                if (gateway == null) {
                    return Terminal.Execute($"ip route add default via {address}");
                }
                else {
                    return Terminal.Execute($"ip route add {gateway} via {address}");
                }
            }

            public static string AddMultipathRoute(string net1, string net2) {
                return Terminal.Execute($"ip route add default scope global nexthop dev {net1} nexthop dev {net2}");
            }

            public static string AddNat(string address, string viaAddress) {
                return Terminal.Execute($"ip route add nat {address} via {viaAddress}");
            }

            public static string DeleteRouteIPV4(string address, string gateway = null) {
                if (gateway == null) {
                    return Terminal.Execute($"ip route del default via {address}");
                }
                else {
                    return Terminal.Execute($"ip route del {gateway} via {address}");
                }
            }

            public static string ShowRoutes(string interfaceName = "") {
                return Terminal.Execute($"ip route show {interfaceName}");
            }

            public static string EnableInterface(string interfaceName) {
                return Terminal.Execute($"ip link set {interfaceName} up");
            }

            public static string DisableInterface(string interfaceName) {
                return Terminal.Execute($"ip link set {interfaceName} down");
            }

            public static string AddTunnelPointToPointIPV4(string interfaceName, string ttl, string foreignTunnel, string address) {
                return Terminal.Execute($"ip tunnel add {interfaceName} mode sit ttl {ttl} remote {foreignTunnel} local {address}");
            }

            public static string DeleteTunnelPointToPointIPV4(string interfaceName) {
                return Terminal.Execute($"ip tunnel del {interfaceName}");
            }

            public static string ShowTunnelsIPV4(string interfaceName) {
                var i = (interfaceName == null) ? "" : $"dev {interfaceName}";
                return Terminal.Execute($"ip tunnel show {i}");
            }

            #region IPV6 Related
            public static string AddNewAddressIPV6(string address, string interfaceName) {
                return Terminal.Execute($"ip -6 addr add {address} dev {interfaceName}");
            }

            public static string DeleteAddressIPV6(string address, string interfaceName) {
                return Terminal.Execute($"ip -6 addr del {address} dev {interfaceName}");
            }

            public static string ShowRoutesIPV6(string interfaceName = "") {
                return Terminal.Execute($"ip -6 route show {interfaceName}");
            }

            public static string FlushConfigurationIPV6() {
                return Terminal.Execute($"ip addr flush dynamic");
            }

            public static string ShowNeighborsIPV6(string interfaceName = null) {
                var i = (interfaceName == null) ? "" : $"dev {interfaceName}";
                return Terminal.Execute($"ip -6 neigh show {i}");
            }

            public static string AddNeighborsIPV6(string address, string layerAddress, string interfaceName) {
                return Terminal.Execute($"ip -6 neigh add {address} lladdr {layerAddress} dev {interfaceName}");
            }

            public static string DeleteNeighborsIPV6(string address, string layerAddress, string interfaceName) {
                return Terminal.Execute($"ip -6 neigh del {address} lladdr {layerAddress} dev {interfaceName}");
            }

            public static string AddRouteIPV6Gateway(string address, string gateway = null) {
                if (gateway == null) {
                    return Terminal.Execute($"ip -6 route add default via {address}");
                }
                else {
                    return Terminal.Execute($"ip -6 route add {gateway} via {address}");
                }
            }

            public static string DeleteRouteIPV6Gateway(string address, string gateway = null) {
                if (gateway == null) {
                    return Terminal.Execute($"ip -6 route del default via {address}");
                }
                else {
                    return Terminal.Execute($"ip -6 route del {gateway} via {address}");
                }
            }

            public static string AddRouteIPV6Interface(string interfaceName, string gateway = null) {
                if (gateway == null) {
                    return Terminal.Execute($"ip -6 route add default dev {interfaceName}");
                }
                else {
                    return Terminal.Execute($"ip -6 route add {gateway} dev {interfaceName}");
                }
            }

            public static string DeleteRouteIPV6Interface(string interfaceName, string gateway = null) {
                if (gateway == null) {
                    return Terminal.Execute($"ip -6 route del default dev {interfaceName}");
                }
                else {
                    return Terminal.Execute($"ip -6 route del {gateway} dev {interfaceName}");
                }
            }

            public static string ShowTunnelsIPV6(string interfaceName) {
                var i = (interfaceName == null) ? "" : $"dev {interfaceName}";
                return Terminal.Execute($"ip -6 tunnel show {i}");
            }
            #endregion
        }

        public class Brctl {
            public static string AddBridgeName(string bridgeName) {
                return Terminal.Execute($"brctl addbr {bridgeName}");
            }

            public static string DeleteBridgeName(string bridgeName) {
                return Terminal.Execute($"brctl delbr {bridgeName}");
            }

            public static string AddNetworkInterfaceToBridge(string bridgeName, string interfaceName) {
                return Terminal.Execute($"brctl addif {bridgeName} {interfaceName}");
            }

            public static string DeleteNetworkInterfaceToBridge(string bridgeName, string interfaceName) {
                return Terminal.Execute($"brctl delif {bridgeName} {interfaceName}");
            }

            public static string EnableStpOnBridge(string bridgeName) {
                return Terminal.Execute($"brctl stp {bridgeName} on");
            }

            public static string DisableStpOnBridge(string bridgeName) {
                return Terminal.Execute($"brctl stp {bridgeName} off");
            }

            public static string ShowBridgeMACS(string bridgeName) {
                return Terminal.Execute($"brctl showmacs {bridgeName}");
            }

            public static string ShowBridgeSTP(string bridgeName) {
                return Terminal.Execute($"brctl showstp {bridgeName}");
            }

            public static string SetBridgePathCost(string bridgeName, string path, string cost) {
                return Terminal.Execute($"brctl setpathcost {bridgeName} {path} {cost} set path cost");
            }

            public static string SetBridgePortPriority(string bridgeName, string port, string priority) {
                return Terminal.Execute($"brctl setportprio {bridgeName} {port} {priority} set port priority");
            }
        }
    }
}