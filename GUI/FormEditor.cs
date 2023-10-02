using Editor.Editor;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Editor
{
	public partial class FormEditor : Form
	{
		public GameEditor Game { get; set; }
		public FormEditor()
		{
			InitializeComponent();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Game.Exit();
		}

		private void FormEditor_SizeChanged(object sender, EventArgs e)
		{
			if (Game == null) return;
			Game.AdjustAspectRatio();
			// Game?.AdjustAspectRatio(); Checks if object is null
		}

		private void splitContainer_Panel1_SizeChanged(object sender, EventArgs e)
		{
			if (Game == null) return;
			Game.AdjustAspectRatio();
		}

		private void createToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new();
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				Game.Project = new(Game.Content, sfd.FileName);
				Text = "Our Cool Editor - " + Game.Project.Name;
				Game.AdjustAspectRatio();
			}
			saveToolStripMenuItem_Click(sender, e);
		}

		private void FormEditor_Load(object sender, EventArgs e)
		{

		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string fname = Path.Combine(Game.Project.Folder, Game.Project.Name);
			using var stream = File.Open(fname, FileMode.Create);
			using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
			Game.Project.Serialize(writer);
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new();
			ofd.Filter = "OCE Files|*.oce";
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				using var stream = File.Open(ofd.FileName, FileMode.Open);
				using var reader = new BinaryReader(stream, Encoding.UTF8, false);
				Game.Project = new();
				Game.Project.Deserialize(reader, Game.Content);
				Text = "Our Cool Editor - " + Game.Project.Name;
				Game.AdjustAspectRatio();
			}
		}
	}
}
