using System;
using System.Windows.Forms;

public class SettingsForm : Form
{
    private NumericUpDown maxEntriesInput = new();
    private Label maxEntriesLabel = new();
    private Button saveBtn = new(), cancelBtn = new();

    public SettingsForm()
    {
        Text = "Settings";
        Width = 320;
        Height = 180;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        maxEntriesLabel.Text = "Max entries to keep:";
        maxEntriesLabel.AutoSize = true;
        maxEntriesLabel.Location = new(20, 25);

        maxEntriesInput.Minimum = 10;
        maxEntriesInput.Maximum = 5000;
        maxEntriesInput.Location = new(180, 20);
        maxEntriesInput.Width = 100;

        // Load current value from DB (default 100)
        int currentMax = int.TryParse(DbHelper.GetSetting("MaxEntries", "100"), out int v) ? v : 100;
        maxEntriesInput.Value = Math.Clamp(currentMax, (int)maxEntriesInput.Minimum, (int)maxEntriesInput.Maximum);

        saveBtn.Text = "Save";
        saveBtn.Location = new(100, 80);
        saveBtn.Click += SaveBtn_Click;

        cancelBtn.Text = "Cancel";
        cancelBtn.Location = new(190, 80);
        cancelBtn.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { maxEntriesLabel, maxEntriesInput, saveBtn, cancelBtn });
    }

    private void SaveBtn_Click(object? sender, EventArgs e)
    {
        int newMax = (int)maxEntriesInput.Value;
        DbHelper.SetSetting("MaxEntries", newMax.ToString());
        DbHelper.TrimToLimit(newMax); // immediately enforce the new limit
        MessageBox.Show("Settings saved.", "Smart Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}