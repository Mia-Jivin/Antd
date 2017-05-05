﻿using antdlib.common;
using antdlib.config;
using antdlib.models;
using anthilla.commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Antd.Timer {
    public class RemoveUnusedModules {
        public System.Threading.Timer Timer { get; private set; }

        public void Start(TimeSpan alertTime) {
            Timer = new System.Threading.Timer(x => {
                Action();
            }, null, alertTime, Timeout.InfiniteTimeSpan);
        }

        public void Stop() {
            Timer?.Dispose();
        }

        private static void Action() {
            new Do().ParametersChangesPost();
        }
    }
}
