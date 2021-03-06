﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RailSharp;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using SoundSwitch.UI.UserControls;
using SoundSwitch.UI.UserControls.HotKeyControl;

namespace SoundSwitch.UI.Forms
{
    public partial class AddProfile : Form
    {
        private readonly SettingsForm   _settingsForm;
        private readonly ProfileSetting _profile;


        public AddProfile(IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings, SettingsForm settingsForm)
        {
            InitializeComponent();
            _settingsForm = settingsForm;
            Icon          = Resources.profile;

            _profile = new ProfileSetting();

            LocalizeForm();
            InitRecordingPlaybackComboBoxes(playbacks, recordings);
            InitBindings();
        }

        private void LocalizeForm()
        {
            Text                       = SettingsStrings.profile_feature_add;
            selectProgramDialog.Filter = $@"{SettingsStrings.profile_feature_executable}|*.exe";

            nameLabel.Text          = SettingsStrings.profile_name;
            programLabel.Text       = SettingsStrings.profile_program;
            hotKeyLabel.Text        = SettingsStrings.hotkeys;
            recordingLabel.Text     = SettingsStrings.recording;
            playbackLabel.Text      = SettingsStrings.playback;
            createButton.Text       = SettingsStrings.profile_addButton;
            addProfileGroupBox.Text = SettingsStrings.profile_feature_profile;
        }

        private void InitBindings()
        {
            nameTextBox.DataBindings.Add(nameof(TextBox.Text), _profile, nameof(ProfileSetting.ProfileName), false, DataSourceUpdateMode.OnPropertyChanged);
            programTextBox.DataBindings.Add(nameof(TextBox.Text), _profile, nameof(ProfileSetting.ApplicationPath), false, DataSourceUpdateMode.OnPropertyChanged);
            //Can't figure out why it stopped working.
            //Using event instead for now.
            //hotKeyTextBox.DataBindings.Add(nameof(HotKeyTextBox.HotKey), _profile, nameof(ProfileSetting.HotKey), true, DataSourceUpdateMode.OnPropertyChanged);


            recordingComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(ProfileSetting.Recording), false, DataSourceUpdateMode.OnPropertyChanged);
            playbackComboBox.DataBindings.Add(nameof(ComboBox.SelectedValue), _profile, nameof(ProfileSetting.Playback), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void InitRecordingPlaybackComboBoxes(IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings)
        {
            recordingComboBox.DataSource =
                recordings
                    .OrderBy(info => info.State)
                    .ThenBy(info => info.Name)
                    .Select(info => new IconTextComboBox.DropDownItem
                        {
                            Icon = info.SmallIcon,
                            Tag  = info,
                            Text = info.Name
                        }
                    ).ToArray();


            playbackComboBox.DataSource =
                playbacks
                    .OrderBy(info => info.State)
                    .ThenBy(info => info.Name)
                    .Select(info => new IconTextComboBox.DropDownItem
                        {
                            Icon = info.SmallIcon,
                            Tag  = info,
                            Text = info.Name
                        }
                    ).ToArray();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void selectProgramButton_Click(object sender, EventArgs e)
        {
            if (selectProgramDialog.ShowDialog(this) != DialogResult.OK)
                return;
            programTextBox.Text = selectProgramDialog.FileName;
            if (string.IsNullOrEmpty(nameTextBox.Text))
            {
                var executableName = selectProgramDialog.FileName?.Split('\\').Last();
                nameTextBox.Text = executableName?.Substring(0, executableName.Length - 4) ?? "";
            }
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            var result = AppModel.Instance.ProfileManager.AddProfile(_profile);
            result.Map(success =>
                  {
                      _settingsForm.RefreshProfiles();
                      _settingsForm.Focus();
                      Close();
                      return success;
                  })
                  .Catch<string>(s =>
                  {
                      MessageBox.Show(s, SettingsStrings.profile_error_title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                      return Result.Success();
                  });
        }

        private void playbackRemoveButton_Click(object sender, EventArgs e)
        {
            _profile.Playback = null;
            try
            {
                playbackComboBox.SelectedIndex = -1;
            }
            catch (ArgumentException)
            {
                //Happens because I receive a System.DBNull when there isn't a selection.
            }

            playbackRemoveButton.Visible = false;
        }

        private void recordingRemoveButton_Click(object sender, EventArgs e)
        {
            _profile.Recording = null;
            try
            {
                recordingComboBox.SelectedIndex = -1;
            }
            catch (ArgumentException)
            {
                //Happens because I receive a System.DBNull when there isn't a selection.
            }

            recordingRemoveButton.Visible = false;
        }

        private void playbackComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (playbackComboBox.SelectedIndex == -1)
            {
                playbackRemoveButton.Visible = false;
                return;
            }

            playbackRemoveButton.Visible = true;
        }

        private void recordingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (recordingComboBox.SelectedIndex == -1)
            {
                recordingRemoveButton.Visible = false;
                return;
            }

            recordingRemoveButton.Visible = true;
        }

        private void hotKeyControl_HotKeyIsSet(object sender, HotKeyIsSetEventArgs e)
        {
            _profile.HotKey = hotKeyControl.HotKey;
        }
    }
}