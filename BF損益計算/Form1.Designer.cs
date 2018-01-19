namespace BF損益計算
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.btnSaveTo = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SD1 = new System.Windows.Forms.SaveFileDialog();
            this.OD1 = new System.Windows.Forms.OpenFileDialog();
            this.OD2 = new System.Windows.Forms.OpenFileDialog();
            this.SD2 = new System.Windows.Forms.SaveFileDialog();
            this.button3 = new System.Windows.Forms.Button();
            this.btnCalc = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(443, 216);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSaveTo
            // 
            this.btnSaveTo.Location = new System.Drawing.Point(25, 101);
            this.btnSaveTo.Name = "btnSaveTo";
            this.btnSaveTo.Size = new System.Drawing.Size(184, 23);
            this.btnSaveTo.TabIndex = 1;
            this.btnSaveTo.Text = "価格データを取得して保存...";
            this.btnSaveTo.UseVisualStyleBackColor = true;
            this.btnSaveTo.Click += new System.EventHandler(this.btnSaveTo_ClickAsync);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(25, 140);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(184, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "取引履歴CSVをロード...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // SD1
            // 
            this.SD1.FileName = global::BF損益計算.Properties.Settings.Default.PriceHistoryFileName;
            this.SD1.Filter = "CSVファイル|*.csv";
            // 
            // OD1
            // 
            this.OD1.FileName = global::BF損益計算.Properties.Settings.Default.TradeHistoryFileName;
            this.OD1.Filter = "csvファイル|*.csv";
            // 
            // OD2
            // 
            this.OD2.FileName = global::BF損益計算.Properties.Settings.Default.PriceHistoryFileName;
            this.OD2.Filter = "csvファイル|*.csv";
            // 
            // SD2
            // 
            this.SD2.Filter = "CSVファイル|*.csv";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(25, 178);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(184, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "価格データをロード...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnCalc
            // 
            this.btnCalc.Location = new System.Drawing.Point(25, 216);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(184, 23);
            this.btnCalc.TabIndex = 4;
            this.btnCalc.Text = "移動平均と売買損益を計算...";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(22, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(334, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "bitFlyer専用の移動平均簿価と売買損益計算用ツール";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(22, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(375, 19);
            this.label2.TabIndex = 6;
            this.label2.Text = "計算結果等について、一切保証いたしません!!";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(630, 261);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnSaveTo);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "ネコにビットコイン";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnSaveTo;
        private System.Windows.Forms.SaveFileDialog SD1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.OpenFileDialog OD1;
        private System.Windows.Forms.OpenFileDialog OD2;
        private System.Windows.Forms.SaveFileDialog SD2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

