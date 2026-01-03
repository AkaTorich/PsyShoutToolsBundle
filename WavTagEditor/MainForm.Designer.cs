namespace WavTagEditor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lblArtist = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAlbum = new System.Windows.Forms.Label();
            this.lblYear = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblComment = new System.Windows.Forms.Label();
            this.txtArtist = new System.Windows.Forms.TextBox();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtAlbum = new System.Windows.Forms.TextBox();
            this.txtYear = new System.Windows.Forms.TextBox();
            this.txtGenre = new System.Windows.Forms.TextBox();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnUpdateTags = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.columnFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnArtistCurrent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnTitleCurrent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAlbumCurrent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnYearCurrent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnGenreCurrent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnCommentCurrent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lblArtist
            // 
            this.lblArtist.AutoSize = true;
            this.lblArtist.Location = new System.Drawing.Point(12, 15);
            this.lblArtist.Name = "lblArtist";
            this.lblArtist.Size = new System.Drawing.Size(77, 13);
            this.lblArtist.TabIndex = 0;
            this.lblArtist.Text = "Исполнитель:";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(12, 41);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(92, 13);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Название трека:";
            // 
            // lblAlbum
            // 
            this.lblAlbum.AutoSize = true;
            this.lblAlbum.Location = new System.Drawing.Point(12, 67);
            this.lblAlbum.Name = "lblAlbum";
            this.lblAlbum.Size = new System.Drawing.Size(49, 13);
            this.lblAlbum.TabIndex = 2;
            this.lblAlbum.Text = "Альбом:";
            // 
            // lblYear
            // 
            this.lblYear.AutoSize = true;
            this.lblYear.Location = new System.Drawing.Point(12, 93);
            this.lblYear.Name = "lblYear";
            this.lblYear.Size = new System.Drawing.Size(28, 13);
            this.lblYear.TabIndex = 3;
            this.lblYear.Text = "Год:";
            // 
            // lblGenre
            // 
            this.lblGenre.AutoSize = true;
            this.lblGenre.Location = new System.Drawing.Point(12, 119);
            this.lblGenre.Name = "lblGenre";
            this.lblGenre.Size = new System.Drawing.Size(39, 13);
            this.lblGenre.TabIndex = 4;
            this.lblGenre.Text = "Жанр:";
            // 
            // lblComment
            // 
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(12, 145);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(75, 13);
            this.lblComment.TabIndex = 5;
            this.lblComment.Text = "Комментарий:";
            // 
            // txtArtist
            // 
            this.txtArtist.Location = new System.Drawing.Point(111, 12);
            this.txtArtist.Name = "txtArtist";
            this.txtArtist.Size = new System.Drawing.Size(578, 20);
            this.txtArtist.TabIndex = 6;
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(111, 38);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(578, 20);
            this.txtTitle.TabIndex = 7;
            // 
            // txtAlbum
            // 
            this.txtAlbum.Location = new System.Drawing.Point(111, 64);
            this.txtAlbum.Name = "txtAlbum";
            this.txtAlbum.Size = new System.Drawing.Size(578, 20);
            this.txtAlbum.TabIndex = 8;
            // 
            // txtYear
            // 
            this.txtYear.Location = new System.Drawing.Point(111, 90);
            this.txtYear.Name = "txtYear";
            this.txtYear.Size = new System.Drawing.Size(578, 20);
            this.txtYear.TabIndex = 9;
            // 
            // txtGenre
            // 
            this.txtGenre.Location = new System.Drawing.Point(111, 116);
            this.txtGenre.Name = "txtGenre";
            this.txtGenre.Size = new System.Drawing.Size(578, 20);
            this.txtGenre.TabIndex = 10;
            // 
            // txtComment
            // 
            this.txtComment.Location = new System.Drawing.Point(111, 142);
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(578, 20);
            this.txtComment.TabIndex = 11;
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(15, 445);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(127, 30);
            this.btnSelectFolder.TabIndex = 12;
            this.btnSelectFolder.Text = "Выбрать папку";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.BtnSelectFolder_Click);
            // 
            // btnUpdateTags
            // 
            this.btnUpdateTags.Enabled = false;
            this.btnUpdateTags.Location = new System.Drawing.Point(148, 445);
            this.btnUpdateTags.Name = "btnUpdateTags";
            this.btnUpdateTags.Size = new System.Drawing.Size(127, 30);
            this.btnUpdateTags.TabIndex = 13;
            this.btnUpdateTags.Text = "Обновить теги";
            this.btnUpdateTags.UseVisualStyleBackColor = true;
            this.btnUpdateTags.Click += new System.EventHandler(this.BtnUpdateTags_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(281, 454);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(83, 13);
            this.lblStatus.TabIndex = 14;
            this.lblStatus.Text = "Готов к работе";
            // 
            // lstFiles
            // 
            this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFilename,
            this.columnSize,
            this.columnArtistCurrent,
            this.columnTitleCurrent,
            this.columnAlbumCurrent,
            this.columnYearCurrent,
            this.columnGenreCurrent,
            this.columnCommentCurrent});
            this.lstFiles.FullRowSelect = true;
            this.lstFiles.GridLines = true;
            this.lstFiles.HideSelection = false;
            this.lstFiles.Location = new System.Drawing.Point(15, 177);
            this.lstFiles.MultiSelect = false;
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(774, 262);
            this.lstFiles.TabIndex = 15;
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.SelectedIndexChanged += new System.EventHandler(this.LstFiles_SelectedIndexChanged);
            // 
            // columnFilename
            // 
            this.columnFilename.Text = "Имя файла";
            this.columnFilename.Width = 150;
            // 
            // columnSize
            // 
            this.columnSize.Text = "Размер";
            this.columnSize.Width = 70;
            // 
            // columnArtistCurrent
            // 
            this.columnArtistCurrent.Text = "Исполнитель";
            this.columnArtistCurrent.Width = 100;
            // 
            // columnTitleCurrent
            // 
            this.columnTitleCurrent.Text = "Название";
            this.columnTitleCurrent.Width = 100;
            // 
            // columnAlbumCurrent
            // 
            this.columnAlbumCurrent.Text = "Альбом";
            this.columnAlbumCurrent.Width = 100;
            // 
            // columnYearCurrent
            // 
            this.columnYearCurrent.Text = "Год";
            this.columnYearCurrent.Width = 50;
            // 
            // columnGenreCurrent
            // 
            this.columnGenreCurrent.Text = "Жанр";
            this.columnGenreCurrent.Width = 100;
            // 
            // columnCommentCurrent
            // 
            this.columnCommentCurrent.Text = "Комментарий";
            this.columnCommentCurrent.Width = 100;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 486);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnUpdateTags);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.txtGenre);
            this.Controls.Add(this.txtYear);
            this.Controls.Add(this.txtAlbum);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.txtArtist);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.lblGenre);
            this.Controls.Add(this.lblYear);
            this.Controls.Add(this.lblAlbum);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblArtist);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WAV Tag Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblArtist;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAlbum;
        private System.Windows.Forms.Label lblYear;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtArtist;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtAlbum;
        private System.Windows.Forms.TextBox txtYear;
        private System.Windows.Forms.TextBox txtGenre;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.Button btnUpdateTags;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListView lstFiles;
        private System.Windows.Forms.ColumnHeader columnFilename;
        private System.Windows.Forms.ColumnHeader columnSize;
        private System.Windows.Forms.ColumnHeader columnArtistCurrent;
        private System.Windows.Forms.ColumnHeader columnTitleCurrent;
        private System.Windows.Forms.ColumnHeader columnAlbumCurrent;
        private System.Windows.Forms.ColumnHeader columnYearCurrent;
        private System.Windows.Forms.ColumnHeader columnGenreCurrent;
        private System.Windows.Forms.ColumnHeader columnCommentCurrent;
    }
}