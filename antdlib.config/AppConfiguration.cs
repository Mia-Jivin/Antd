﻿using System;
using System.IO;
using antdlib.common;
using antdlib.models;
using Newtonsoft.Json;

namespace antdlib.config {
    public class AppConfiguration {

        private AppConfigurationModel _model;

        private readonly string _file = $"{Parameter.AntdCfg}/app.conf";

        private readonly ApiConsumer _api = new ApiConsumer();

        public AppConfiguration() {
            if(!File.Exists(_file)) {
                _model = new AppConfigurationModel();
            }
            else {
                try {
                    var text = File.ReadAllText(_file);
                    var obj = JsonConvert.DeserializeObject<AppConfigurationModel>(text);
                    _model = obj;
                }
                catch(Exception) {
                    _model = new AppConfigurationModel();
                }

            }
        }

        public AppConfigurationModel Get() {
            return _model;
        }

        public void Save(AppConfigurationModel model) {
            _model = model;
            if(File.Exists(_file)) {
                File.Copy(_file, $"{_file}.bck", true);
            }
            File.WriteAllText(_file, JsonConvert.SerializeObject(_model, Formatting.Indented));
        }

        public void UiSave(AppConfigurationModel model) {
            var savedModel = Get();
            _model = model;
            _api.Post($"http://localhost:{savedModel.AntdPort}/config", _model.ToDictionary());
        }
    }
}