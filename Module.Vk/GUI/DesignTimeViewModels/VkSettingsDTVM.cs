﻿using System;
using System.Windows.Input;
using Module.Vk.GUI.AbstractViewModels;
using Root.MVVM;

namespace Module.Vk.GUI.DesignTimeViewModels
{
    public class VkSettingsDTVM : IVkSettingsVM
    {
        public bool Loaded => false;
        public string LoadingErrorMessage => "Some unknown error(((";

        public ICommand ReloadCmd { get; } = RelayCommand.Empty;

        public string AccessToken { get; set; } = "{private access token}";
        public string UserId { get; set; } = "1238769901";

        public void Load()
        {
            throw new NotSupportedException();
        }

        public bool Save()
        {
            throw new NotSupportedException();
        }
    }
}