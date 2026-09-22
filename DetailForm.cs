using System.Windows.Forms;

public class DetailForm : Form
{
    public DetailForm(string content)
    {
        Text = "Clipboard Item Detail";
        Width = 500; Height = 400;
        var box = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Text = content
        };
        Controls.Add(box);
    }
}