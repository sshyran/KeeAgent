﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KeePass.Forms;

namespace KeeAgent.UI
{
  [DefaultBindingProperty("KeyLocation")]
  public partial class KeyLocationPanel : UserControl
  {

    private PwEntryForm mPwEntryForm;

    public event EventHandler KeyLocationChanged;

    [Category("Data")]
    public EntrySettings.LocationData KeyLocation
    {
      get
      {
        return locationSettingsBindingSource.DataSource as EntrySettings.LocationData;
      }
      set
      {
        if (DesignMode) { return; }
        if (object.ReferenceEquals(locationSettingsBindingSource.DataSource, value)) {
          return;
        }
        if (value == null) {
          locationSettingsBindingSource.DataSource = typeof(EntrySettings.LocationData);
        } else {
          locationSettingsBindingSource.DataSource = value;
        }
        OnKeyLocationChanged();
      }
    }

    public KeyLocationPanel()
    {
      InitializeComponent();

      // make transparent so tab styling shows
      SetStyle(ControlStyles.SupportsTransparentBackColor, true);
      BackColor = Color.Transparent;

      locationGroupBox.DataBindings["SelectedRadioButton"].Format +=
        delegate(object aSender, ConvertEventArgs aEventArgs)
        {
          if (aEventArgs.DesiredType == typeof(string)) {
            var type = aEventArgs.Value as EntrySettings.LocationType?;
            switch (type) {
              case EntrySettings.LocationType.Attachment:
                aEventArgs.Value = attachmentRadioButton.Name;
                break;
              case EntrySettings.LocationType.File:
                aEventArgs.Value = fileRadioButton.Name;
                break;
              default:
                aEventArgs.Value = string.Empty;
                break;
            }
          } else {
            Debug.Fail("unexpected");
          }
        };
      locationGroupBox.DataBindings["SelectedRadioButton"].Parse +=
        delegate(object aSender, ConvertEventArgs aEventArgs)
        {
          if (aEventArgs.DesiredType == typeof(EntrySettings.LocationType?) &&
            aEventArgs.Value is string) {
            var valueString = aEventArgs.Value as string;
            if (valueString == attachmentRadioButton.Name) {
              aEventArgs.Value = EntrySettings.LocationType.Attachment;
            } else if (valueString == fileRadioButton.Name) {
              aEventArgs.Value = EntrySettings.LocationType.File;
            } else {
              aEventArgs.Value = null;
            }
          } else {
            Debug.Fail("unexpected");
          }
        };
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      if (DesignMode) { return; }
      mPwEntryForm = ParentForm as PwEntryForm;
      UpdateControlStates();
    }


    private void UpdateControlStates()
    {
      attachmentComboBox.Enabled = attachmentRadioButton.Checked;
      fileNameTextBox.Enabled = fileRadioButton.Checked;
      browseButton.Enabled = fileRadioButton.Checked;
    }

    private void radioButton_CheckedChanged(object sender, EventArgs e)
    {
      UpdateControlStates();
    }

    private void attachmentComboBox_VisibleChanged(object sender, EventArgs e)
    {
      if (DesignMode) { return; }
      if (attachmentComboBox.Visible) {
        attachmentComboBox.Items.Clear();
        if (mPwEntryForm != null) {
          foreach (var binary in mPwEntryForm.EntryBinaries) {
            attachmentComboBox.Items.Add(binary.Key);
          }
        } else {
          Debug.Fail("Don't have binaries");
        }
      }
    }

    private void browseButton_Click(object sender, EventArgs e)
    {
      try {
        openFileDialog.InitialDirectory =
          Path.GetDirectoryName(fileNameTextBox.Text);
      } catch (Exception) { }
      var result = openFileDialog.ShowDialog();
      if (result == DialogResult.OK) {
        fileNameTextBox.Text = openFileDialog.FileName;
        fileNameTextBox.Focus();
      }
    }

    private void locationSettingsBindingSource_BindingComplete(object sender,
      BindingCompleteEventArgs e)
    {
      if (e.BindingCompleteContext == BindingCompleteContext.DataSourceUpdate) {
        e.Binding.BindingManagerBase.EndCurrentEdit();
        OnKeyLocationChanged();
      }
    }

    private void OnKeyLocationChanged()
    {
      if (KeyLocationChanged != null) {
        KeyLocationChanged(this, new EventArgs());
      }
    }
  }
}