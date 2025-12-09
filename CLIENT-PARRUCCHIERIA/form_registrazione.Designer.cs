using System.Security.Cryptography;
using System.Windows.Forms;

namespace CLIENT_PARRUCCHIERIA
{
    partial class form_registrazione
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;//components è una variabile di tipo IContainer, usata per tenere traccia di tutti i controlli e componenti aggiunti al form (bottoni, label, textBox, ecc.).

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelNome = new System.Windows.Forms.Label();
            this.textBoxNome = new System.Windows.Forms.TextBox();
            this.textBoxCognome = new System.Windows.Forms.TextBox();
            this.textBoxEmail = new System.Windows.Forms.TextBox();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelCognome = new System.Windows.Forms.Label();
            this.labelEmail = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxConfPassword = new System.Windows.Forms.TextBox();
            this.labelConfPassword = new System.Windows.Forms.Label();
            this.buttonRegister = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonEsci = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelNome
            // 
            this.labelNome.AutoSize = true;
            this.labelNome.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelNome.Location = new System.Drawing.Point(444, 152);
            this.labelNome.Name = "labelNome";
            this.labelNome.Size = new System.Drawing.Size(52, 16);
            this.labelNome.TabIndex = 0;
            this.labelNome.Text = "Nome *";
            // 
            // textBoxNome
            // 
            this.textBoxNome.Location = new System.Drawing.Point(502, 146);
            this.textBoxNome.Name = "textBoxNome";
            this.textBoxNome.Size = new System.Drawing.Size(129, 22);
            this.textBoxNome.TabIndex = 1;
            // 
            // textBoxCognome
            // 
            this.textBoxCognome.Location = new System.Drawing.Point(502, 227);
            this.textBoxCognome.Name = "textBoxCognome";
            this.textBoxCognome.Size = new System.Drawing.Size(129, 22);
            this.textBoxCognome.TabIndex = 2;
            // 
            // textBoxEmail
            // 
            this.textBoxEmail.Location = new System.Drawing.Point(502, 305);
            this.textBoxEmail.Name = "textBoxEmail";
            this.textBoxEmail.Size = new System.Drawing.Size(129, 22);
            this.textBoxEmail.TabIndex = 3;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(503, 379);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(128, 22);
            this.textBoxPassword.TabIndex = 4;
            // 
            // labelCognome
            // 
            this.labelCognome.AutoSize = true;
            this.labelCognome.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelCognome.Location = new System.Drawing.Point(422, 233);
            this.labelCognome.Name = "labelCognome";
            this.labelCognome.Size = new System.Drawing.Size(74, 16);
            this.labelCognome.TabIndex = 5;
            this.labelCognome.Text = "Cognome *";
            // 
            // labelEmail
            // 
            this.labelEmail.AutoSize = true;
            this.labelEmail.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelEmail.Location = new System.Drawing.Point(448, 311);
            this.labelEmail.Name = "labelEmail";
            this.labelEmail.Size = new System.Drawing.Size(48, 16);
            this.labelEmail.TabIndex = 6;
            this.labelEmail.Text = "email *";
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelPassword.Location = new System.Drawing.Point(421, 385);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(75, 16);
            this.labelPassword.TabIndex = 7;
            this.labelPassword.Text = "Password *";
            // 
            // textBoxConfPassword
            // 
            this.textBoxConfPassword.Location = new System.Drawing.Point(503, 452);
            this.textBoxConfPassword.Name = "textBoxConfPassword";
            this.textBoxConfPassword.PasswordChar = '*';
            this.textBoxConfPassword.Size = new System.Drawing.Size(129, 22);
            this.textBoxConfPassword.TabIndex = 8;
            // 
            // labelConfPassword
            // 
            this.labelConfPassword.AutoSize = true;
            this.labelConfPassword.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelConfPassword.Location = new System.Drawing.Point(361, 458);
            this.labelConfPassword.Name = "labelConfPassword";
            this.labelConfPassword.Size = new System.Drawing.Size(135, 16);
            this.labelConfPassword.TabIndex = 9;
            this.labelConfPassword.Text = "Conferma password *";
            // 
            // buttonRegister
            // 
            this.buttonRegister.BackColor = System.Drawing.Color.LemonChiffon;
            this.buttonRegister.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.buttonRegister.Location = new System.Drawing.Point(407, 510);
            this.buttonRegister.Name = "buttonRegister";
            this.buttonRegister.Size = new System.Drawing.Size(284, 58);
            this.buttonRegister.TabIndex = 10;
            this.buttonRegister.Text = "REGISTRATI";
            this.buttonRegister.UseVisualStyleBackColor = false;
            this.buttonRegister.Click += new System.EventHandler(this.buttonRegister_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Cambria", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.SaddleBrown;
            this.label1.Location = new System.Drawing.Point(316, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(465, 70);
            this.label1.TabIndex = 11;
            this.label1.Text = "PARRUCCHIERIA";
            // 
            // buttonEsci
            // 
            this.buttonEsci.BackColor = System.Drawing.Color.LemonChiffon;
            this.buttonEsci.Location = new System.Drawing.Point(1010, 571);
            this.buttonEsci.Name = "buttonEsci";
            this.buttonEsci.Size = new System.Drawing.Size(90, 33);
            this.buttonEsci.TabIndex = 12;
            this.buttonEsci.Text = "ESCI";
            this.buttonEsci.UseVisualStyleBackColor = false;
            this.buttonEsci.Click += new System.EventHandler(this.buttonEsci_Click);
            // 
            // form_registrazione
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;//indica come il form deve scalare i controlli. con font scala i controlli in base alla dimensione del font
            this.BackColor = System.Drawing.Color.Tan;
            this.ClientSize = new System.Drawing.Size(1112, 616);
            this.Controls.Add(this.buttonEsci);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonRegister);
            this.Controls.Add(this.labelConfPassword);
            this.Controls.Add(this.textBoxConfPassword);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.labelEmail);
            this.Controls.Add(this.labelCognome);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxEmail);
            this.Controls.Add(this.textBoxCognome);
            this.Controls.Add(this.textBoxNome);
            this.Controls.Add(this.labelNome);
            this.Name = "form_registrazione";
            this.Text = "CREA ACCOUNT";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.form_registrazione_FormClosing);
            this.ResumeLayout(false);//Dopo aver finito di aggiungere tutti i controlli riattivo il layout.
            this.PerformLayout(); //forza l’aggiornamento immediato del layout del form e dei controlli figli

        }

        #endregion

        private System.Windows.Forms.Label labelNome;
        private System.Windows.Forms.TextBox textBoxNome;
        private System.Windows.Forms.TextBox textBoxCognome;
        private System.Windows.Forms.TextBox textBoxEmail;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelCognome;
        private System.Windows.Forms.Label labelEmail;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxConfPassword;
        private System.Windows.Forms.Label labelConfPassword;
        private System.Windows.Forms.Button buttonRegister;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonEsci;
    }
}

