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
using Antd.Network;
using Nancy;
using Nancy.Security;

namespace Antd.Modules {
    public class ServiceNetworkModule : CoreModule {

        public ServiceNetworkModule() {
            this.RequiresAuthentication();

            Post["/services/network/restart"] = x => {
                var networkConfiguration = new NetworkConfiguration();
                networkConfiguration.Start();
                return HttpStatusCode.OK;
            };

            Post["/services/network/interface"] = x => {
                string Interface = Request.Form.Interface;
                string mode = Request.Form.Mode;
                string status = Request.Form.Status;
                string staticAddres = Request.Form.StaticAddres;
                string staticRange = Request.Form.StaticRange;
                string txqueuelen = Request.Form.Txqueuelen;
                string mtu = Request.Form.Mtu;
                var model = new NetworkInterfaceConfigurationModel {
                    Interface = Interface,
                    Mode = (NetworkInterfaceMode)Enum.Parse(typeof(NetworkInterfaceMode), mode),
                    Status = (NetworkInterfaceStatus)Enum.Parse(typeof(NetworkInterfaceStatus), status),
                    StaticAddress = staticAddres,
                    StaticRange = staticRange,
                    Txqueuelen = txqueuelen,
                    Mtu = mtu
                };
                var networkConfiguration = new NetworkConfiguration();
                networkConfiguration.AddInterfaceSetting(model);
                return Response.AsRedirect("/");
            };

            Post["/services/network/interface/del"] = x => {
                string guid = Request.Form.Guid;
                var networkConfiguration = new NetworkConfiguration();
                networkConfiguration.RemoveInterfaceSetting(guid);
                return HttpStatusCode.OK;
            };
        }
    }
}