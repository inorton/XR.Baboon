
// This file has been generated by the GUI designer. Do not modify.
namespace XR.Baboon
{
	public partial class FindSourceFileDialog
	{
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.Label label1;
		
		private global::Gtk.Label assemblyName;
		
		private global::Gtk.Label missingFilePath;
		
		private global::Gtk.FileChooserWidget fileChooser;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget XR.Baboon.FindSourceFileDialog
			this.Name = "XR.Baboon.FindSourceFileDialog";
			this.Title = global::Mono.Unix.Catalog.GetString ("Select File Location");
			this.Icon = global::Stetic.IconLoader.LoadIcon (this, "gtk-directory", global::Gtk.IconSize.Menu);
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child XR.Baboon.FindSourceFileDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xpad = 3;
			this.label1.Ypad = 3;
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Locate missing sources");
			this.label1.UseMarkup = true;
			this.vbox2.Add (this.label1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.label1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.assemblyName = new global::Gtk.Label ();
			this.assemblyName.Name = "assemblyName";
			this.assemblyName.Xpad = 3;
			this.assemblyName.Ypad = 3;
			this.assemblyName.Xalign = 0F;
			this.assemblyName.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Foo.Bar.Whatever Version=0.0.0</b>");
			this.assemblyName.UseMarkup = true;
			this.vbox2.Add (this.assemblyName);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.assemblyName]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.missingFilePath = new global::Gtk.Label ();
			this.missingFilePath.Name = "missingFilePath";
			this.missingFilePath.Xpad = 3;
			this.missingFilePath.Ypad = 3;
			this.missingFilePath.Xalign = 0F;
			this.missingFilePath.LabelProp = global::Mono.Unix.Catalog.GetString ("this/is/a/path/to/a/missing/source/file.cs");
			this.missingFilePath.Ellipsize = ((global::Pango.EllipsizeMode)(1));
			this.vbox2.Add (this.missingFilePath);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.missingFilePath]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.fileChooser = new global::Gtk.FileChooserWidget (((global::Gtk.FileChooserAction)(2)));
			this.fileChooser.Name = "fileChooser";
			this.vbox2.Add (this.fileChooser);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.fileChooser]));
			w5.Position = 3;
			w1.Add (this.vbox2);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(w1 [this.vbox2]));
			w6.Position = 0;
			// Internal child XR.Baboon.FindSourceFileDialog.ActionArea
			global::Gtk.HButtonBox w7 = this.ActionArea;
			w7.Name = "dialog1_ActionArea";
			w7.Spacing = 10;
			w7.BorderWidth = ((uint)(5));
			w7.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w8 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w7 [this.buttonCancel]));
			w8.Expand = false;
			w8.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w9 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w7 [this.buttonOk]));
			w9.Position = 1;
			w9.Expand = false;
			w9.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 630;
			this.DefaultHeight = 466;
			this.Show ();
		}
	}
}
