namespace ProtectionPacker
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpFileSelection = new System.Windows.Forms.GroupBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.lblOutputFile = new System.Windows.Forms.Label();
            this.lblInputFile = new System.Windows.Forms.Label();
            this.grpProtectionOptions = new System.Windows.Forms.GroupBox();
            this.chkFakeAPI = new System.Windows.Forms.CheckBox();
            this.chkVirtualization = new System.Windows.Forms.CheckBox();
            this.chkResourceProtection = new System.Windows.Forms.CheckBox();
            this.chkStringEncryption = new System.Windows.Forms.CheckBox();
            this.chkObfuscation = new System.Windows.Forms.CheckBox();
            this.chkAntiDebug = new System.Windows.Forms.CheckBox();
            this.chkEncryption = new System.Windows.Forms.CheckBox();
            this.chkCompression = new System.Windows.Forms.CheckBox();
            this.grpAntiDumpLevel = new System.Windows.Forms.GroupBox();
            this.rbAntiDumpMaximum = new System.Windows.Forms.RadioButton();
            this.rbAntiDumpMedium = new System.Windows.Forms.RadioButton();
            this.rbAntiDumpLight = new System.Windows.Forms.RadioButton();
            this.rbAntiDumpNone = new System.Windows.Forms.RadioButton();
            this.grpCompressionLevel = new System.Windows.Forms.GroupBox();
            this.rbCompressionMaximum = new System.Windows.Forms.RadioButton();
            this.rbCompressionOptimal = new System.Windows.Forms.RadioButton();
            this.rbCompressionFast = new System.Windows.Forms.RadioButton();
            this.rbCompressionNone = new System.Windows.Forms.RadioButton();
            this.grpApplicationType = new System.Windows.Forms.GroupBox();
            this.rbWindowsApp = new System.Windows.Forms.RadioButton();
            this.rbConsoleApp = new System.Windows.Forms.RadioButton();
            this.grpOutputFileType = new System.Windows.Forms.GroupBox();
            this.rbOutputDll = new System.Windows.Forms.RadioButton();
            this.rbOutputExe = new System.Windows.Forms.RadioButton();
            this.grpDebugOptions = new System.Windows.Forms.GroupBox();
            this.chkPackerDebug = new System.Windows.Forms.CheckBox();
            this.chkDebugOutput = new System.Windows.Forms.CheckBox();
            this.btnPack = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnMaximumProtection = new System.Windows.Forms.Button();
            this.btnLightProtection = new System.Windows.Forms.Button();
            this.grpFileSelection.SuspendLayout();
            this.grpProtectionOptions.SuspendLayout();
            this.grpAntiDumpLevel.SuspendLayout();
            this.grpCompressionLevel.SuspendLayout();
            this.grpApplicationType.SuspendLayout();
            this.grpOutputFileType.SuspendLayout();
            this.grpDebugOptions.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(360, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🛡️ PsyShout Protection Packer";
            // 
            // grpFileSelection
            // 
            this.grpFileSelection.Controls.Add(this.btnBrowseOutput);
            this.grpFileSelection.Controls.Add(this.btnBrowseInput);
            this.grpFileSelection.Controls.Add(this.txtOutputFile);
            this.grpFileSelection.Controls.Add(this.txtInputFile);
            this.grpFileSelection.Controls.Add(this.lblOutputFile);
            this.grpFileSelection.Controls.Add(this.lblInputFile);
            this.grpFileSelection.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpFileSelection.Location = new System.Drawing.Point(17, 50);
            this.grpFileSelection.Name = "grpFileSelection";
            this.grpFileSelection.Size = new System.Drawing.Size(760, 100);
            this.grpFileSelection.TabIndex = 1;
            this.grpFileSelection.TabStop = false;
            this.grpFileSelection.Text = "Выбор файлов";
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Location = new System.Drawing.Point(660, 60);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(85, 25);
            this.btnBrowseOutput.TabIndex = 5;
            this.btnBrowseOutput.Text = "Обзор...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Location = new System.Drawing.Point(660, 28);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(85, 25);
            this.btnBrowseInput.TabIndex = 4;
            this.btnBrowseInput.Text = "Обзор...";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Location = new System.Drawing.Point(110, 61);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.Size = new System.Drawing.Size(540, 23);
            this.txtOutputFile.TabIndex = 3;
            // 
            // txtInputFile
            // 
            this.txtInputFile.Location = new System.Drawing.Point(110, 29);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.Size = new System.Drawing.Size(540, 23);
            this.txtInputFile.TabIndex = 2;
            // 
            // lblOutputFile
            // 
            this.lblOutputFile.AutoSize = true;
            this.lblOutputFile.Location = new System.Drawing.Point(15, 64);
            this.lblOutputFile.Name = "lblOutputFile";
            this.lblOutputFile.Size = new System.Drawing.Size(93, 15);
            this.lblOutputFile.TabIndex = 1;
            this.lblOutputFile.Text = "Выходной файл:";
            // 
            // lblInputFile
            // 
            this.lblInputFile.AutoSize = true;
            this.lblInputFile.Location = new System.Drawing.Point(15, 32);
            this.lblInputFile.Name = "lblInputFile";
            this.lblInputFile.Size = new System.Drawing.Size(89, 15);
            this.lblInputFile.TabIndex = 0;
            this.lblInputFile.Text = "Входной файл:";
            // 
            // grpProtectionOptions
            // 
            this.grpProtectionOptions.Controls.Add(this.chkFakeAPI);
            this.grpProtectionOptions.Controls.Add(this.chkVirtualization);
            this.grpProtectionOptions.Controls.Add(this.chkResourceProtection);
            this.grpProtectionOptions.Controls.Add(this.chkStringEncryption);
            this.grpProtectionOptions.Controls.Add(this.chkObfuscation);
            this.grpProtectionOptions.Controls.Add(this.chkAntiDebug);
            this.grpProtectionOptions.Controls.Add(this.chkEncryption);
            this.grpProtectionOptions.Controls.Add(this.chkCompression);
            this.grpProtectionOptions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpProtectionOptions.Location = new System.Drawing.Point(17, 156);
            this.grpProtectionOptions.Name = "grpProtectionOptions";
            this.grpProtectionOptions.Size = new System.Drawing.Size(370, 140);
            this.grpProtectionOptions.TabIndex = 2;
            this.grpProtectionOptions.TabStop = false;
            this.grpProtectionOptions.Text = "Параметры защиты";
            // 
            // chkFakeAPI
            // 
            this.chkFakeAPI.AutoSize = true;
            this.chkFakeAPI.Checked = true;
            this.chkFakeAPI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFakeAPI.Location = new System.Drawing.Point(200, 110);
            this.chkFakeAPI.Name = "chkFakeAPI";
            this.chkFakeAPI.Size = new System.Drawing.Size(154, 19);
            this.chkFakeAPI.TabIndex = 7;
            this.chkFakeAPI.Text = "Ложные API вызовы";
            this.chkFakeAPI.UseVisualStyleBackColor = true;
            // 
            // chkVirtualization
            // 
            this.chkVirtualization.AutoSize = true;
            this.chkVirtualization.Checked = true;
            this.chkVirtualization.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVirtualization.Location = new System.Drawing.Point(200, 85);
            this.chkVirtualization.Name = "chkVirtualization";
            this.chkVirtualization.Size = new System.Drawing.Size(145, 19);
            this.chkVirtualization.TabIndex = 6;
            this.chkVirtualization.Text = "Виртуализация кода";
            this.chkVirtualization.UseVisualStyleBackColor = true;
            // 
            // chkResourceProtection
            // 
            this.chkResourceProtection.AutoSize = true;
            this.chkResourceProtection.Checked = true;
            this.chkResourceProtection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkResourceProtection.Location = new System.Drawing.Point(200, 60);
            this.chkResourceProtection.Name = "chkResourceProtection";
            this.chkResourceProtection.Size = new System.Drawing.Size(126, 19);
            this.chkResourceProtection.TabIndex = 5;
            this.chkResourceProtection.Text = "Защита ресурсов";
            this.chkResourceProtection.UseVisualStyleBackColor = true;
            // 
            // chkStringEncryption
            // 
            this.chkStringEncryption.AutoSize = true;
            this.chkStringEncryption.Checked = true;
            this.chkStringEncryption.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStringEncryption.Location = new System.Drawing.Point(200, 35);
            this.chkStringEncryption.Name = "chkStringEncryption";
            this.chkStringEncryption.Size = new System.Drawing.Size(133, 19);
            this.chkStringEncryption.TabIndex = 4;
            this.chkStringEncryption.Text = "Шифрование строк";
            this.chkStringEncryption.UseVisualStyleBackColor = true;
            // 
            // chkObfuscation
            // 
            this.chkObfuscation.AutoSize = true;
            this.chkObfuscation.Checked = true;
            this.chkObfuscation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkObfuscation.Location = new System.Drawing.Point(15, 110);
            this.chkObfuscation.Name = "chkObfuscation";
            this.chkObfuscation.Size = new System.Drawing.Size(97, 19);
            this.chkObfuscation.TabIndex = 3;
            this.chkObfuscation.Text = "Обфускация";
            this.chkObfuscation.UseVisualStyleBackColor = true;
            // 
            // chkAntiDebug
            // 
            this.chkAntiDebug.AutoSize = true;
            this.chkAntiDebug.Checked = true;
            this.chkAntiDebug.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAntiDebug.Location = new System.Drawing.Point(15, 85);
            this.chkAntiDebug.Name = "chkAntiDebug";
            this.chkAntiDebug.Size = new System.Drawing.Size(113, 19);
            this.chkAntiDebug.TabIndex = 2;
            this.chkAntiDebug.Text = "Анти-отладка";
            this.chkAntiDebug.UseVisualStyleBackColor = true;
            // 
            // chkEncryption
            // 
            this.chkEncryption.AutoSize = true;
            this.chkEncryption.Checked = true;
            this.chkEncryption.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEncryption.Location = new System.Drawing.Point(15, 60);
            this.chkEncryption.Name = "chkEncryption";
            this.chkEncryption.Size = new System.Drawing.Size(100, 19);
            this.chkEncryption.TabIndex = 1;
            this.chkEncryption.Text = "Шифрование";
            this.chkEncryption.UseVisualStyleBackColor = true;
            // 
            // chkCompression
            // 
            this.chkCompression.AutoSize = true;
            this.chkCompression.Checked = true;
            this.chkCompression.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCompression.Location = new System.Drawing.Point(15, 35);
            this.chkCompression.Name = "chkCompression";
            this.chkCompression.Size = new System.Drawing.Size(72, 19);
            this.chkCompression.TabIndex = 0;
            this.chkCompression.Text = "Сжатие";
            this.chkCompression.UseVisualStyleBackColor = true;
            // 
            // grpAntiDumpLevel
            // 
            this.grpAntiDumpLevel.Controls.Add(this.rbAntiDumpMaximum);
            this.grpAntiDumpLevel.Controls.Add(this.rbAntiDumpMedium);
            this.grpAntiDumpLevel.Controls.Add(this.rbAntiDumpLight);
            this.grpAntiDumpLevel.Controls.Add(this.rbAntiDumpNone);
            this.grpAntiDumpLevel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpAntiDumpLevel.Location = new System.Drawing.Point(407, 156);
            this.grpAntiDumpLevel.Name = "grpAntiDumpLevel";
            this.grpAntiDumpLevel.Size = new System.Drawing.Size(180, 140);
            this.grpAntiDumpLevel.TabIndex = 3;
            this.grpAntiDumpLevel.TabStop = false;
            this.grpAntiDumpLevel.Text = "Уровень анти-дампа";
            // 
            // rbAntiDumpMaximum
            // 
            this.rbAntiDumpMaximum.AutoSize = true;
            this.rbAntiDumpMaximum.Checked = true;
            this.rbAntiDumpMaximum.Location = new System.Drawing.Point(15, 110);
            this.rbAntiDumpMaximum.Name = "rbAntiDumpMaximum";
            this.rbAntiDumpMaximum.Size = new System.Drawing.Size(115, 19);
            this.rbAntiDumpMaximum.TabIndex = 3;
            this.rbAntiDumpMaximum.TabStop = true;
            this.rbAntiDumpMaximum.Text = "Максимальный";
            this.rbAntiDumpMaximum.UseVisualStyleBackColor = true;
            // 
            // rbAntiDumpMedium
            // 
            this.rbAntiDumpMedium.AutoSize = true;
            this.rbAntiDumpMedium.Location = new System.Drawing.Point(15, 85);
            this.rbAntiDumpMedium.Name = "rbAntiDumpMedium";
            this.rbAntiDumpMedium.Size = new System.Drawing.Size(74, 19);
            this.rbAntiDumpMedium.TabIndex = 2;
            this.rbAntiDumpMedium.Text = "Средний";
            this.rbAntiDumpMedium.UseVisualStyleBackColor = true;
            // 
            // rbAntiDumpLight
            // 
            this.rbAntiDumpLight.AutoSize = true;
            this.rbAntiDumpLight.Location = new System.Drawing.Point(15, 60);
            this.rbAntiDumpLight.Name = "rbAntiDumpLight";
            this.rbAntiDumpLight.Size = new System.Drawing.Size(68, 19);
            this.rbAntiDumpLight.TabIndex = 1;
            this.rbAntiDumpLight.Text = "Легкий";
            this.rbAntiDumpLight.UseVisualStyleBackColor = true;
            // 
            // rbAntiDumpNone
            // 
            this.rbAntiDumpNone.AutoSize = true;
            this.rbAntiDumpNone.Location = new System.Drawing.Point(15, 35);
            this.rbAntiDumpNone.Name = "rbAntiDumpNone";
            this.rbAntiDumpNone.Size = new System.Drawing.Size(92, 19);
            this.rbAntiDumpNone.TabIndex = 0;
            this.rbAntiDumpNone.Text = "Отключено";
            this.rbAntiDumpNone.UseVisualStyleBackColor = true;
            // 
            // grpCompressionLevel
            // 
            this.grpCompressionLevel.Controls.Add(this.rbCompressionMaximum);
            this.grpCompressionLevel.Controls.Add(this.rbCompressionOptimal);
            this.grpCompressionLevel.Controls.Add(this.rbCompressionFast);
            this.grpCompressionLevel.Controls.Add(this.rbCompressionNone);
            this.grpCompressionLevel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpCompressionLevel.Location = new System.Drawing.Point(597, 156);
            this.grpCompressionLevel.Name = "grpCompressionLevel";
            this.grpCompressionLevel.Size = new System.Drawing.Size(180, 140);
            this.grpCompressionLevel.TabIndex = 4;
            this.grpCompressionLevel.TabStop = false;
            this.grpCompressionLevel.Text = "Уровень сжатия";
            // 
            // rbCompressionMaximum
            // 
            this.rbCompressionMaximum.AutoSize = true;
            this.rbCompressionMaximum.Checked = true;
            this.rbCompressionMaximum.Location = new System.Drawing.Point(15, 110);
            this.rbCompressionMaximum.Name = "rbCompressionMaximum";
            this.rbCompressionMaximum.Size = new System.Drawing.Size(115, 19);
            this.rbCompressionMaximum.TabIndex = 3;
            this.rbCompressionMaximum.TabStop = true;
            this.rbCompressionMaximum.Text = "Максимальный";
            this.rbCompressionMaximum.UseVisualStyleBackColor = true;
            // 
            // rbCompressionOptimal
            // 
            this.rbCompressionOptimal.AutoSize = true;
            this.rbCompressionOptimal.Location = new System.Drawing.Point(15, 85);
            this.rbCompressionOptimal.Name = "rbCompressionOptimal";
            this.rbCompressionOptimal.Size = new System.Drawing.Size(103, 19);
            this.rbCompressionOptimal.TabIndex = 2;
            this.rbCompressionOptimal.Text = "Оптимальный";
            this.rbCompressionOptimal.UseVisualStyleBackColor = true;
            // 
            // rbCompressionFast
            // 
            this.rbCompressionFast.AutoSize = true;
            this.rbCompressionFast.Location = new System.Drawing.Point(15, 60);
            this.rbCompressionFast.Name = "rbCompressionFast";
            this.rbCompressionFast.Size = new System.Drawing.Size(75, 19);
            this.rbCompressionFast.TabIndex = 1;
            this.rbCompressionFast.Text = "Быстрый";
            this.rbCompressionFast.UseVisualStyleBackColor = true;
            // 
            // rbCompressionNone
            // 
            this.rbCompressionNone.AutoSize = true;
            this.rbCompressionNone.Location = new System.Drawing.Point(15, 35);
            this.rbCompressionNone.Name = "rbCompressionNone";
            this.rbCompressionNone.Size = new System.Drawing.Size(92, 19);
            this.rbCompressionNone.TabIndex = 0;
            this.rbCompressionNone.Text = "Отключено";
            this.rbCompressionNone.UseVisualStyleBackColor = true;
            //
            // grpApplicationType
            //
            this.grpApplicationType.Controls.Add(this.rbWindowsApp);
            this.grpApplicationType.Controls.Add(this.rbConsoleApp);
            this.grpApplicationType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpApplicationType.Location = new System.Drawing.Point(197, 302);
            this.grpApplicationType.Name = "grpApplicationType";
            this.grpApplicationType.Size = new System.Drawing.Size(250, 80);
            this.grpApplicationType.TabIndex = 5;
            this.grpApplicationType.TabStop = false;
            this.grpApplicationType.Text = "Тип приложения (для EXE)";
            //
            // rbWindowsApp
            //
            this.rbWindowsApp.AutoSize = true;
            this.rbWindowsApp.Checked = true;
            this.rbWindowsApp.Location = new System.Drawing.Point(15, 50);
            this.rbWindowsApp.Name = "rbWindowsApp";
            this.rbWindowsApp.Size = new System.Drawing.Size(208, 19);
            this.rbWindowsApp.TabIndex = 1;
            this.rbWindowsApp.TabStop = true;
            this.rbWindowsApp.Text = "Windows-приложение (без консоли)";
            this.rbWindowsApp.UseVisualStyleBackColor = true;
            //
            // rbConsoleApp
            //
            this.rbConsoleApp.AutoSize = true;
            this.rbConsoleApp.Location = new System.Drawing.Point(15, 25);
            this.rbConsoleApp.Name = "rbConsoleApp";
            this.rbConsoleApp.Size = new System.Drawing.Size(224, 19);
            this.rbConsoleApp.TabIndex = 0;
            this.rbConsoleApp.Text = "Консольное приложение (с консолью)";
            this.rbConsoleApp.UseVisualStyleBackColor = true;
            //
            // grpOutputFileType
            //
            this.grpOutputFileType.Controls.Add(this.rbOutputDll);
            this.grpOutputFileType.Controls.Add(this.rbOutputExe);
            this.grpOutputFileType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpOutputFileType.Location = new System.Drawing.Point(17, 302);
            this.grpOutputFileType.Name = "grpOutputFileType";
            this.grpOutputFileType.Size = new System.Drawing.Size(170, 80);
            this.grpOutputFileType.TabIndex = 13;
            this.grpOutputFileType.TabStop = false;
            this.grpOutputFileType.Text = "Тип выходного файла";
            //
            // rbOutputDll
            //
            this.rbOutputDll.AutoSize = true;
            this.rbOutputDll.Location = new System.Drawing.Point(15, 50);
            this.rbOutputDll.Name = "rbOutputDll";
            this.rbOutputDll.Size = new System.Drawing.Size(119, 19);
            this.rbOutputDll.TabIndex = 1;
            this.rbOutputDll.Text = "DLL (библиотека)";
            this.rbOutputDll.UseVisualStyleBackColor = true;
            //
            // rbOutputExe
            //
            this.rbOutputExe.AutoSize = true;
            this.rbOutputExe.Checked = true;
            this.rbOutputExe.Location = new System.Drawing.Point(15, 25);
            this.rbOutputExe.Name = "rbOutputExe";
            this.rbOutputExe.Size = new System.Drawing.Size(137, 19);
            this.rbOutputExe.TabIndex = 0;
            this.rbOutputExe.TabStop = true;
            this.rbOutputExe.Text = "EXE (исполняемый)";
            this.rbOutputExe.UseVisualStyleBackColor = true;
            //
            // grpDebugOptions
            // 
            this.grpDebugOptions.Controls.Add(this.chkPackerDebug);
            this.grpDebugOptions.Controls.Add(this.chkDebugOutput);
            this.grpDebugOptions.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpDebugOptions.Location = new System.Drawing.Point(457, 302);
            this.grpDebugOptions.Name = "grpDebugOptions";
            this.grpDebugOptions.Size = new System.Drawing.Size(320, 80);
            this.grpDebugOptions.TabIndex = 6;
            this.grpDebugOptions.TabStop = false;
            this.grpDebugOptions.Text = "Параметры отладки";
            // 
            // chkPackerDebug
            // 
            this.chkPackerDebug.AutoSize = true;
            this.chkPackerDebug.Location = new System.Drawing.Point(15, 50);
            this.chkPackerDebug.Name = "chkPackerDebug";
            this.chkPackerDebug.Size = new System.Drawing.Size(172, 19);
            this.chkPackerDebug.TabIndex = 1;
            this.chkPackerDebug.Text = "Отладка процесса упаковки";
            this.chkPackerDebug.UseVisualStyleBackColor = true;
            // 
            // chkDebugOutput
            // 
            this.chkDebugOutput.AutoSize = true;
            this.chkDebugOutput.Location = new System.Drawing.Point(15, 25);
            this.chkDebugOutput.Name = "chkDebugOutput";
            this.chkDebugOutput.Size = new System.Drawing.Size(194, 19);
            this.chkDebugOutput.TabIndex = 0;
            this.chkDebugOutput.Text = "Отладочный вывод в защищенном файле";
            this.chkDebugOutput.UseVisualStyleBackColor = true;
            // 
            // btnPack
            // 
            this.btnPack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnPack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPack.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnPack.ForeColor = System.Drawing.Color.White;
            this.btnPack.Location = new System.Drawing.Point(567, 388);
            this.btnPack.Name = "btnPack";
            this.btnPack.Size = new System.Drawing.Size(210, 40);
            this.btnPack.TabIndex = 7;
            this.btnPack.Text = "🚀 Начать упаковку";
            this.btnPack.UseVisualStyleBackColor = false;
            this.btnPack.Click += new System.EventHandler(this.btnPack_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(17, 434);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(760, 23);
            this.progressBar.TabIndex = 8;
            // 
            // grpLog
            // 
            this.grpLog.Controls.Add(this.btnClearLog);
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.grpLog.Location = new System.Drawing.Point(17, 463);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(760, 200);
            this.grpLog.TabIndex = 9;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Журнал операций";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(660, 18);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(85, 25);
            this.btnClearLog.TabIndex = 1;
            this.btnClearLog.Text = "Очистить";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtLog.Location = new System.Drawing.Point(15, 49);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(730, 140);
            this.txtLog.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 676);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(794, 22);
            this.statusStrip.TabIndex = 10;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(94, 17);
            this.lblStatus.Text = "Готов к работе";
            // 
            // btnMaximumProtection
            // 
            this.btnMaximumProtection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnMaximumProtection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaximumProtection.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnMaximumProtection.ForeColor = System.Drawing.Color.White;
            this.btnMaximumProtection.Location = new System.Drawing.Point(17, 388);
            this.btnMaximumProtection.Name = "btnMaximumProtection";
            this.btnMaximumProtection.Size = new System.Drawing.Size(165, 40);
            this.btnMaximumProtection.TabIndex = 11;
            this.btnMaximumProtection.Text = "⚡ Максимальная защита";
            this.btnMaximumProtection.UseVisualStyleBackColor = false;
            this.btnMaximumProtection.Click += new System.EventHandler(this.btnMaximumProtection_Click);
            // 
            // btnLightProtection
            // 
            this.btnLightProtection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(196)))), ((int)(((byte)(15)))));
            this.btnLightProtection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLightProtection.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLightProtection.ForeColor = System.Drawing.Color.White;
            this.btnLightProtection.Location = new System.Drawing.Point(188, 388);
            this.btnLightProtection.Name = "btnLightProtection";
            this.btnLightProtection.Size = new System.Drawing.Size(165, 40);
            this.btnLightProtection.TabIndex = 12;
            this.btnLightProtection.Text = "⚡ Быстрая защита";
            this.btnLightProtection.UseVisualStyleBackColor = false;
            this.btnLightProtection.Click += new System.EventHandler(this.btnLightProtection_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 698);
            this.Controls.Add(this.btnLightProtection);
            this.Controls.Add(this.btnMaximumProtection);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnPack);
            this.Controls.Add(this.grpDebugOptions);
            this.Controls.Add(this.grpOutputFileType);
            this.Controls.Add(this.grpApplicationType);
            this.Controls.Add(this.grpCompressionLevel);
            this.Controls.Add(this.grpAntiDumpLevel);
            this.Controls.Add(this.grpProtectionOptions);
            this.Controls.Add(this.grpFileSelection);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PsyShout Protection Packer v1.0.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.grpFileSelection.ResumeLayout(false);
            this.grpFileSelection.PerformLayout();
            this.grpProtectionOptions.ResumeLayout(false);
            this.grpProtectionOptions.PerformLayout();
            this.grpAntiDumpLevel.ResumeLayout(false);
            this.grpAntiDumpLevel.ResumeLayout(false);
            this.grpCompressionLevel.ResumeLayout(false);
            this.grpCompressionLevel.PerformLayout();
            this.grpApplicationType.ResumeLayout(false);
            this.grpApplicationType.PerformLayout();
            this.grpOutputFileType.ResumeLayout(false);
            this.grpOutputFileType.PerformLayout();
            this.grpDebugOptions.ResumeLayout(false);
            this.grpDebugOptions.PerformLayout();
            this.grpLog.ResumeLayout(false);
            this.grpLog.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpFileSelection;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Label lblOutputFile;
        private System.Windows.Forms.Label lblInputFile;
        private System.Windows.Forms.GroupBox grpProtectionOptions;
        private System.Windows.Forms.CheckBox chkFakeAPI;
        private System.Windows.Forms.CheckBox chkVirtualization;
        private System.Windows.Forms.CheckBox chkResourceProtection;
        private System.Windows.Forms.CheckBox chkStringEncryption;
        private System.Windows.Forms.CheckBox chkObfuscation;
        private System.Windows.Forms.CheckBox chkAntiDebug;
        private System.Windows.Forms.CheckBox chkEncryption;
        private System.Windows.Forms.CheckBox chkCompression;
        private System.Windows.Forms.GroupBox grpAntiDumpLevel;
        private System.Windows.Forms.RadioButton rbAntiDumpMaximum;
        private System.Windows.Forms.RadioButton rbAntiDumpMedium;
        private System.Windows.Forms.RadioButton rbAntiDumpLight;
        private System.Windows.Forms.RadioButton rbAntiDumpNone;
        private System.Windows.Forms.GroupBox grpCompressionLevel;
        private System.Windows.Forms.RadioButton rbCompressionMaximum;
        private System.Windows.Forms.RadioButton rbCompressionOptimal;
        private System.Windows.Forms.RadioButton rbCompressionFast;
        private System.Windows.Forms.RadioButton rbCompressionNone;
        private System.Windows.Forms.GroupBox grpApplicationType;
        private System.Windows.Forms.RadioButton rbWindowsApp;
        private System.Windows.Forms.RadioButton rbConsoleApp;
        private System.Windows.Forms.GroupBox grpOutputFileType;
        private System.Windows.Forms.RadioButton rbOutputDll;
        private System.Windows.Forms.RadioButton rbOutputExe;
        private System.Windows.Forms.GroupBox grpDebugOptions;
        private System.Windows.Forms.CheckBox chkPackerDebug;
        private System.Windows.Forms.CheckBox chkDebugOutput;
        private System.Windows.Forms.Button btnPack;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Button btnMaximumProtection;
        private System.Windows.Forms.Button btnLightProtection;
    }
}

