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

using System.Collections.Generic;
using antd.commands;
using antdlib.common;
using Antd.Storage;
using Nancy;

namespace Antd.Modules {
    public class TestModule : CoreModule {

        public class TestClass {
            public string Text { get; set; }
        }

        public TestModule() {

            Get["Test page", "/test"] = x => Response.AsText("Hello World!");

            Get["/test/page"] = x => View["page-test"];

            Get["/test/page2"] = x => View["page-test-2"];

            Get["/test/vnc"] = x => View["page-vnc"];

            Get["/test/hash/{str}"] = x => {
                string s = x.str;
                return Response.AsText(Encryption.XHash(s));
            };

            Get["/test/command1/{val}"] = x => {
                string val = x.val;
                if(string.IsNullOrEmpty(val)) {
                    return HttpStatusCode.BadRequest;
                }
                var launcher = new CommandLauncher();
                var result = launcher.Launch("test-sub-string", new Dictionary<string, string> { { "$obj", val } });
                return Response.AsJson(result);
            };

            Get["/test/command2/{val}"] = x => {
                string val = x.val;
                if(string.IsNullOrEmpty(val)) {
                    return HttpStatusCode.BadRequest;
                }
                var launcher = new CommandLauncher();
                var result = launcher.Launch("test-sub-list", new Dictionary<string, string> { { "$obj", val }, { "$value", val + "2" } });
                return Response.AsJson(result);
            };
        }
    }
}