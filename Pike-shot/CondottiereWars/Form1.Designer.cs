using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using PikeAndShot;

namespace PikeAndShot
{
    partial class LevelConstructorForm : LevelEditorScreenListner
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
            this.levelComboBox = new System.Windows.Forms.ComboBox();
            this.levelTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.newLevelButton = new System.Windows.Forms.Button();
            this.formationTextBox = new System.Windows.Forms.TextBox();
            this.formationListBox = new System.Windows.Forms.ListBox();
            this.patternListBox = new System.Windows.Forms.ListBox();
            this.patternTextBox = new System.Windows.Forms.TextBox();
            this.newFormationButton = new System.Windows.Forms.Button();
            this.addFormationButton = new System.Windows.Forms.Button();
            this.removeFormationButton = new System.Windows.Forms.Button();
            this.newPatternButton = new System.Windows.Forms.Button();
            this.addPatternButton = new System.Windows.Forms.Button();
            this.removePatternButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.xTextBox = new System.Windows.Forms.TextBox();
            this.yTextBox = new System.Windows.Forms.TextBox();
            this.spawnTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.actionComboBox0 = new System.Windows.Forms.ComboBox();
            this.actionComboBox1 = new System.Windows.Forms.ComboBox();
            this.actionComboBox2 = new System.Windows.Forms.ComboBox();
            this.actionComboBox3 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.durationTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.soldierComboBox = new System.Windows.Forms.ComboBox();
            this.soldierListBox = new System.Windows.Forms.ListBox();
            this.removeSoldierButton = new System.Windows.Forms.Button();
            this.addSoldierButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.themeComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.terrainComboBox = new System.Windows.Forms.ComboBox();
            this.addTerrainButton = new System.Windows.Forms.Button();
            this.terrainListBox = new System.Windows.Forms.ListBox();
            this.removeTerrainButton = new System.Windows.Forms.Button();
            this.txTextBox = new System.Windows.Forms.TextBox();
            this.tyTextBox = new System.Windows.Forms.TextBox();
            this.tSpawnTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.movePatternUp = new System.Windows.Forms.Button();
            this.movePatternDown = new System.Windows.Forms.Button();
            this.moveFormationUp = new System.Windows.Forms.Button();
            this.moveFormationDown = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // levelComboBox
            // 
            this.levelComboBox.FormattingEnabled = true;
            this.levelComboBox.Location = new System.Drawing.Point(181, 29);
            this.levelComboBox.Name = "levelComboBox";
            this.levelComboBox.Size = new System.Drawing.Size(163, 21);
            this.levelComboBox.TabIndex = 0;
            this.levelComboBox.SelectedIndexChanged += new System.EventHandler(this.levelComboBox_SelectedIndexChanged);
            // 
            // levelTextBox
            // 
            this.levelTextBox.Location = new System.Drawing.Point(12, 29);
            this.levelTextBox.Name = "levelTextBox";
            this.levelTextBox.Size = new System.Drawing.Size(163, 20);
            this.levelTextBox.TabIndex = 1;
            this.levelTextBox.TextChanged += new System.EventHandler(this.levelTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Level";
            // 
            // newLevelButton
            // 
            this.newLevelButton.Location = new System.Drawing.Point(350, 27);
            this.newLevelButton.Name = "newLevelButton";
            this.newLevelButton.Size = new System.Drawing.Size(75, 23);
            this.newLevelButton.TabIndex = 3;
            this.newLevelButton.Text = "New";
            this.newLevelButton.UseVisualStyleBackColor = true;
            this.newLevelButton.Click += new System.EventHandler(this.newLevelButton_Click);
            // 
            // formationTextBox
            // 
            this.formationTextBox.Location = new System.Drawing.Point(12, 128);
            this.formationTextBox.Name = "formationTextBox";
            this.formationTextBox.Size = new System.Drawing.Size(163, 20);
            this.formationTextBox.TabIndex = 4;
            this.formationTextBox.TextChanged += new System.EventHandler(this.formationTextBox_TextChanged);
            // 
            // formationListBox
            // 
            this.formationListBox.FormattingEnabled = true;
            this.formationListBox.Location = new System.Drawing.Point(12, 155);
            this.formationListBox.Name = "formationListBox";
            this.formationListBox.Size = new System.Drawing.Size(163, 212);
            this.formationListBox.TabIndex = 5;
            this.formationListBox.SelectedIndexChanged += new System.EventHandler(this.formationListBox_SelectedIndexChanged);
            // 
            // patternListBox
            // 
            this.patternListBox.FormattingEnabled = true;
            this.patternListBox.Location = new System.Drawing.Point(275, 155);
            this.patternListBox.Name = "patternListBox";
            this.patternListBox.Size = new System.Drawing.Size(162, 212);
            this.patternListBox.TabIndex = 6;
            this.patternListBox.SelectedIndexChanged += new System.EventHandler(this.patternListBox_SelectedIndexChanged);
            // 
            // patternTextBox
            // 
            this.patternTextBox.Location = new System.Drawing.Point(275, 128);
            this.patternTextBox.Name = "patternTextBox";
            this.patternTextBox.Size = new System.Drawing.Size(162, 20);
            this.patternTextBox.TabIndex = 7;
            this.patternTextBox.TextChanged += new System.EventHandler(this.patternTextBox_TextChanged);
            // 
            // newFormationButton
            // 
            this.newFormationButton.Location = new System.Drawing.Point(12, 372);
            this.newFormationButton.Name = "newFormationButton";
            this.newFormationButton.Size = new System.Drawing.Size(49, 23);
            this.newFormationButton.TabIndex = 8;
            this.newFormationButton.Text = "New";
            this.newFormationButton.UseVisualStyleBackColor = true;
            this.newFormationButton.Click += new System.EventHandler(this.newFormationButton_Click);
            // 
            // addFormationButton
            // 
            this.addFormationButton.Location = new System.Drawing.Point(67, 372);
            this.addFormationButton.Name = "addFormationButton";
            this.addFormationButton.Size = new System.Drawing.Size(44, 23);
            this.addFormationButton.TabIndex = 9;
            this.addFormationButton.Text = "Add";
            this.addFormationButton.UseVisualStyleBackColor = true;
            this.addFormationButton.Click += new System.EventHandler(this.addFormationButton_Click);
            // 
            // removeFormationButton
            // 
            this.removeFormationButton.Location = new System.Drawing.Point(117, 372);
            this.removeFormationButton.Name = "removeFormationButton";
            this.removeFormationButton.Size = new System.Drawing.Size(58, 23);
            this.removeFormationButton.TabIndex = 10;
            this.removeFormationButton.Text = "Remove";
            this.removeFormationButton.UseVisualStyleBackColor = true;
            this.removeFormationButton.Click += new System.EventHandler(this.removeFormationButton_Click);
            // 
            // newPatternButton
            // 
            this.newPatternButton.Location = new System.Drawing.Point(275, 372);
            this.newPatternButton.Name = "newPatternButton";
            this.newPatternButton.Size = new System.Drawing.Size(46, 23);
            this.newPatternButton.TabIndex = 11;
            this.newPatternButton.Text = "New";
            this.newPatternButton.UseVisualStyleBackColor = true;
            this.newPatternButton.Click += new System.EventHandler(this.newPatternButton_Click);
            // 
            // addPatternButton
            // 
            this.addPatternButton.Location = new System.Drawing.Point(327, 372);
            this.addPatternButton.Name = "addPatternButton";
            this.addPatternButton.Size = new System.Drawing.Size(46, 23);
            this.addPatternButton.TabIndex = 12;
            this.addPatternButton.Text = "Add";
            this.addPatternButton.UseVisualStyleBackColor = true;
            this.addPatternButton.Click += new System.EventHandler(this.addPatternButton_Click);
            // 
            // removePatternButton
            // 
            this.removePatternButton.Location = new System.Drawing.Point(379, 372);
            this.removePatternButton.Name = "removePatternButton";
            this.removePatternButton.Size = new System.Drawing.Size(58, 23);
            this.removePatternButton.TabIndex = 13;
            this.removePatternButton.Text = "Remove";
            this.removePatternButton.UseVisualStyleBackColor = true;
            this.removePatternButton.Click += new System.EventHandler(this.removePatternButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 109);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Formations";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(275, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Patterns";
            // 
            // xTextBox
            // 
            this.xTextBox.Location = new System.Drawing.Point(216, 155);
            this.xTextBox.Name = "xTextBox";
            this.xTextBox.Size = new System.Drawing.Size(53, 20);
            this.xTextBox.TabIndex = 16;
            this.xTextBox.TextChanged += new System.EventHandler(this.xTextBox_TextChanged);
            // 
            // yTextBox
            // 
            this.yTextBox.Location = new System.Drawing.Point(216, 182);
            this.yTextBox.Name = "yTextBox";
            this.yTextBox.Size = new System.Drawing.Size(53, 20);
            this.yTextBox.TabIndex = 17;
            this.yTextBox.TextChanged += new System.EventHandler(this.yTextBox_TextChanged);
            // 
            // spawnTextBox
            // 
            this.spawnTextBox.Location = new System.Drawing.Point(216, 209);
            this.spawnTextBox.Name = "spawnTextBox";
            this.spawnTextBox.Size = new System.Drawing.Size(53, 20);
            this.spawnTextBox.TabIndex = 18;
            this.spawnTextBox.TextChanged += new System.EventHandler(this.spawnTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(196, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "X";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(196, 185);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Y";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(178, 212);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Spawn";
            // 
            // actionComboBox0
            // 
            this.actionComboBox0.FormattingEnabled = true;
            this.actionComboBox0.Location = new System.Drawing.Point(444, 155);
            this.actionComboBox0.Name = "actionComboBox0";
            this.actionComboBox0.Size = new System.Drawing.Size(121, 21);
            this.actionComboBox0.TabIndex = 22;
            this.actionComboBox0.SelectedIndexChanged += new System.EventHandler(this.actionComboBox0_SelectedIndexChanged);
            // 
            // actionComboBox1
            // 
            this.actionComboBox1.FormattingEnabled = true;
            this.actionComboBox1.Location = new System.Drawing.Point(444, 185);
            this.actionComboBox1.Name = "actionComboBox1";
            this.actionComboBox1.Size = new System.Drawing.Size(121, 21);
            this.actionComboBox1.TabIndex = 23;
            this.actionComboBox1.SelectedIndexChanged += new System.EventHandler(this.actionComboBox1_SelectedIndexChanged);
            // 
            // actionComboBox2
            // 
            this.actionComboBox2.FormattingEnabled = true;
            this.actionComboBox2.Location = new System.Drawing.Point(444, 213);
            this.actionComboBox2.Name = "actionComboBox2";
            this.actionComboBox2.Size = new System.Drawing.Size(121, 21);
            this.actionComboBox2.TabIndex = 24;
            this.actionComboBox2.SelectedIndexChanged += new System.EventHandler(this.actionComboBox2_SelectedIndexChanged);
            // 
            // actionComboBox3
            // 
            this.actionComboBox3.FormattingEnabled = true;
            this.actionComboBox3.Location = new System.Drawing.Point(444, 241);
            this.actionComboBox3.Name = "actionComboBox3";
            this.actionComboBox3.Size = new System.Drawing.Size(121, 21);
            this.actionComboBox3.TabIndex = 25;
            this.actionComboBox3.SelectedIndexChanged += new System.EventHandler(this.actionComboBox3_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(444, 136);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 13);
            this.label7.TabIndex = 26;
            this.label7.Text = "Actions";
            // 
            // durationTextBox
            // 
            this.durationTextBox.Location = new System.Drawing.Point(444, 281);
            this.durationTextBox.Name = "durationTextBox";
            this.durationTextBox.Size = new System.Drawing.Size(121, 20);
            this.durationTextBox.TabIndex = 27;
            this.durationTextBox.TextChanged += new System.EventHandler(this.durationTextBox_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(444, 265);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 28;
            this.label8.Text = "Duration";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(577, 109);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "Soldier";
            // 
            // soldierComboBox
            // 
            this.soldierComboBox.FormattingEnabled = true;
            this.soldierComboBox.Location = new System.Drawing.Point(580, 128);
            this.soldierComboBox.Name = "soldierComboBox";
            this.soldierComboBox.Size = new System.Drawing.Size(151, 21);
            this.soldierComboBox.TabIndex = 30;
            // 
            // soldierListBox
            // 
            this.soldierListBox.FormattingEnabled = true;
            this.soldierListBox.Location = new System.Drawing.Point(580, 156);
            this.soldierListBox.Name = "soldierListBox";
            this.soldierListBox.Size = new System.Drawing.Size(192, 212);
            this.soldierListBox.TabIndex = 31;
            // 
            // removeSoldierButton
            // 
            this.removeSoldierButton.Location = new System.Drawing.Point(580, 374);
            this.removeSoldierButton.Name = "removeSoldierButton";
            this.removeSoldierButton.Size = new System.Drawing.Size(192, 23);
            this.removeSoldierButton.TabIndex = 34;
            this.removeSoldierButton.Text = "Remove";
            this.removeSoldierButton.UseVisualStyleBackColor = true;
            this.removeSoldierButton.Click += new System.EventHandler(this.removeSoldierButton_Click);
            // 
            // addSoldierButton
            // 
            this.addSoldierButton.Location = new System.Drawing.Point(737, 126);
            this.addSoldierButton.Name = "addSoldierButton";
            this.addSoldierButton.Size = new System.Drawing.Size(35, 23);
            this.addSoldierButton.TabIndex = 33;
            this.addSoldierButton.Text = "Add";
            this.addSoldierButton.UseVisualStyleBackColor = true;
            this.addSoldierButton.Click += new System.EventHandler(this.addSoldierButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(431, 20);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(134, 37);
            this.saveButton.TabIndex = 35;
            this.saveButton.Text = "SAVE";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // themeComboBox
            // 
            this.themeComboBox.FormattingEnabled = true;
            this.themeComboBox.Location = new System.Drawing.Point(12, 495);
            this.themeComboBox.Name = "themeComboBox";
            this.themeComboBox.Size = new System.Drawing.Size(163, 21);
            this.themeComboBox.TabIndex = 36;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 479);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(69, 13);
            this.label10.TabIndex = 37;
            this.label10.Text = "Level Theme";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 523);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 13);
            this.label11.TabIndex = 38;
            this.label11.Text = "Terrain";
            // 
            // terrainComboBox
            // 
            this.terrainComboBox.FormattingEnabled = true;
            this.terrainComboBox.Location = new System.Drawing.Point(12, 540);
            this.terrainComboBox.Name = "terrainComboBox";
            this.terrainComboBox.Size = new System.Drawing.Size(121, 21);
            this.terrainComboBox.TabIndex = 39;
            // 
            // addTerrainButton
            // 
            this.addTerrainButton.Location = new System.Drawing.Point(140, 540);
            this.addTerrainButton.Name = "addTerrainButton";
            this.addTerrainButton.Size = new System.Drawing.Size(35, 23);
            this.addTerrainButton.TabIndex = 40;
            this.addTerrainButton.Text = "Add";
            this.addTerrainButton.UseVisualStyleBackColor = true;
            this.addTerrainButton.Click += new System.EventHandler(this.addTerrainButton_Click);
            // 
            // terrainListBox
            // 
            this.terrainListBox.FormattingEnabled = true;
            this.terrainListBox.Location = new System.Drawing.Point(12, 568);
            this.terrainListBox.Name = "terrainListBox";
            this.terrainListBox.Size = new System.Drawing.Size(163, 160);
            this.terrainListBox.TabIndex = 41;
            this.terrainListBox.SelectedIndexChanged += new System.EventHandler(this.terrainListBox_SelectedIndexChanged);
            // 
            // removeTerrainButton
            // 
            this.removeTerrainButton.Location = new System.Drawing.Point(12, 734);
            this.removeTerrainButton.Name = "removeTerrainButton";
            this.removeTerrainButton.Size = new System.Drawing.Size(163, 23);
            this.removeTerrainButton.TabIndex = 42;
            this.removeTerrainButton.Text = "Remove";
            this.removeTerrainButton.UseVisualStyleBackColor = true;
            this.removeTerrainButton.Click += new System.EventHandler(this.removeTerrainButton_Click);
            // 
            // txTextBox
            // 
            this.txTextBox.Location = new System.Drawing.Point(216, 568);
            this.txTextBox.Name = "txTextBox";
            this.txTextBox.Size = new System.Drawing.Size(53, 20);
            this.txTextBox.TabIndex = 43;
            this.txTextBox.TextChanged += new System.EventHandler(this.txTextBox_TextChanged);
            // 
            // tyTextBox
            // 
            this.tyTextBox.Location = new System.Drawing.Point(216, 595);
            this.tyTextBox.Name = "tyTextBox";
            this.tyTextBox.Size = new System.Drawing.Size(53, 20);
            this.tyTextBox.TabIndex = 44;
            this.tyTextBox.TextChanged += new System.EventHandler(this.tyTextBox_TextChanged);
            // 
            // tSpawnTextBox
            // 
            this.tSpawnTextBox.Location = new System.Drawing.Point(216, 622);
            this.tSpawnTextBox.Name = "tSpawnTextBox";
            this.tSpawnTextBox.Size = new System.Drawing.Size(53, 20);
            this.tSpawnTextBox.TabIndex = 45;
            this.tSpawnTextBox.TextChanged += new System.EventHandler(this.tSpawnTextBox_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(196, 571);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(14, 13);
            this.label12.TabIndex = 46;
            this.label12.Text = "X";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(196, 598);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(14, 13);
            this.label13.TabIndex = 47;
            this.label13.Text = "Y";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(178, 625);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(40, 13);
            this.label14.TabIndex = 48;
            this.label14.Text = "Spawn";
            // 
            // movePatternUp
            // 
            this.movePatternUp.Location = new System.Drawing.Point(275, 402);
            this.movePatternUp.Name = "movePatternUp";
            this.movePatternUp.Size = new System.Drawing.Size(78, 23);
            this.movePatternUp.TabIndex = 49;
            this.movePatternUp.Text = "move up";
            this.movePatternUp.UseVisualStyleBackColor = true;
            this.movePatternUp.Click += new System.EventHandler(this.movePatternUp_Click);
            // 
            // movePatternDown
            // 
            this.movePatternDown.Location = new System.Drawing.Point(360, 402);
            this.movePatternDown.Name = "movePatternDown";
            this.movePatternDown.Size = new System.Drawing.Size(75, 23);
            this.movePatternDown.TabIndex = 50;
            this.movePatternDown.Text = "move down";
            this.movePatternDown.UseVisualStyleBackColor = true;
            this.movePatternDown.Click += new System.EventHandler(this.movePatternDown_Click);
            // 
            // moveFormationUp
            // 
            this.moveFormationUp.Location = new System.Drawing.Point(12, 402);
            this.moveFormationUp.Name = "moveFormationUp";
            this.moveFormationUp.Size = new System.Drawing.Size(75, 23);
            this.moveFormationUp.TabIndex = 51;
            this.moveFormationUp.Text = "move up";
            this.moveFormationUp.UseVisualStyleBackColor = true;
            this.moveFormationUp.Click += new System.EventHandler(this.moveFormationUp_Click);
            // 
            // moveFormationDown
            // 
            this.moveFormationDown.Location = new System.Drawing.Point(100, 402);
            this.moveFormationDown.Name = "moveFormationDown";
            this.moveFormationDown.Size = new System.Drawing.Size(75, 23);
            this.moveFormationDown.TabIndex = 52;
            this.moveFormationDown.Text = "move down";
            this.moveFormationDown.UseVisualStyleBackColor = true;
            this.moveFormationDown.Click += new System.EventHandler(this.moveFormationDown_Click);
            // 
            // LevelConstructorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(789, 746);
            this.Controls.Add(this.moveFormationDown);
            this.Controls.Add(this.moveFormationUp);
            this.Controls.Add(this.movePatternDown);
            this.Controls.Add(this.movePatternUp);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tSpawnTextBox);
            this.Controls.Add(this.tyTextBox);
            this.Controls.Add(this.txTextBox);
            this.Controls.Add(this.removeTerrainButton);
            this.Controls.Add(this.terrainListBox);
            this.Controls.Add(this.addTerrainButton);
            this.Controls.Add(this.terrainComboBox);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.themeComboBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.removeSoldierButton);
            this.Controls.Add(this.addSoldierButton);
            this.Controls.Add(this.soldierListBox);
            this.Controls.Add(this.soldierComboBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.durationTextBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.actionComboBox3);
            this.Controls.Add(this.actionComboBox2);
            this.Controls.Add(this.actionComboBox1);
            this.Controls.Add(this.actionComboBox0);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.spawnTextBox);
            this.Controls.Add(this.yTextBox);
            this.Controls.Add(this.xTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.removePatternButton);
            this.Controls.Add(this.addPatternButton);
            this.Controls.Add(this.newPatternButton);
            this.Controls.Add(this.removeFormationButton);
            this.Controls.Add(this.addFormationButton);
            this.Controls.Add(this.newFormationButton);
            this.Controls.Add(this.patternTextBox);
            this.Controls.Add(this.patternListBox);
            this.Controls.Add(this.formationListBox);
            this.Controls.Add(this.formationTextBox);
            this.Controls.Add(this.newLevelButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.levelTextBox);
            this.Controls.Add(this.levelComboBox);
            this.Name = "LevelConstructorForm";
            this.Text = "Level Constructor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox levelComboBox;
        private System.Windows.Forms.TextBox levelTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button newLevelButton;
        private System.Windows.Forms.TextBox formationTextBox;
        private System.Windows.Forms.ListBox formationListBox;
        private System.Windows.Forms.ListBox patternListBox;
        private System.Windows.Forms.TextBox patternTextBox;
        private System.Windows.Forms.Button newFormationButton;
        private System.Windows.Forms.Button addFormationButton;
        private System.Windows.Forms.Button removeFormationButton;
        private System.Windows.Forms.Button newPatternButton;
        private System.Windows.Forms.Button addPatternButton;
        private System.Windows.Forms.Button removePatternButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox xTextBox;
        private System.Windows.Forms.TextBox yTextBox;
        private System.Windows.Forms.TextBox spawnTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox actionComboBox0;
        private System.Windows.Forms.ComboBox actionComboBox1;
        private System.Windows.Forms.ComboBox actionComboBox2;
        private System.Windows.Forms.ComboBox actionComboBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox durationTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox soldierComboBox;
        private System.Windows.Forms.ListBox soldierListBox;
        private System.Windows.Forms.Button removeSoldierButton;
        private System.Windows.Forms.Button addSoldierButton;
        private System.Windows.Forms.Button saveButton;
        private ComboBox themeComboBox;
        private Label label10;
        private Label label11;
        private ComboBox terrainComboBox;
        private Button addTerrainButton;
        private ListBox terrainListBox;
        private Button removeTerrainButton;
        private TextBox txTextBox;
        private TextBox tyTextBox;
        private TextBox tSpawnTextBox;
        private Label label12;
        private Label label13;
        private Label label14;
        private Button movePatternUp;
        private Button movePatternDown;
        private Button moveFormationUp;
        private Button moveFormationDown;
    }
}

