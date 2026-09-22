using System;
using System.Windows.Forms;

public class MainForm : Form
{
    private DataGridView grid = new();
    private TextBox searchBox = new(), deleteBtn = new(), pinBtn = new(), clearBtn = new(), settingsBtn = new();
    private System.Windows.Forms.Timer clipTimer = new();
    private String lastClip = "";

    public MainForm()
    {
        Text = "Smart Clipboard Manager";
        Width = 700; Height = 500;

        searchBox.Location = new System.Drawing.Point(10, 10);
        searchBox.Width = 300;
        searchBox.PlaceholderText = "Search...";
        searchBox.TextChanged += (s, e) => RefreshGrid(searchBox.Text);

        grid.Location = new System.Drawing.Point(10, 40);
        grid.Size = new System.Drawing.Size(660, 350);
        grid.ReadOnly = true;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.DoubleClick += Grid_DoubleClick;

        copyBtn.Text = "Copy"; copyBtn.Location = new(10, 400); copyBtn.Click += (s, e) => CopySelected();
        pinBtn.Text = "Pin/Unpin"; pinBtn.Location = new(100, 400); pinBtn.Click += (s, e) => TogglePinSelected();
        deleteBtn.Text = "Delete"; deleteBtn.Location = new(210, 400); deleteBtn.Click += (s, e) => DeleteSelected();
        clearBtn.Text = "Clear All"; clearBtn.Location = new(300, 400); clearBtn.Click += (s, e) => { DbHelper.ClearAll(); RefreshGrid(""); };
        settingsBtn.Text = "Settings"; settingsBtn.Location = new(410, 400); settingsBtn.Click += (s, e) => new SettingsForm().ShowDialog();


        Controls.AddRange(new Control[] { searchBox, grid, copyBtn, pinBtn, deleteBtn, clearBtn, settingsBtn });

        DbHelper.Init();
        RefreshGrid("");

        clipTimer.Interval = 800;
        clipTimer.Tick += ClipTimer_Tick;
        clipTimer.Start();
    }

    private void ClipTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (Clipboard.ContainsText())
            {
                string current = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(current) && current != lastClip)
                {
                    lastClip = current;
                    DbHelper.InsertItem(current, "Text");

                    int maxEntries = int.TryParse(DbHelper.GetSetting("MaxEntries", "100"), out int v) ? v : 100;
                    DbHelper.TrimToLimit(maxEntries);

                    RefreshGrid(searchBox.Text);
                }
            }
        }
        catch { }
    }

    private void RefreshGrid(string query)
    {
        var items = string.IsNullOrWhiteSpace(query) ? DbHelper.GetAll() : DbHelper.Search(query);
        grid.DataSource = null;
        grid.DataSource = items;
        if (grid.Columns["Content"] != null)
            grid.Columns["Content"].Width = 400;
    }

    private ClipboardItem? GetSelected()
    {
        if (grid.CurrentRow?.DataBoundItem is ClipboardItem item) return item;
        return null;
    }

    private void CopySelected()
    {
        var item = GetSelected();
        if (item != null) Clipboard.SetText(item.Content);
    }

    private void DeleteSelected()
    {
        var item = GetSelected();
        if (item != null) { DbHelper.Delete(item.Id); RefreshGrid(searchBox.Text); }
    }

    private void TogglePinSelected()
    {
        var item = GetSelected();
        if (item != null) { DbHelper.TogglePin(item.Id, !item.IsPinned); RefreshGrid(searchBox.Text); }
    }

    private void Grid_DoubleClick(object? sender, EventArgs e)
    {
        var item = GetSelected();
        if (item != null) new DetailForm(item.Content).ShowDialog();
    }
}