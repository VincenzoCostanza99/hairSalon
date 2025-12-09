namespace CLIENT_PARRUCCHIERIA
{
    partial class form_login
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
            this.LabelEmail = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxEmail = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelLOGO = new System.Windows.Forms.Label();
            this.buttonAccedi = new System.Windows.Forms.Button();
            this.buttonEsci = new System.Windows.Forms.Button();
            this.buttonRegistrazione = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LabelEmail
            // 
            this.LabelEmail.AutoSize = true;//la label ridimensiona automaticamente la sua larghezza/altezza per adattarsi al testo
            this.LabelEmail.Location = new System.Drawing.Point(275, 177);
            this.LabelEmail.Name = "LabelEmail";
            this.LabelEmail.Size = new System.Drawing.Size(41, 16);//larghezza e altezza in pixel 
            this.LabelEmail.TabIndex = 0;//ordine di tabulazione quando l'utente preme tab. un valore piu basso indica che il controllo viene selezionato prima
            this.LabelEmail.Text = "Email";
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(249, 262);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(67, 16);
            this.labelPassword.TabIndex = 1;
            this.labelPassword.Text = "Password";
            // 
            // textBoxEmail
            // 
            this.textBoxEmail.Location = new System.Drawing.Point(331, 177);
            this.textBoxEmail.Name = "textBoxEmail";
            this.textBoxEmail.Size = new System.Drawing.Size(150, 22);
            this.textBoxEmail.TabIndex = 2;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(331, 259);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(150, 22);
            this.textBoxPassword.TabIndex = 3;
            // 
            // labelLOGO
            // 
            this.labelLOGO.AutoSize = true;
            this.labelLOGO.Font = new System.Drawing.Font("Cambria", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLOGO.ForeColor = System.Drawing.Color.SaddleBrown;
            this.labelLOGO.Location = new System.Drawing.Point(182, 48);
            this.labelLOGO.Name = "labelLOGO";
            this.labelLOGO.Size = new System.Drawing.Size(465, 70);
            this.labelLOGO.TabIndex = 4;
            this.labelLOGO.Text = "PARRUCCHIERIA";
            // 
            // buttonAccedi
            // 
            this.buttonAccedi.BackColor = System.Drawing.Color.LemonChiffon;
            this.buttonAccedi.Location = new System.Drawing.Point(269, 348);
            this.buttonAccedi.Name = "buttonAccedi";
            this.buttonAccedi.Size = new System.Drawing.Size(263, 41);
            this.buttonAccedi.TabIndex = 5;
            this.buttonAccedi.Text = "ACCEDI";
            this.buttonAccedi.UseVisualStyleBackColor = false;
            this.buttonAccedi.Click += new System.EventHandler(this.buttonAccedi_Click);
            // 
            // buttonEsci
            // 
            this.buttonEsci.BackColor = System.Drawing.Color.LemonChiffon;
            this.buttonEsci.Location = new System.Drawing.Point(736, 452);
            this.buttonEsci.Name = "buttonEsci";
            this.buttonEsci.Size = new System.Drawing.Size(75, 23);
            this.buttonEsci.TabIndex = 6;
            this.buttonEsci.Text = "ESCI";
            this.buttonEsci.UseVisualStyleBackColor = false;
            this.buttonEsci.Click += new System.EventHandler(this.buttonEsci_Click);
            // 
            // buttonRegistrazione
            // 
            this.buttonRegistrazione.BackColor = System.Drawing.Color.LemonChiffon;
            this.buttonRegistrazione.Location = new System.Drawing.Point(269, 418);
            this.buttonRegistrazione.Name = "buttonRegistrazione";
            this.buttonRegistrazione.Size = new System.Drawing.Size(263, 40);
            this.buttonRegistrazione.TabIndex = 7;
            this.buttonRegistrazione.Text = "CREA UN ACCOUNT";
            this.buttonRegistrazione.UseVisualStyleBackColor = false;
            this.buttonRegistrazione.Click += new System.EventHandler(this.buttonRegistrazione_Click);
            // 
            // form_login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;//indica come il form deve scalare i controlli. con font scala i controlli in base alla dimensione del font
            this.BackColor = System.Drawing.Color.Tan;
            this.ClientSize = new System.Drawing.Size(849, 518);
            this.Controls.Add(this.buttonRegistrazione);
            this.Controls.Add(this.buttonEsci);
            this.Controls.Add(this.buttonAccedi);
            this.Controls.Add(this.labelLOGO);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxEmail);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.LabelEmail);
            this.Name = "form_login";
            this.Text = "LOGIN";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.form_login_FormClosing);
            this.ResumeLayout(false);//Dopo aver finito di aggiungere tutti i controlli riattivo il layout.
            this.PerformLayout();//forza l’aggiornamento immediato del layout del form e dei controlli figli

        }

        #endregion

        private System.Windows.Forms.Label LabelEmail;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxEmail;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelLOGO;
        private System.Windows.Forms.Button buttonAccedi;
        private System.Windows.Forms.Button buttonEsci;
        private System.Windows.Forms.Button buttonRegistrazione;
    }
}