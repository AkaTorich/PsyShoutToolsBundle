namespace ThunderPriceCalculator
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

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lblDeposit = new System.Windows.Forms.Label();
            this.lblCurrentPrice = new System.Windows.Forms.Label();
            this.lblTickPrice = new System.Windows.Forms.Label();
            this.lblTakeProfit = new System.Windows.Forms.Label();
            this.lblStopLoss = new System.Windows.Forms.Label();
            this.lblVolume = new System.Windows.Forms.Label();
            this.lblDealAmount = new System.Windows.Forms.Label();
            this.lblMargin = new System.Windows.Forms.Label();
            this.txtDeposit = new System.Windows.Forms.TextBox();
            this.txtRisk = new System.Windows.Forms.TextBox();
            this.cmbRiskType = new System.Windows.Forms.ComboBox();
            this.txtCurrentPrice = new System.Windows.Forms.TextBox();
            this.txtTickPrice = new System.Windows.Forms.TextBox();
            this.lblDesiredProfit = new System.Windows.Forms.Label();
            this.txtDesiredProfit = new System.Windows.Forms.TextBox();
            this.cmbProfitType = new System.Windows.Forms.ComboBox();
            this.lblDesiredLeverage = new System.Windows.Forms.Label();
            this.txtDesiredLeverage = new System.Windows.Forms.TextBox();
            this.lblOrderType = new System.Windows.Forms.Label();
            this.cmbOrderType = new System.Windows.Forms.ComboBox();
            this.txtTakeProfit = new System.Windows.Forms.TextBox();
            this.txtStopLoss = new System.Windows.Forms.TextBox();
            this.txtVolume = new System.Windows.Forms.TextBox();
            this.txtDealAmount = new System.Windows.Forms.TextBox();
            this.txtMargin = new System.Windows.Forms.TextBox();
            this.lblMarginType = new System.Windows.Forms.Label();
            this.cmbMarginType = new System.Windows.Forms.ComboBox();
            this.lblNotification = new System.Windows.Forms.Label();
            this.txtNotification = new System.Windows.Forms.TextBox();
            this.lblRisk = new System.Windows.Forms.Label();
            this.lblTraderType = new System.Windows.Forms.Label();
            this.cmbTraderType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblDeposit
            // 
            this.lblDeposit.BackColor = System.Drawing.Color.Transparent;
            this.lblDeposit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDeposit.Location = new System.Drawing.Point(20, 20);
            this.lblDeposit.Name = "lblDeposit";
            this.lblDeposit.Size = new System.Drawing.Size(150, 20);
            this.lblDeposit.TabIndex = 0;
            this.lblDeposit.Text = "Размер депозита ($):";
            // 
            // lblCurrentPrice
            // 
            this.lblCurrentPrice.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblCurrentPrice.Location = new System.Drawing.Point(20, 99);
            this.lblCurrentPrice.Name = "lblCurrentPrice";
            this.lblCurrentPrice.Size = new System.Drawing.Size(150, 20);
            this.lblCurrentPrice.TabIndex = 6;
            this.lblCurrentPrice.Text = "Текущая цена:";
            // 
            // lblTickPrice
            // 
            this.lblTickPrice.BackColor = System.Drawing.Color.Transparent;
            this.lblTickPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblTickPrice.Location = new System.Drawing.Point(20, 139);
            this.lblTickPrice.Name = "lblTickPrice";
            this.lblTickPrice.Size = new System.Drawing.Size(150, 20);
            this.lblTickPrice.TabIndex = 8;
            this.lblTickPrice.Text = "Цена тика ($):";
            // 
            // lblTakeProfit
            // 
            this.lblTakeProfit.BackColor = System.Drawing.Color.Transparent;
            this.lblTakeProfit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblTakeProfit.Location = new System.Drawing.Point(20, 379);
            this.lblTakeProfit.Name = "lblTakeProfit";
            this.lblTakeProfit.Size = new System.Drawing.Size(150, 20);
            this.lblTakeProfit.TabIndex = 10;
            this.lblTakeProfit.Text = "Тейк-профит (пункты):";
            // 
            // lblStopLoss
            // 
            this.lblStopLoss.BackColor = System.Drawing.Color.Transparent;
            this.lblStopLoss.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblStopLoss.Location = new System.Drawing.Point(20, 419);
            this.lblStopLoss.Name = "lblStopLoss";
            this.lblStopLoss.Size = new System.Drawing.Size(150, 20);
            this.lblStopLoss.TabIndex = 12;
            this.lblStopLoss.Text = "Стоп-лосс (пункты):";
            // 
            // lblVolume
            // 
            this.lblVolume.BackColor = System.Drawing.Color.Transparent;
            this.lblVolume.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblVolume.Location = new System.Drawing.Point(20, 459);
            this.lblVolume.Name = "lblVolume";
            this.lblVolume.Size = new System.Drawing.Size(150, 20);
            this.lblVolume.TabIndex = 14;
            this.lblVolume.Text = "Объем (лоты):";
            // 
            // lblDealAmount
            // 
            this.lblDealAmount.BackColor = System.Drawing.Color.Transparent;
            this.lblDealAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDealAmount.Location = new System.Drawing.Point(20, 499);
            this.lblDealAmount.Name = "lblDealAmount";
            this.lblDealAmount.Size = new System.Drawing.Size(150, 20);
            this.lblDealAmount.TabIndex = 16;
            this.lblDealAmount.Text = "Сумма сделки ($):";
            // 
            // lblMargin
            // 
            this.lblMargin.BackColor = System.Drawing.Color.Transparent;
            this.lblMargin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblMargin.Location = new System.Drawing.Point(20, 539);
            this.lblMargin.Name = "lblMargin";
            this.lblMargin.Size = new System.Drawing.Size(150, 20);
            this.lblMargin.TabIndex = 18;
            this.lblMargin.Text = "Необходимая маржа ($):";
            // 
            // txtDeposit
            // 
            this.txtDeposit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.txtDeposit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDeposit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtDeposit.Location = new System.Drawing.Point(180, 20);
            this.txtDeposit.Name = "txtDeposit";
            this.txtDeposit.Size = new System.Drawing.Size(180, 20);
            this.txtDeposit.TabIndex = 1;
            this.txtDeposit.Text = "145";
            // 
            // txtRisk
            // 
            this.txtRisk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.txtRisk.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRisk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtRisk.Location = new System.Drawing.Point(180, 59);
            this.txtRisk.Name = "txtRisk";
            this.txtRisk.Size = new System.Drawing.Size(99, 20);
            this.txtRisk.TabIndex = 3;
            this.txtRisk.Text = "1.5";
            // 
            // cmbRiskType
            // 
            this.cmbRiskType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.cmbRiskType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRiskType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbRiskType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmbRiskType.Items.AddRange(new object[] {
            "USD",
            "Percent"});
            this.cmbRiskType.Location = new System.Drawing.Point(285, 59);
            this.cmbRiskType.Name = "cmbRiskType";
            this.cmbRiskType.Size = new System.Drawing.Size(75, 21);
            this.cmbRiskType.TabIndex = 5;
            // 
            // txtCurrentPrice
            // 
            this.txtCurrentPrice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.txtCurrentPrice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCurrentPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtCurrentPrice.Location = new System.Drawing.Point(180, 99);
            this.txtCurrentPrice.Name = "txtCurrentPrice";
            this.txtCurrentPrice.Size = new System.Drawing.Size(180, 20);
            this.txtCurrentPrice.TabIndex = 7;
            this.txtCurrentPrice.Text = "108124.8";
            // 
            // txtTickPrice
            // 
            this.txtTickPrice.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.txtTickPrice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTickPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtTickPrice.Location = new System.Drawing.Point(180, 139);
            this.txtTickPrice.Name = "txtTickPrice";
            this.txtTickPrice.Size = new System.Drawing.Size(180, 20);
            this.txtTickPrice.TabIndex = 9;
            this.txtTickPrice.Text = "0.1";
            // 
            // lblDesiredProfit
            // 
            this.lblDesiredProfit.BackColor = System.Drawing.Color.Transparent;
            this.lblDesiredProfit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDesiredProfit.Location = new System.Drawing.Point(20, 179);
            this.lblDesiredProfit.Name = "lblDesiredProfit";
            this.lblDesiredProfit.Size = new System.Drawing.Size(150, 20);
            this.lblDesiredProfit.TabIndex = 26;
            this.lblDesiredProfit.Text = "Желаемая прибыль:";
            // 
            // txtDesiredProfit
            // 
            this.txtDesiredProfit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.txtDesiredProfit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDesiredProfit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtDesiredProfit.Location = new System.Drawing.Point(180, 179);
            this.txtDesiredProfit.Name = "txtDesiredProfit";
            this.txtDesiredProfit.Size = new System.Drawing.Size(100, 20);
            this.txtDesiredProfit.TabIndex = 27;
            this.txtDesiredProfit.Text = "3";
            // 
            // cmbProfitType
            // 
            this.cmbProfitType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.cmbProfitType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProfitType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbProfitType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmbProfitType.Items.AddRange(new object[] {
            "Percent",
            "USD"});
            this.cmbProfitType.Location = new System.Drawing.Point(285, 179);
            this.cmbProfitType.Name = "cmbProfitType";
            this.cmbProfitType.Size = new System.Drawing.Size(75, 21);
            this.cmbProfitType.TabIndex = 28;
            // 
            // lblDesiredLeverage
            // 
            this.lblDesiredLeverage.BackColor = System.Drawing.Color.Transparent;
            this.lblDesiredLeverage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblDesiredLeverage.Location = new System.Drawing.Point(20, 219);
            this.lblDesiredLeverage.Name = "lblDesiredLeverage";
            this.lblDesiredLeverage.Size = new System.Drawing.Size(150, 20);
            this.lblDesiredLeverage.TabIndex = 29;
            this.lblDesiredLeverage.Text = "Желаемое плечо:";
            // 
            // txtDesiredLeverage
            // 
            this.txtDesiredLeverage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.txtDesiredLeverage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDesiredLeverage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtDesiredLeverage.Location = new System.Drawing.Point(180, 219);
            this.txtDesiredLeverage.Name = "txtDesiredLeverage";
            this.txtDesiredLeverage.Size = new System.Drawing.Size(180, 20);
            this.txtDesiredLeverage.TabIndex = 30;
            this.txtDesiredLeverage.Text = "10";
            // 
            // lblOrderType
            // 
            this.lblOrderType.BackColor = System.Drawing.Color.Transparent;
            this.lblOrderType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblOrderType.Location = new System.Drawing.Point(20, 339);
            this.lblOrderType.Name = "lblOrderType";
            this.lblOrderType.Size = new System.Drawing.Size(150, 20);
            this.lblOrderType.TabIndex = 24;
            this.lblOrderType.Text = "Тип ордера:";
            // 
            // cmbOrderType
            // 
            this.cmbOrderType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.cmbOrderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOrderType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbOrderType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmbOrderType.Items.AddRange(new object[] {
            "Buy",
            "Sell"});
            this.cmbOrderType.Location = new System.Drawing.Point(180, 339);
            this.cmbOrderType.Name = "cmbOrderType";
            this.cmbOrderType.Size = new System.Drawing.Size(180, 21);
            this.cmbOrderType.TabIndex = 25;
            this.cmbOrderType.SelectedIndexChanged += new System.EventHandler(this.InputField_TextChanged);
            // 
            // txtTakeProfit
            // 
            this.txtTakeProfit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.txtTakeProfit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTakeProfit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtTakeProfit.Location = new System.Drawing.Point(180, 379);
            this.txtTakeProfit.Name = "txtTakeProfit";
            this.txtTakeProfit.ReadOnly = true;
            this.txtTakeProfit.Size = new System.Drawing.Size(180, 20);
            this.txtTakeProfit.TabIndex = 11;
            // 
            // txtStopLoss
            // 
            this.txtStopLoss.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.txtStopLoss.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStopLoss.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtStopLoss.Location = new System.Drawing.Point(180, 419);
            this.txtStopLoss.Name = "txtStopLoss";
            this.txtStopLoss.ReadOnly = true;
            this.txtStopLoss.Size = new System.Drawing.Size(180, 20);
            this.txtStopLoss.TabIndex = 13;
            // 
            // txtVolume
            // 
            this.txtVolume.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.txtVolume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtVolume.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtVolume.Location = new System.Drawing.Point(180, 459);
            this.txtVolume.Name = "txtVolume";
            this.txtVolume.ReadOnly = true;
            this.txtVolume.Size = new System.Drawing.Size(180, 20);
            this.txtVolume.TabIndex = 15;
            // 
            // txtDealAmount
            // 
            this.txtDealAmount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.txtDealAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDealAmount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtDealAmount.Location = new System.Drawing.Point(180, 499);
            this.txtDealAmount.Name = "txtDealAmount";
            this.txtDealAmount.ReadOnly = true;
            this.txtDealAmount.Size = new System.Drawing.Size(180, 20);
            this.txtDealAmount.TabIndex = 17;
            // 
            // txtMargin
            // 
            this.txtMargin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.txtMargin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMargin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtMargin.Location = new System.Drawing.Point(180, 539);
            this.txtMargin.Name = "txtMargin";
            this.txtMargin.ReadOnly = true;
            this.txtMargin.Size = new System.Drawing.Size(180, 20);
            this.txtMargin.TabIndex = 19;
            // 
            // lblMarginType
            // 
            this.lblMarginType.BackColor = System.Drawing.Color.Transparent;
            this.lblMarginType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblMarginType.Location = new System.Drawing.Point(20, 259);
            this.lblMarginType.Name = "lblMarginType";
            this.lblMarginType.Size = new System.Drawing.Size(150, 20);
            this.lblMarginType.TabIndex = 22;
            this.lblMarginType.Text = "Тип маржи:";
            // 
            // cmbMarginType
            // 
            this.cmbMarginType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.cmbMarginType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMarginType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbMarginType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmbMarginType.Location = new System.Drawing.Point(180, 259);
            this.cmbMarginType.Name = "cmbMarginType";
            this.cmbMarginType.Size = new System.Drawing.Size(180, 21);
            this.cmbMarginType.TabIndex = 23;
            // 
            // lblNotification
            // 
            this.lblNotification.BackColor = System.Drawing.Color.Transparent;
            this.lblNotification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblNotification.Location = new System.Drawing.Point(20, 579);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(150, 20);
            this.lblNotification.TabIndex = 24;
            this.lblNotification.Text = "Уведомления:";
            // 
            // txtNotification
            // 
            this.txtNotification.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(55)))), ((int)(((byte)(55)))));
            this.txtNotification.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNotification.ForeColor = System.Drawing.Color.White;
            this.txtNotification.Location = new System.Drawing.Point(20, 599);
            this.txtNotification.Multiline = true;
            this.txtNotification.Name = "txtNotification";
            this.txtNotification.ReadOnly = true;
            this.txtNotification.Size = new System.Drawing.Size(340, 89);
            this.txtNotification.TabIndex = 25;
            // 
            // lblRisk
            // 
            this.lblRisk.BackColor = System.Drawing.Color.Transparent;
            this.lblRisk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblRisk.Location = new System.Drawing.Point(20, 61);
            this.lblRisk.Name = "lblRisk";
            this.lblRisk.Size = new System.Drawing.Size(150, 20);
            this.lblRisk.TabIndex = 2;
            this.lblRisk.Text = "Риск:";
            // 
            // lblTraderType
            // 
            this.lblTraderType.BackColor = System.Drawing.Color.Transparent;
            this.lblTraderType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblTraderType.Location = new System.Drawing.Point(20, 299);
            this.lblTraderType.Name = "lblTraderType";
            this.lblTraderType.Size = new System.Drawing.Size(150, 20);
            this.lblTraderType.TabIndex = 31;
            this.lblTraderType.Text = "Тип комиссии:";
            // 
            // cmbTraderType
            // 
            this.cmbTraderType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.cmbTraderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTraderType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbTraderType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cmbTraderType.Items.AddRange(new object[] {
            "Taker",
            "Maker"});
            this.cmbTraderType.Location = new System.Drawing.Point(180, 299);
            this.cmbTraderType.Name = "cmbTraderType";
            this.cmbTraderType.Size = new System.Drawing.Size(180, 21);
            this.cmbTraderType.TabIndex = 32;
            // 
            // MainForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ClientSize = new System.Drawing.Size(384, 705);
            this.TopMost = true;
            this.Controls.Add(this.lblDeposit);
            this.Controls.Add(this.txtDeposit);
            this.Controls.Add(this.lblRisk);
            this.Controls.Add(this.txtRisk);
            this.Controls.Add(this.cmbRiskType);
            this.Controls.Add(this.lblCurrentPrice);
            this.Controls.Add(this.txtCurrentPrice);
            this.Controls.Add(this.lblTickPrice);
            this.Controls.Add(this.txtTickPrice);
            this.Controls.Add(this.lblDesiredProfit);
            this.Controls.Add(this.txtDesiredProfit);
            this.Controls.Add(this.cmbProfitType);
            this.Controls.Add(this.lblDesiredLeverage);
            this.Controls.Add(this.txtDesiredLeverage);
            this.Controls.Add(this.lblMarginType);
            this.Controls.Add(this.cmbMarginType);
            this.Controls.Add(this.lblTraderType);
            this.Controls.Add(this.cmbTraderType);
            this.Controls.Add(this.lblOrderType);
            this.Controls.Add(this.cmbOrderType);
            this.Controls.Add(this.lblTakeProfit);
            this.Controls.Add(this.txtTakeProfit);
            this.Controls.Add(this.lblStopLoss);
            this.Controls.Add(this.txtStopLoss);
            this.Controls.Add(this.lblVolume);
            this.Controls.Add(this.txtVolume);
            this.Controls.Add(this.lblDealAmount);
            this.Controls.Add(this.txtDealAmount);
            this.Controls.Add(this.lblMargin);
            this.Controls.Add(this.txtMargin);
            this.Controls.Add(this.lblNotification);
            this.Controls.Add(this.txtNotification);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Thunder Price Calculator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox txtDeposit;
        private System.Windows.Forms.TextBox txtRisk;
        private System.Windows.Forms.TextBox txtTickPrice;
        private System.Windows.Forms.TextBox txtCurrentPrice;
        private System.Windows.Forms.ComboBox cmbRiskType;
        private System.Windows.Forms.TextBox txtTakeProfit;
        private System.Windows.Forms.TextBox txtStopLoss;
        private System.Windows.Forms.TextBox txtVolume;
        private System.Windows.Forms.TextBox txtDealAmount;
        private System.Windows.Forms.TextBox txtMargin;
        private System.Windows.Forms.Label lblDeposit;
        private System.Windows.Forms.Label lblTickPrice;
        private System.Windows.Forms.Label lblCurrentPrice;
        private System.Windows.Forms.Label lblTakeProfit;
        private System.Windows.Forms.Label lblStopLoss;
        private System.Windows.Forms.Label lblVolume;
        private System.Windows.Forms.Label lblDealAmount;
        private System.Windows.Forms.Label lblMargin;
        private System.Windows.Forms.Label lblDesiredLeverage;
        private System.Windows.Forms.TextBox txtDesiredLeverage;
        private System.Windows.Forms.Label lblOrderType;
        private System.Windows.Forms.ComboBox cmbOrderType;
        private System.Windows.Forms.Label lblMarginType;
        private System.Windows.Forms.ComboBox cmbMarginType;
        private System.Windows.Forms.Label lblNotification;
        private System.Windows.Forms.TextBox txtNotification;
        private System.Windows.Forms.Label lblDesiredProfit;
        private System.Windows.Forms.TextBox txtDesiredProfit;
        private System.Windows.Forms.ComboBox cmbProfitType;
        private System.Windows.Forms.Label lblRisk;
        private System.Windows.Forms.Label lblTraderType;
        private System.Windows.Forms.ComboBox cmbTraderType;
    }
}