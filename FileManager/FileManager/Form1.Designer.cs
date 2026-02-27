namespace FileManager
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listBoxFiles = new ListBox();
            groupBox1 = new GroupBox();
            buttonRename = new Button();
            buttonDelete = new Button();
            textBoxDestination = new TextBox();
            textBoxNewName = new TextBox();
            buttonRemove = new Button();
            buttonCopy = new Button();
            buttonProperties = new Button();
            groupBoxProperties = new GroupBox();
            labelPath = new Label();
            labelData = new Label();
            labelOwner = new Label();
            labelSize = new Label();
            labelName = new Label();
            folderBrowserDialog1 = new FolderBrowserDialog();
            buttonBrowse = new Button();
            comboBoxSortBy = new ComboBox();
            groupBox1.SuspendLayout();
            groupBoxProperties.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxFiles
            // 
            listBoxFiles.FormattingEnabled = true;
            listBoxFiles.Location = new Point(17, 63);
            listBoxFiles.Name = "listBoxFiles";
            listBoxFiles.Size = new Size(247, 529);
            listBoxFiles.TabIndex = 3;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(buttonRename);
            groupBox1.Controls.Add(buttonDelete);
            groupBox1.Controls.Add(textBoxDestination);
            groupBox1.Controls.Add(textBoxNewName);
            groupBox1.Controls.Add(buttonRemove);
            groupBox1.Controls.Add(buttonCopy);
            groupBox1.Controls.Add(buttonProperties);
            groupBox1.Location = new Point(279, 63);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(203, 316);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Работа с файлами";
            // 
            // buttonRename
            // 
            buttonRename.BackColor = SystemColors.ActiveCaption;
            buttonRename.Location = new Point(6, 71);
            buttonRename.Name = "buttonRename";
            buttonRename.Size = new Size(181, 34);
            buttonRename.TabIndex = 6;
            buttonRename.Text = "переименовать";
            buttonRename.UseVisualStyleBackColor = false;
            // 
            // buttonDelete
            // 
            buttonDelete.BackColor = SystemColors.ActiveCaption;
            buttonDelete.Location = new Point(6, 268);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(181, 34);
            buttonDelete.TabIndex = 5;
            buttonDelete.Text = "удалить";
            buttonDelete.UseVisualStyleBackColor = false;
            // 
            // textBoxDestination
            // 
            textBoxDestination.Location = new Point(6, 231);
            textBoxDestination.Name = "textBoxDestination";
            textBoxDestination.Size = new Size(150, 31);
            textBoxDestination.TabIndex = 8;
            textBoxDestination.Text = "new folder";
            // 
            // textBoxNewName
            // 
            textBoxNewName.Location = new Point(6, 111);
            textBoxNewName.Name = "textBoxNewName";
            textBoxNewName.Size = new Size(150, 31);
            textBoxNewName.TabIndex = 7;
            textBoxNewName.Text = "new name";
            // 
            // buttonRemove
            // 
            buttonRemove.BackColor = SystemColors.ActiveCaption;
            buttonRemove.Location = new Point(6, 191);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(181, 34);
            buttonRemove.TabIndex = 4;
            buttonRemove.Text = "переместить";
            buttonRemove.UseVisualStyleBackColor = false;
            // 
            // buttonCopy
            // 
            buttonCopy.BackColor = SystemColors.ActiveCaption;
            buttonCopy.Location = new Point(6, 151);
            buttonCopy.Name = "buttonCopy";
            buttonCopy.Size = new Size(181, 34);
            buttonCopy.TabIndex = 2;
            buttonCopy.Text = "копировать";
            buttonCopy.UseVisualStyleBackColor = false;
            // 
            // buttonProperties
            // 
            buttonProperties.BackColor = SystemColors.ActiveCaption;
            buttonProperties.Location = new Point(6, 30);
            buttonProperties.Name = "buttonProperties";
            buttonProperties.Size = new Size(181, 34);
            buttonProperties.TabIndex = 0;
            buttonProperties.Text = "показать свойства";
            buttonProperties.UseVisualStyleBackColor = false;
            // 
            // groupBoxProperties
            // 
            groupBoxProperties.Controls.Add(labelPath);
            groupBoxProperties.Controls.Add(labelData);
            groupBoxProperties.Controls.Add(labelOwner);
            groupBoxProperties.Controls.Add(labelSize);
            groupBoxProperties.Controls.Add(labelName);
            groupBoxProperties.Location = new Point(279, 385);
            groupBoxProperties.Name = "groupBoxProperties";
            groupBoxProperties.Size = new Size(371, 207);
            groupBoxProperties.TabIndex = 5;
            groupBoxProperties.TabStop = false;
            groupBoxProperties.Text = "Свойства файла";
            // 
            // labelPath
            // 
            labelPath.AutoSize = true;
            labelPath.Location = new Point(13, 135);
            labelPath.Name = "labelPath";
            labelPath.Size = new Size(54, 25);
            labelPath.TabIndex = 4;
            labelPath.Text = "Путь:";
            // 
            // labelData
            // 
            labelData.AutoSize = true;
            labelData.Location = new Point(13, 160);
            labelData.Name = "labelData";
            labelData.Size = new Size(146, 25);
            labelData.TabIndex = 3;
            labelData.Text = "Дата изменения:";
            // 
            // labelOwner
            // 
            labelOwner.AutoSize = true;
            labelOwner.Location = new Point(13, 102);
            labelOwner.Name = "labelOwner";
            labelOwner.Size = new Size(92, 25);
            labelOwner.TabIndex = 2;
            labelOwner.Text = "Владелец:";
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new Point(13, 71);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(81, 25);
            labelSize.TabIndex = 1;
            labelSize.Text = "Размер: ";
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(13, 41);
            labelName.Name = "labelName";
            labelName.Size = new Size(56, 25);
            labelName.TabIndex = 0;
            labelName.Text = "Имя: ";
            // 
            // buttonBrowse
            // 
            buttonBrowse.Location = new Point(17, 14);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new Size(112, 34);
            buttonBrowse.TabIndex = 9;
            buttonBrowse.Text = "обзор ";
            buttonBrowse.UseVisualStyleBackColor = true;
            // 
            // comboBoxSortBy
            // 
            comboBoxSortBy.FormattingEnabled = true;
            comboBoxSortBy.Items.AddRange(new object[] { "По имени", "По размеру", "По типу", "По дате изменения" });
            comboBoxSortBy.Location = new Point(279, 16);
            comboBoxSortBy.Name = "comboBoxSortBy";
            comboBoxSortBy.Size = new Size(187, 33);
            comboBoxSortBy.TabIndex = 10;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 613);
            Controls.Add(comboBoxSortBy);
            Controls.Add(buttonBrowse);
            Controls.Add(groupBoxProperties);
            Controls.Add(groupBox1);
            Controls.Add(listBoxFiles);
            Name = "Form1";
            Text = "Form1";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBoxProperties.ResumeLayout(false);
            groupBoxProperties.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        //private Button buttonLoad;
        private ListBox listBoxFiles;
        private GroupBox groupBox1;
        private Button buttonProperties;
        private Button buttonCopy;
        private Button buttonDelete;
        private Button buttonRemove;
        private GroupBox groupBoxProperties;
        private Label labelOwner;
        private Label labelSize;
        private Label labelName;
        private Label labelData;
        private Button buttonRename;
        private TextBox textBoxNewName;
        private TextBox textBoxDestination;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button buttonBrowse;
        private Label labelPath;
        private ComboBox comboBoxSortBy;
    }
}
