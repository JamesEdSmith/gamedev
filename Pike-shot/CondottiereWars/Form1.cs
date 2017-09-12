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
using Microsoft.Xna.Framework.Content;
using PikeAndShot;
using System.Collections;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace PikeAndShot
{

    public partial class LevelConstructorForm : Form, LevelEditorScreenListner
    {        
        private List<PikeAndShot.Level> _levels;
        private List<SoldierClass> _soldierClass;
        private List<ActionClass> _actionClass;
        private List<TerrainClass> _terrainClass;
        private int currLevel;
        private int currFormation;
        private int currPatternAction;
        private int currTerrain;
        private List<FormListener> _formListeners;
        private int copyTally;
        private Level copiedLevel;

        public LevelConstructorForm(List<Level>levels)
        {
            InitializeComponent();
            _levels = new List<PikeAndShot.Level>(10);
            _soldierClass = new List<SoldierClass>(32);
            _actionClass = new List<ActionClass>(32);
            _terrainClass = new List<TerrainClass>(32);
            _formListeners = new List<FormListener>(3);
            _levels = levels;
            copyTally = 0;
            loadLevels();
            initControls();
        }

        public PikeAndShot.Level getCurrLevel()
        {
            return _levels[currLevel];
        }

        public void addFormListener(FormListener listener)
        {
            _formListeners.Add(listener);
            sendUpdate();
        }

        private void sendUpdate()
        {
            foreach(FormListener fl in _formListeners)
                fl.updateLevel(_levels[currLevel], currFormation);
        }

        private void sendUpdateTerrain()
        {
            foreach (FormListener fl in _formListeners)
                fl.updateLevel(_levels[currLevel], -1);
        }

        private void loadLevels()
        {
            //load soldier classes and fill the comboBox with them
            SoldierClass soldier = new SoldierClass("Merc Pikeman", 0);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Merc Arquebusier", 1);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Merc Crossbowman", 2);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Merc Crossbowman Pavise", 3);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Merc Doppelsoldier", 4);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Merc Soldier", 5);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Merc Cavalry", 6);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Goblin Slinger", 10);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Goblin Berzerker", 11);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Goblin Brigand", 12);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Goblin Hauler", 13);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("Goblin Wolf", 14);
            _soldierClass.Add(soldier);
            soldier = new SoldierClass("NPCFleer", 20);
            _soldierClass.Add(soldier);

            foreach (SoldierClass sc in _soldierClass)
            {
                soldierComboBox.Items.Add(sc.id.ToString() + " " + sc.name);
            }

            //load Pattern Actions and fill each of the comboboxes with them
            ActionClass action = new ActionClass("Idle", 0);
            _actionClass.Add(action);
            action = new ActionClass("Left", 1);
            _actionClass.Add(action);
            action = new ActionClass("Up", 2);
            _actionClass.Add(action);
            action = new ActionClass("Right", 3);
            _actionClass.Add(action);
            action = new ActionClass("Down", 4);
            _actionClass.Add(action);
            action = new ActionClass("Pike", 5);
            _actionClass.Add(action);
            action = new ActionClass("Raise", 6);
            _actionClass.Add(action);
            action = new ActionClass("Shot", 7);
            _actionClass.Add(action);
            action = new ActionClass("Cavalry Halt", 8);
            _actionClass.Add(action);
            action = new ActionClass("Cavalry Turn", 9);
            _actionClass.Add(action);
            action = new ActionClass("Charge", 10);
            _actionClass.Add(action);
            action = new ActionClass("Reload", 11);
            _actionClass.Add(action);
            action = new ActionClass("Spawn", 12);
            _actionClass.Add(action);

            foreach (ActionClass sc in _actionClass)
            {
                actionComboBox0.Items.Add(sc.id.ToString() + " " + sc.name);
                actionComboBox1.Items.Add(sc.id.ToString() + " " + sc.name);
                actionComboBox2.Items.Add(sc.id.ToString() + " " + sc.name);
                actionComboBox3.Items.Add(sc.id.ToString() + " " + sc.name);
            }

            TerrainClass terrain = new TerrainClass("tree 0", 0);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("hori road", 1);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("turn road", 2);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("road mile marker", 3);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("road turn marker", 4);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("hori road 2", 5);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("tree 1", 6);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("tree 2", 7);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("bush 1", 8);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("bush 2", 9);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("bush 3", 10);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("wagon", 11);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("ox brown", 12);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("ox grey", 13);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("ox dead", 14);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("peasant wounded", 15);
            _terrainClass.Add(terrain);
            terrain = new TerrainClass("peasant dead", 16);
            _terrainClass.Add(terrain);

            foreach (TerrainClass sc in _terrainClass)
            {
                terrainComboBox.Items.Add(sc.id.ToString() + " " + sc.name);
            }

            formationSideComboBox.Items.Add("-1 ENEMY");
            formationSideComboBox.Items.Add("0 NEUTRAL");
            formationSideComboBox.Items.Add("1 PLAYER");
        }

        private void initControls()
        {
            foreach (Level l in _levels)
            {
                levelComboBox.Items.Add(l.levelName);
            }
            levelComboBox.SelectedIndex = 0;
            levelComboBox_SelectedIndexChanged(null, null);
        }

        private void levelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currLevel = levelComboBox.SelectedIndex;
            levelTextBox.Text = _levels[currLevel].levelName;
            formationListBox.Items.Clear();

            foreach (string s in _levels[currLevel].formationNames)
            {
                formationListBox.Items.Add(s);
            }
            refreshLevel();
        }

        private void refreshLevel()
        {
            refreshFormationListBox();
            refreshPatternListBox();
            refreshTerrainListBox();
        }

        private void formationListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currFormation = formationListBox.SelectedIndex;

            if (currFormation != -1)
            {
                xTextBox.Text = _levels[currLevel].formationPositions[currFormation].X.ToString();
                yTextBox.Text = _levels[currLevel].formationPositions[currFormation].Y.ToString();
                spawnTextBox.Text = _levels[currLevel].formationTimes[currFormation].ToString();
                formationTextBox.Text = _levels[currLevel].formationNames[currFormation];
                formationSideComboBox.SelectedIndex = _levels[currLevel].formationSides[currFormation] + 1;

                // populate the soldier list for this formation
                soldierListBox.Items.Clear();
                Predicate<SoldierClass> predicate;
                foreach (int s in _levels[currLevel].formations[currFormation])
                {
                    predicate = delegate(SoldierClass arg)
                       {
                           return arg.id == s;
                       };
                    soldierListBox.Items.Add(_soldierClass.Find(predicate).id.ToString() + " " + _soldierClass.Find(predicate).name);
                }

                //populate the patternaction list for this formation
                patternListBox.Items.Clear();
                foreach (string pa in _levels[currLevel].formationActionNames[currFormation])
                {
                    patternListBox.Items.Add(pa);
                }
                if (patternListBox.Items.Count > 0)
                {
                    currPatternAction = 0;
                    patternListBox.SelectedIndex = 0;
                    patternListBox_SelectedIndexChanged(sender, e);
                }
            }
        }

        private void xTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currFormation != -1)
            {
                try
                {
                    _levels[currLevel].formationPositions[currFormation] = new Vector2(System.Convert.ToSingle(xTextBox.Text), _levels[currLevel].formationPositions[currFormation].Y);
                }
                catch
                {
                    _levels[currLevel].formationPositions[currFormation] = new Vector2(0f, _levels[currLevel].formationPositions[currFormation].Y);
                }
            }

            sendUpdate();
        }

        private void yTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currFormation != -1)
            {
                try
                {
                    _levels[currLevel].formationPositions[currFormation] = new Vector2(_levels[currLevel].formationPositions[currFormation].X, System.Convert.ToSingle(yTextBox.Text));
                }
                catch
                {
                    _levels[currLevel].formationPositions[currFormation] = new Vector2(_levels[currLevel].formationPositions[currFormation].X, 0f);
                }
            }
            sendUpdate();
        }

        private void spawnTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currFormation != -1)
            {
                try
                {
                    _levels[currLevel].formationTimes[currFormation] = System.Convert.ToDouble(spawnTextBox.Text);
                }
                catch
                {
                    _levels[currLevel].formationTimes[currFormation] = 0f;
                }
            }

            sendUpdate();
        }

        private void removeFormationButton_Click(object sender, EventArgs e)
        {
            if (currFormation != -1 && currFormation < formationListBox.Items.Count)
            {
                removeFormation(currFormation);
                formationListBox.Items.RemoveAt(currFormation);
            }

            sendUpdate();
        }

        private void removeFormation(int currFormation)
        {
            _levels[currLevel].formationActionNames.RemoveAt(currFormation);
            _levels[currLevel].formationActions.RemoveAt(currFormation);
            _levels[currLevel].formationPositions.RemoveAt(currFormation);
            _levels[currLevel].formations.RemoveAt(currFormation);
            _levels[currLevel].formationTimes.RemoveAt(currFormation);
            _levels[currLevel].formationNames.RemoveAt(currFormation);

            currFormation = -1;
            currPatternAction = -1;
        }

        private void formationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (formationListBox.SelectedIndex != -1)
            {
                _levels[currLevel].formationNames[currFormation] = formationTextBox.Text;

                //refresh the list box
                formationListBox.Items.Clear();
                foreach (string s in _levels[currLevel].formationNames)
                    formationListBox.Items.Add(s);

                formationListBox.SelectedIndex = currFormation;
            }

            sendUpdate();
        }

        private void levelTextBox_TextChanged(object sender, EventArgs e)
        {
            _levels[currLevel].levelName = levelTextBox.Text;
            levelComboBox.Items.Clear();

            foreach (Level l in _levels)
                levelComboBox.Items.Add(l.levelName);

            levelComboBox.SelectedIndex = currLevel;

            sendUpdate();
        }

        private void newFormationButton_Click(object sender, EventArgs e)
        {
            _levels[currLevel].formationNames.Add("New Formation");
            _levels[currLevel].formationPositions.Add(new Vector2(1, 1));
            _levels[currLevel].formationTimes.Add(0f);
            List<PatternAction> formationActions = new List<PatternAction>(10);
            List<int> actions = new List<int>(10);
            actions.Add(0);
            formationActions.Add(new PatternAction(actions,1000));
            _levels[currLevel].formationActions.Add(formationActions);
            _levels[currLevel].formations.Add(new List<int>(10));
            List<string> formationActionNames = new List<string>(10);
            formationActionNames.Add("New Action");
            _levels[currLevel].formationActionNames.Add(formationActionNames);
            _levels[currLevel].formationSides.Add(-1);

            refreshFormationListBox();

            sendUpdate();
        }

        private void refreshFormationListBox()
        {
            formationListBox.Items.Clear();

            if (currFormation >= _levels[currLevel].formations.Count)
            {
                currFormation = 0;
            }

            foreach (string s in _levels[currLevel].formationNames)
                formationListBox.Items.Add(s);

            formationListBox.SelectedIndex = currFormation;

            if (_levels[currLevel].formations != null && _levels[currLevel].formationSides.Count > currFormation)
                formationSideComboBox.SelectedIndex = _levels[currLevel].formationSides[currFormation] + 1;

            refreshSoldierListBox();
        }

        private void addFormationButton_Click(object sender, EventArgs e)
        {
            _levels[currLevel].formationNames.Add(_levels[currLevel].formationNames[currFormation]);
            _levels[currLevel].formationPositions.Add(_levels[currLevel].formationPositions[currFormation]);
            _levels[currLevel].formationTimes.Add(_levels[currLevel].formationTimes[currFormation]);            

            List<int> formation = new List<int>(10);
            foreach(int i in _levels[currLevel].formations[currFormation])
                formation.Add(i);

            _levels[currLevel].formations.Add(formation);

            List<string>formationActionNames = new List<string>(32);
            foreach (string s in _levels[currLevel].formationActionNames[currFormation])
                formationActionNames.Add(s);

            _levels[currLevel].formationActionNames.Add(formationActionNames);

            List<PatternAction> patternActions = new List<PatternAction>(32);
            List<int> actions;
            foreach (PatternAction pa in _levels[currLevel].formationActions[currFormation])
            {
                actions = new List<int>(32);
                foreach(int action in pa.actions)
                    actions.Add(action);

                PatternAction newPa = new PatternAction(actions, pa.duration);
                patternActions.Add(newPa);
            }

            _levels[currLevel].formationActions.Add(patternActions);
            _levels[currLevel].formationSides.Add(_levels[currLevel].formationSides[currFormation]);

            refreshFormationListBox();

            sendUpdate();
        }

        private void newLevelButton_Click(object sender, EventArgs e)
        {
            Level newLevel = new Level();
            newLevel.levelName = "New Level";
            _levels.Add(newLevel);

            refreshLevelComboBox();
            refreshLevel();

            sendUpdate();
        }

        private void refreshLevelComboBox()
        {
            currLevel = _levels.Count - 1;

            levelComboBox.Items.Clear();

            foreach (Level l in _levels)
                levelComboBox.Items.Add(l.levelName);

            levelComboBox.SelectedIndex = currLevel;
        }

        private void addSoldierButton_Click(object sender, EventArgs e)
        {
            if (soldierComboBox.SelectedIndex != -1)
            {
                _levels[currLevel].formations[currFormation].Add(_soldierClass[soldierComboBox.SelectedIndex].id);
                soldierListBox.Items.Add(_soldierClass[soldierComboBox.SelectedIndex].id.ToString() + " " + _soldierClass[soldierComboBox.SelectedIndex].name);
            }
            sendUpdate();
        }

        private void removeSoldierButton_Click(object sender, EventArgs e)
        {
            if (soldierListBox.SelectedIndex != -1)
            {
                _levels[currLevel].formations[currFormation].RemoveAt(soldierListBox.SelectedIndex);
                soldierListBox.Items.RemoveAt(soldierListBox.SelectedIndex);
            }
            sendUpdate();
        }

        private void patternListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currPatternAction = patternListBox.SelectedIndex;

            if (currPatternAction != -1)
            {
                if (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count > 0)
                    actionComboBox0.SelectedIndex = _levels[currLevel].formationActions[currFormation][currPatternAction].actions[0];
                else
                    actionComboBox0.SelectedIndex = -1;
                if (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count > 1)
                    actionComboBox1.SelectedIndex = _levels[currLevel].formationActions[currFormation][currPatternAction].actions[1];
                else
                    actionComboBox1.SelectedIndex = -1;
                if (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count > 2)
                    actionComboBox2.SelectedIndex = _levels[currLevel].formationActions[currFormation][currPatternAction].actions[2];
                else
                    actionComboBox2.SelectedIndex = -1;
                if (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count > 3)
                    actionComboBox3.SelectedIndex = _levels[currLevel].formationActions[currFormation][currPatternAction].actions[3];
                else
                    actionComboBox3.SelectedIndex = -1;

                patternTextBox.Text = _levels[currLevel].formationActionNames[currFormation][currPatternAction];
                durationTextBox.Text = _levels[currLevel].formationActions[currFormation][currPatternAction].duration.ToString();
            }
        }

        private void actionComboBox0_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currPatternAction == -1 || actionComboBox0.SelectedIndex == -1)
                return;

            if (_levels[currLevel].formationActions[currFormation] == null)
                _levels[currLevel].formationActions[currFormation] = new List<PatternAction>(10);

            if (_levels[currLevel].formationActions[currFormation][currPatternAction] == null)
                _levels[currLevel].formationActions[currFormation][currPatternAction] = new PatternAction();

            while (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count <= 0)
                _levels[currLevel].formationActions[currFormation][currPatternAction].actions.Add(0);

            _levels[currLevel].formationActions[currFormation][currPatternAction].actions[0] = actionComboBox0.SelectedIndex;
        }

        private void actionComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currPatternAction == -1 || actionComboBox0.SelectedIndex == -1)
                return;

            if (_levels[currLevel].formationActions[currFormation] == null)
                _levels[currLevel].formationActions[currFormation] = new List<PatternAction>(10);

            if (_levels[currLevel].formationActions[currFormation][currPatternAction] == null)
                _levels[currLevel].formationActions[currFormation][currPatternAction] = new PatternAction();

            while (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count <= 1)
                _levels[currLevel].formationActions[currFormation][currPatternAction].actions.Add(0);

            _levels[currLevel].formationActions[currFormation][currPatternAction].actions[1] = actionComboBox1.SelectedIndex;
        }

        private void actionComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currPatternAction == -1 || actionComboBox1.SelectedIndex == -1)
                return;

            if (_levels[currLevel].formationActions[currFormation] == null)
                _levels[currLevel].formationActions[currFormation] = new List<PatternAction>(10);

            if (_levels[currLevel].formationActions[currFormation][currPatternAction] == null)
                _levels[currLevel].formationActions[currFormation][currPatternAction] = new PatternAction();

            while (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count <= 2)
                _levels[currLevel].formationActions[currFormation][currPatternAction].actions.Add(0);

            _levels[currLevel].formationActions[currFormation][currPatternAction].actions[2] = actionComboBox2.SelectedIndex;
        }

        private void actionComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currPatternAction == -1 || actionComboBox2.SelectedIndex == -1)
                return;

            if (_levels[currLevel].formationActions[currFormation] == null)
                _levels[currLevel].formationActions[currFormation] = new List<PatternAction>(10);

            if (_levels[currLevel].formationActions[currFormation][currPatternAction] == null)
                _levels[currLevel].formationActions[currFormation][currPatternAction] = new PatternAction();

            while (_levels[currLevel].formationActions[currFormation][currPatternAction].actions.Count <= 3)
                _levels[currLevel].formationActions[currFormation][currPatternAction].actions.Add(0);

            _levels[currLevel].formationActions[currFormation][currPatternAction].actions[3] = actionComboBox3.SelectedIndex;
        }

        private void durationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currPatternAction == -1)
                return;

            if (_levels[currLevel].formationActions[currFormation] == null)
                _levels[currLevel].formationActions[currFormation] = new List<PatternAction>(10);

            if (_levels[currLevel].formationActions[currFormation][currPatternAction] == null)
                _levels[currLevel].formationActions[currFormation][currPatternAction] = new PatternAction();

            if (currFormation != -1)
            {
                try
                {
                    _levels[currLevel].formationActions[currFormation][currPatternAction].duration = System.Convert.ToDouble(durationTextBox.Text);
                }
                catch
                {
                    _levels[currLevel].formationActions[currFormation][currPatternAction].duration = 0f;
                }
            }

            sendUpdate();
        }

        private void patternTextBox_TextChanged(object sender, EventArgs e)
        {
            if (patternListBox.SelectedIndex != -1)
            {
                _levels[currLevel].formationActionNames[currFormation][currPatternAction] = patternTextBox.Text;

                //refresh the list box
                patternListBox.Items.Clear();
                foreach (string s in _levels[currLevel].formationActionNames[currFormation])
                    patternListBox.Items.Add(s);

                patternListBox.SelectedIndex = currPatternAction;
            }
        }

        private void newPatternButton_Click(object sender, EventArgs e)
        {
            _levels[currLevel].formationActionNames[currFormation].Add("New Action");
            _levels[currLevel].formationActions[currFormation].Add(new PatternAction(new List<int>(4), 0f));

            refreshPatternListBox();
        }

        private void refreshPatternListBox()
        {
            patternListBox.Items.Clear();

            if (_levels[currLevel].formationActions.Count > 0 && _levels[currLevel].formationActions[currFormation].Count > 0)
            {
                if(currPatternAction >= _levels[currLevel].formationActions[currFormation].Count )
                    currPatternAction = _levels[currLevel].formationActions[currFormation].Count - 1;
                foreach (string s in _levels[currLevel].formationActionNames[currFormation])
                    patternListBox.Items.Add(s);

                patternListBox.SelectedIndex = currPatternAction;
            }
            else
                currPatternAction = -1;
        }

        private void refreshSoldierListBox()
        {
            this.soldierListBox.Items.Clear();
        }

        private void addPatternButton_Click(object sender, EventArgs e)
        {
            _levels[currLevel].formationActionNames[currFormation].Add(_levels[currLevel].formationActionNames[currFormation][currPatternAction]);

            PatternAction action = new PatternAction(new List<int>(4), _levels[currLevel].formationActions[currFormation][currPatternAction].duration);
            foreach (int i in _levels[currLevel].formationActions[currFormation][currPatternAction].actions)
                action.addAction(i);

            _levels[currLevel].formationActions[currFormation].Add(action);

            refreshPatternListBox();
        }

        private void removePatternButton_Click(object sender, EventArgs e)
        {
            if (currPatternAction != -1 && currPatternAction < patternListBox.Items.Count)
            {
                removePattern(currPatternAction);
                patternListBox.Items.RemoveAt(currPatternAction);
            }
        }

        private void removePattern(int currPatternAction)
        {
            _levels[currLevel].formationActionNames[currFormation].RemoveAt(currPatternAction);
            _levels[currLevel].formationActions[currFormation].RemoveAt(currPatternAction);

            currPatternAction = -1;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            // make levelContent from our level
            LevelPipeline.LevelContent testValue = new LevelPipeline.LevelContent(_levels[currLevel].levelName);

            testValue.formations = _levels[currLevel].formations;
            foreach (List<PatternAction> lp in _levels[currLevel].formationActions)
            {
                testValue.formationActions.Add(new List<LevelPipeline.PatternActionContent>(20));
                foreach (PatternAction pa in lp)
                    testValue.formationActions[testValue.formationActions.Count - 1].Add(new LevelPipeline.PatternActionContent(pa.actions, pa.duration));
            }
            
            testValue.formationPositions = _levels[currLevel].formationPositions;
            testValue.formationTimes = _levels[currLevel].formationTimes;
            testValue.formationSides = _levels[currLevel].formationSides;
            testValue.formationNames = _levels[currLevel].formationNames;
            testValue.formationActionNames = _levels[currLevel].formationActionNames;

            testValue.terrains = _levels[currLevel].terrains;
            testValue.terrainPositions = _levels[currLevel].terrainPositions;
            testValue.terrainTimes = _levels[currLevel].terrainTimes;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter xmlWriter = XmlWriter.Create("Content\\" + testValue.levelName + ".xml", settings))
            {
                IntermediateSerializer.Serialize(xmlWriter, testValue, null);
            }
        }

        void LevelEditorScreenListner.updateLevelFromScreenTerrain(int formation, float x, float y)
        {
            if (formation == -1)
                return;

            _levels[currLevel].terrainTimes[formation] = _levels[currLevel].terrainTimes[formation] + (x - _levels[currLevel].terrainPositions[formation].X);
            _levels[currLevel].terrainPositions[formation] = new Vector2(x, y);

            if (currTerrain == formation)
            {
                txTextBox.Text = _levels[currLevel].terrainPositions[currTerrain].X.ToString();
                tyTextBox.Text = _levels[currLevel].terrainPositions[currTerrain].Y.ToString();
                tSpawnTextBox.Text = _levels[currLevel].terrainTimes[currTerrain].ToString();
            }
        }

        void LevelEditorScreenListner.updateLevelFromScreen(int formation, float x, float y)
        {
            if (formation == -1)
                return;

            _levels[currLevel].formationTimes[formation] = _levels[currLevel].formationTimes[formation] + (x -_levels[currLevel].formationPositions[formation].X);
            _levels[currLevel].formationPositions[formation] = new Vector2(x, y);

            currFormation = formation;
            formationListBox.SelectedIndex = formation;

            xTextBox.Text = _levels[currLevel].formationPositions[currFormation].X.ToString();
            yTextBox.Text = _levels[currLevel].formationPositions[currFormation].Y.ToString();
            spawnTextBox.Text = _levels[currLevel].formationTimes[currFormation].ToString();
            formationTextBox.Text = _levels[currLevel].formationNames[currFormation];

            // populate the soldier list for this formation
            soldierListBox.Items.Clear();
            Predicate<SoldierClass> predicate;
            foreach (int s in _levels[currLevel].formations[currFormation])
            {
                predicate = delegate(SoldierClass arg)
                {
                    return arg.id == s;
                };
                soldierListBox.Items.Add(_soldierClass.Find(predicate).id.ToString() + " " + _soldierClass.Find(predicate).name);
            }

            //populate the patternaction list for this formation
            patternListBox.Items.Clear();
            foreach (string pa in _levels[currLevel].formationActionNames[currFormation])
            {
                patternListBox.Items.Add(pa);
            }
            
        }

        void LevelEditorScreenListner.copyTerrain(int ter)
        {
            _levels[currLevel].terrains.Add(_levels[currLevel].terrains[ter]);
            _levels[currLevel].terrainPositions.Add(_levels[currLevel].terrainPositions[ter]);
            _levels[currLevel].terrainTimes.Add(_levels[currLevel].terrainTimes[ter]);

            refreshTerrainListBox();
            sendUpdate();
        }

        void LevelEditorScreenListner.copyFormation(int formation)
        {
            copyTally++;

            _levels[currLevel].formationNames.Add(_levels[currLevel].formationNames[formation] + "_copy" + copyTally);
            _levels[currLevel].formationPositions.Add(_levels[currLevel].formationPositions[formation]);
            _levels[currLevel].formationTimes.Add(_levels[currLevel].formationTimes[formation]);
            _levels[currLevel].formationSides.Add(_levels[currLevel].formationSides[formation]);

            List<int> newFormation = new List<int>(10);
            foreach (int i in _levels[currLevel].formations[formation])
                newFormation.Add(i);

            _levels[currLevel].formations.Add(newFormation);

            List<string> formationActionNames = new List<string>(32);
            foreach (string s in _levels[currLevel].formationActionNames[formation])
                formationActionNames.Add(s);

            _levels[currLevel].formationActionNames.Add(formationActionNames);

            List<PatternAction> patternActions = new List<PatternAction>(32);
            List<int> actions;
            foreach (PatternAction pa in _levels[currLevel].formationActions[formation])
            {
                actions = new List<int>(32);
                foreach (int action in pa.actions)
                    actions.Add(action);

                PatternAction newPa = new PatternAction(actions, pa.duration);
                patternActions.Add(newPa);
            }

            _levels[currLevel].formationActions.Add(patternActions);

            refreshFormationListBox();

            sendUpdate();
        }

        void LevelEditorScreenListner.copyFormations(ArrayList formations, ArrayList terrains)
        {
            foreach (int formation in formations)
            {
                _levels[currLevel].formationNames.Add(_levels[currLevel].formationNames[formation]);
                _levels[currLevel].formationPositions.Add(_levels[currLevel].formationPositions[formation]);
                _levels[currLevel].formationTimes.Add(_levels[currLevel].formationTimes[formation]);
                _levels[currLevel].formationSides.Add(_levels[currLevel].formationSides[formation]);

                List<int> newFormation = new List<int>(10);
                foreach (int i in _levels[currLevel].formations[formation])
                    newFormation.Add(i);

                _levels[currLevel].formations.Add(newFormation);

                List<string> formationActionNames = new List<string>(32);
                foreach (string s in _levels[currLevel].formationActionNames[formation])
                    formationActionNames.Add(s);

                _levels[currLevel].formationActionNames.Add(formationActionNames);

                List<PatternAction> patternActions = new List<PatternAction>(32);
                List<int> actions;
                foreach (PatternAction pa in _levels[currLevel].formationActions[formation])
                {
                    actions = new List<int>(32);
                    foreach (int action in pa.actions)
                        actions.Add(action);

                    PatternAction newPa = new PatternAction(actions, pa.duration);
                    patternActions.Add(newPa);
                }

                _levels[currLevel].formationActions.Add(patternActions);
            }
            refreshFormationListBox();

            foreach (int ter in terrains)
            {
                _levels[currLevel].terrains.Add(_levels[currLevel].terrains[ter]);
                _levels[currLevel].terrainPositions.Add(_levels[currLevel].terrainPositions[ter]);
                _levels[currLevel].terrainTimes.Add(_levels[currLevel].terrainTimes[ter]);
            }
            refreshTerrainListBox();

            sendUpdate();
        }


        void LevelEditorScreenListner.deleteFormation(int formation)
        {
            _levels[currLevel].formationActionNames.RemoveAt(formation);
            _levels[currLevel].formationActions.RemoveAt(formation);
            _levels[currLevel].formationPositions.RemoveAt(formation);
            _levels[currLevel].formations.RemoveAt(formation);
            _levels[currLevel].formationTimes.RemoveAt(formation);
            _levels[currLevel].formationNames.RemoveAt(formation);
            _levels[currLevel].formationSides.RemoveAt(formation);

            currFormation = -1;
            currPatternAction = -1;

            formationListBox.Items.RemoveAt(formation);
            sendUpdate();
        }

        void LevelEditorScreenListner.deleteTerrain(int ter)
        {
            _levels[currLevel].terrainPositions.RemoveAt(ter);
            _levels[currLevel].terrainTimes.RemoveAt(ter);
            _levels[currLevel].terrains.RemoveAt(ter);
            currTerrain = -1;

            terrainListBox.Items.RemoveAt(ter);
            sendUpdate();
        }

        private void txTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currTerrain != -1)
            {
                try
                {
                    _levels[currLevel].terrainPositions[currTerrain] = new Vector2(System.Convert.ToSingle(txTextBox.Text), _levels[currLevel].terrainPositions[currTerrain].Y);
                }
                catch
                {
                    _levels[currLevel].terrainPositions[currTerrain] = new Vector2(0f, _levels[currLevel].terrainPositions[currTerrain].Y);
                }
            }

            sendUpdateTerrain();
        }

        private void terrainListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currTerrain = terrainListBox.SelectedIndex;

            if (currTerrain != -1)
            {
                txTextBox.Text = _levels[currLevel].terrainPositions[currTerrain].X.ToString();
                tyTextBox.Text = _levels[currLevel].terrainPositions[currTerrain].Y.ToString();
                tSpawnTextBox.Text = _levels[currLevel].terrainTimes[currTerrain].ToString();
            }
        }

        private void removeTerrainButton_Click(object sender, EventArgs e)
        {
            if (terrainListBox.SelectedIndex != -1)
            {
                _levels[currLevel].terrainPositions.RemoveAt(terrainListBox.SelectedIndex);
                _levels[currLevel].terrains.RemoveAt(terrainListBox.SelectedIndex);
                terrainListBox.Items.RemoveAt(terrainListBox.SelectedIndex);
            }

            currTerrain = -1;
            sendUpdate();
        }

        private void tyTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currTerrain != -1)
            {
                try
                {
                    _levels[currLevel].terrainPositions[currTerrain] = new Vector2(_levels[currLevel].terrainPositions[currTerrain].X, System.Convert.ToSingle(tyTextBox.Text));
                }
                catch
                {
                    _levels[currLevel].terrainPositions[currTerrain] = new Vector2(_levels[currLevel].terrainPositions[currTerrain].X, 0f);
                }
            }

            sendUpdateTerrain();
        }

        private void tSpawnTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currTerrain != -1)
            {
                try
                {
                    _levels[currLevel].terrainTimes[currTerrain] = System.Convert.ToDouble(tSpawnTextBox.Text);
                }
                catch
                {
                    _levels[currLevel].terrainTimes[currTerrain] = 0f;
                }
            }

            sendUpdate();
        }

        private void addTerrainButton_Click(object sender, EventArgs e)
        {
            if (terrainComboBox.SelectedIndex != -1)
            {
                _levels[currLevel].terrains.Add(_terrainClass[terrainComboBox.SelectedIndex].id);
                _levels[currLevel].terrainTimes.Add(0f);
                _levels[currLevel].terrainPositions.Add(Vector2.Zero);
                terrainListBox.Items.Add(_terrainClass[terrainComboBox.SelectedIndex].id.ToString() + " " + _terrainClass[terrainComboBox.SelectedIndex].name);
            }
            sendUpdate();
        }

        void refreshTerrainListBox()
        {
            terrainListBox.Items.Clear();
            foreach (int i in _levels[currLevel].terrains)
            {
                terrainListBox.Items.Add(_terrainClass[i].id.ToString() + " " + _terrainClass[i].name);
            }
        }

        private void movePatternUp_Click(object sender, EventArgs e)
        {
            if (patternListBox.SelectedIndex != -1 && patternListBox.SelectedIndex != 0)
            {
                PatternAction pattern = _levels[currLevel].formationActions[currFormation][patternListBox.SelectedIndex];
                _levels[currLevel].formationActions[currFormation].RemoveAt(patternListBox.SelectedIndex);
                _levels[currLevel].formationActions[currFormation].Insert(patternListBox.SelectedIndex - 1, pattern);

                String patternName = _levels[currLevel].formationActionNames[currFormation][patternListBox.SelectedIndex];
                _levels[currLevel].formationActionNames[currFormation].RemoveAt(patternListBox.SelectedIndex);
                _levels[currLevel].formationActionNames[currFormation].Insert(patternListBox.SelectedIndex - 1, patternName);

                currPatternAction--;
                refreshPatternListBox();
                sendUpdate();
            }
        }

        private void movePatternDown_Click(object sender, EventArgs e)
        {
            if (patternListBox.SelectedIndex != -1 && patternListBox.SelectedIndex < patternListBox.Items.Count -1)
            {
                PatternAction pattern = _levels[currLevel].formationActions[currFormation][patternListBox.SelectedIndex];
                _levels[currLevel].formationActions[currFormation].RemoveAt(patternListBox.SelectedIndex);
                _levels[currLevel].formationActions[currFormation].Insert(patternListBox.SelectedIndex + 1, pattern);

                String patternName = _levels[currLevel].formationActionNames[currFormation][patternListBox.SelectedIndex];
                _levels[currLevel].formationActionNames[currFormation].RemoveAt(patternListBox.SelectedIndex);
                _levels[currLevel].formationActionNames[currFormation].Insert(patternListBox.SelectedIndex + 1, patternName);

                currPatternAction++;
                refreshPatternListBox();
                sendUpdate();
            }
        }

        private void moveFormationUp_Click(object sender, EventArgs e)
        {
            if (formationListBox.SelectedIndex != -1 && formationListBox.SelectedIndex != 0)
            {
                String formationName = _levels[currLevel].formationNames[currFormation];
                _levels[currLevel].formationNames.RemoveAt(currFormation);
                _levels[currLevel].formationNames.Insert(formationListBox.SelectedIndex - 1, formationName);

                Vector2 formationPosition = _levels[currLevel].formationPositions[currFormation];
                _levels[currLevel].formationPositions.RemoveAt(currFormation);
                _levels[currLevel].formationPositions.Insert(formationListBox.SelectedIndex - 1, formationPosition);

                double formationTime = _levels[currLevel].formationTimes[currFormation];
                _levels[currLevel].formationTimes.RemoveAt(currFormation);
                _levels[currLevel].formationTimes.Insert(formationListBox.SelectedIndex - 1, formationTime);

                List<PatternAction> formationActions = _levels[currLevel].formationActions[currFormation];
                _levels[currLevel].formationActions.RemoveAt(currFormation);
                _levels[currLevel].formationActions.Insert(formationListBox.SelectedIndex - 1, formationActions);

                List<string> formationActionNames = _levels[currLevel].formationActionNames[currFormation];
                _levels[currLevel].formationActionNames.RemoveAt(currFormation);
                _levels[currLevel].formationActionNames.Insert(formationListBox.SelectedIndex - 1, formationActionNames);

                List<int> formation = _levels[currLevel].formations[currFormation];
                _levels[currLevel].formations.RemoveAt(currFormation);
                _levels[currLevel].formations.Insert(formationListBox.SelectedIndex - 1, formation);

                int side = _levels[currLevel].formationSides[currFormation];
                _levels[currLevel].formationSides.RemoveAt(currFormation);
                _levels[currLevel].formationSides.Insert(formationListBox.SelectedIndex - 1, side);

                currFormation--;
                refreshFormationListBox();
                sendUpdate();
            }
        }

        private void moveFormationDown_Click(object sender, EventArgs e)
        {
            if (formationListBox.SelectedIndex != -1 && formationListBox.SelectedIndex < formationListBox.Items.Count - 1)
            {
                String formationName = _levels[currLevel].formationNames[currFormation];
                _levels[currLevel].formationNames.RemoveAt(currFormation);
                _levels[currLevel].formationNames.Insert(formationListBox.SelectedIndex + 1, formationName);

                Vector2 formationPosition = _levels[currLevel].formationPositions[currFormation];
                _levels[currLevel].formationPositions.RemoveAt(currFormation);
                _levels[currLevel].formationPositions.Insert(formationListBox.SelectedIndex + 1, formationPosition);

                double formationTime = _levels[currLevel].formationTimes[currFormation];
                _levels[currLevel].formationTimes.RemoveAt(currFormation);
                _levels[currLevel].formationTimes.Insert(formationListBox.SelectedIndex + 1, formationTime);

                List<PatternAction> formationActions = _levels[currLevel].formationActions[currFormation];
                _levels[currLevel].formationActions.RemoveAt(currFormation);
                _levels[currLevel].formationActions.Insert(formationListBox.SelectedIndex + 1, formationActions);

                List<string> formationActionNames = _levels[currLevel].formationActionNames[currFormation];
                _levels[currLevel].formationActionNames.RemoveAt(currFormation);
                _levels[currLevel].formationActionNames.Insert(formationListBox.SelectedIndex + 1, formationActionNames);

                List<int> formation = _levels[currLevel].formations[currFormation];
                _levels[currLevel].formations.RemoveAt(currFormation);
                _levels[currLevel].formations.Insert(formationListBox.SelectedIndex + 1, formation);

                int side = _levels[currLevel].formationSides[currFormation];
                _levels[currLevel].formationSides.RemoveAt(currFormation);
                _levels[currLevel].formationSides.Insert(formationListBox.SelectedIndex + 1, side);

                currFormation++;
                refreshFormationListBox();
                sendUpdate();
            }
        }

        private List<PatternAction> copiedformationActions;
        private List<string> copiedformationNames;

        private void copyPattern_Click(object sender, EventArgs e)
        {
            copiedformationActions = new List<PatternAction>();
            foreach (PatternAction pa in _levels[currLevel].formationActions[currFormation])
                copiedformationActions.Add(pa.copy());

            copiedformationNames = new List<string>();
            foreach (string str in _levels[currLevel].formationActionNames[currFormation])
                copiedformationNames.Add(String.Copy(str));

            int cf = currFormation;
            sendUpdate(); 
            refreshFormationListBox();
            currFormation = cf;
            formationListBox.SelectedIndex = cf;
        }

        private void pastePattern_Click(object sender, EventArgs e)
        {
            _levels[currLevel].formationActions[currFormation] = new List<PatternAction>();
            foreach (PatternAction pa in copiedformationActions)
                _levels[currLevel].formationActions[currFormation].Add(pa.copy());

            _levels[currLevel].formationActionNames[currFormation] = new List<string>();
            foreach (string str in copiedformationNames)
                _levels[currLevel].formationActionNames[currFormation].Add(String.Copy(str));

            int cf = currFormation;
            sendUpdate();
            refreshFormationListBox();
            currFormation = cf;
            formationListBox.SelectedIndex = cf;
        }

        private void formationSideComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currFormation != -1 && formationSideComboBox.SelectedIndex != -1)
            {
                try
                {
                    _levels[currLevel].formationSides[currFormation] = formationSideComboBox.SelectedIndex - 1;
                }
                catch
                {
                    _levels[currLevel].formationSides[currFormation] = -1;
                }
            }

            sendUpdate();
        }

        private void shiftFormationsButton_Click(object sender, EventArgs e)
        {
            float amount;
            float begin;

            try
            {
                amount = System.Convert.ToSingle(shiftFormationsTextBox.Text);
            }
            catch
            {
                amount = 0f;
            }

            try
            {
                begin = System.Convert.ToSingle(shiftBeginTextBox.Text);
            }
            catch
            {
                begin = 0f;
            }

            for (int i = 0 ; i < _levels[currLevel].formationPositions.Count; i++)
            {
                if (_levels[currLevel].formationPositions[i].X > begin)
                {
                    _levels[currLevel].formationPositions[i] = _levels[currLevel].formationPositions[i] + new Vector2(amount, 0);
                    _levels[currLevel].formationTimes[i] += amount;
                }
            }
            sendUpdate();
            refreshFormationListBox();
        }

        private void copyLevel_Click(object sender, EventArgs e)
        {
            copiedLevel = _levels[currLevel];
        }

        private void pasteLevel_Click(object sender, EventArgs e)
        {
            if(copiedLevel != null)
            {
                foreach(List<string> list in copiedLevel.formationActionNames)
                {
                    List<string> newList = new List<string>();
                    foreach(string s in list)
                    {
                        newList.Add(String.Copy(s));
                    }
                    _levels[currLevel].formationActionNames.Add(newList);
                }
                foreach (List<PatternAction> newPAList in copiedLevel.formationActions)
                {
                    List<PatternAction> newList = new List<PatternAction>();
                    foreach (PatternAction pa in newPAList)
                    {
                        newList.Add(pa.copy());
                    }
                    _levels[currLevel].formationActions.Add(newList);
                }
                foreach (string str in copiedLevel.formationNames)
                {
                    _levels[currLevel].formationNames.Add(String.Copy(str));
                }
                foreach(Vector2 vect in copiedLevel.formationPositions)
                {
                    _levels[currLevel].formationPositions.Add(new Vector2(vect.X, vect.Y));
                }
                foreach(List<int> list in copiedLevel.formations)
                {
                    List<int> newList = new List<int>();
                    foreach(int i in list)
                    {
                        newList.Add(i);
                    }
                    _levels[currLevel].formations.Add(newList);
                }
                foreach (int i in copiedLevel.formationSides)
                {
                    _levels[currLevel].formationSides.Add(i);
                }
                foreach(double d in copiedLevel.formationTimes)
                {
                    _levels[currLevel].formationTimes.Add(d);
                }
                sendUpdate();
                refreshFormationListBox();
                refreshPatternListBox();
                refreshSoldierListBox();
            }
        }

        private void startPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _levels[currLevel].startingPosition = System.Convert.ToSingle(startPositionTextBox.Text);
            }
            catch (FormatException fe)
            {
                _levels[currLevel].startingPosition = 0;
            }
            sendUpdate();
        }

    }

    public class SoldierClass
    {
        public string name;
        public int id;

        public SoldierClass()
        {

        }

        public SoldierClass(string name, int id)
        {
            this.name = name;
            this.id = id;
        }
    }

    public class TerrainClass
    {
        public string name;
        public int id;

        public TerrainClass()
        {

        }

        public TerrainClass(string name, int id)
        {
            this.name = name;
            this.id = id;
        }
    }

    public class ActionClass
    {
        public string name;
        public int id;

        public ActionClass()
        {

        }

        public ActionClass(string name, int id)
        {
            this.name = name;
            this.id = id;
        }
    }

    public interface FormListener
    {
        void updateLevel(Level level, int selectedFormation);
    }
}
