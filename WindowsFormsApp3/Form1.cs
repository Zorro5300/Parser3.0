using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LexicalAnalyzer
{
    public class Form1 : Form
    {
        private RichTextBox editorTextBox;
        private DataGridView errorGridView;
        private RichTextBox errorLogTextBox;
        private TabControl errorsTabControl;
        private string currentFile = null;
        private bool isModified = false;

        private LexicalAnalyzer analyzer;

        private MenuStrip menuStrip;
        private ToolStripMenuItem menuFile, menuEdit, menuText, menuRun, menuHelp, menuLanguage;
        private ToolStripMenuItem miNew, miOpen, miSave, miSaveAs, miExit;
        private ToolStripMenuItem miUndo, miRedo, miCut, miCopy, miPaste, miSelectAll;
        private ToolStripMenuItem miTask, miGrammar, miClass, miMethod, miTest, miLit, miSource;
        private ToolStripMenuItem miRun;
        private ToolStripMenuItem miHelp, miAbout;
        private ToolStripMenuItem miLangRu, miLangEn;

        private ToolStrip toolStrip;
        private ToolStripButton tbNew, tbOpen, tbSave, tbUndo, tbRedo, tbCut, tbCopy, tbPaste, tbRun, tbHelp;

        private Label errorsTableLabel, errorsLogLabel;
        private TabPage tabErrorsTable, tabErrorsLog;

        public Form1()
        {
            analyzer = new LexicalAnalyzer();
            Localization.LanguageChanged += (s, e) => ApplyLocalization();
            CreateInterface();
            ApplyLocalization();
        }

        private void CreateInterface()
        {
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.LightGray;

            menuStrip = new MenuStrip { Font = new Font("Segoe UI", 10, FontStyle.Regular) };

            menuFile = new ToolStripMenuItem();
            miNew = new ToolStripMenuItem(null, null, (s, e) => NewFile());
            miOpen = new ToolStripMenuItem(null, null, (s, e) => OpenFile());
            miSave = new ToolStripMenuItem(null, null, (s, e) => SaveFile());
            miSaveAs = new ToolStripMenuItem(null, null, (s, e) => SaveFileAs());
            miExit = new ToolStripMenuItem(null, null, (s, e) => Close());
            menuFile.DropDownItems.AddRange(new ToolStripItem[]
            {
                miNew, miOpen, miSave, miSaveAs, new ToolStripSeparator(), miExit
            });

            menuEdit = new ToolStripMenuItem();
            miUndo = new ToolStripMenuItem(null, null, (s, e) => Undo());
            miRedo = new ToolStripMenuItem(null, null, (s, e) => Redo());
            miCut = new ToolStripMenuItem(null, null, (s, e) => editorTextBox?.Cut());
            miCopy = new ToolStripMenuItem(null, null, (s, e) => editorTextBox?.Copy());
            miPaste = new ToolStripMenuItem(null, null, (s, e) => editorTextBox?.Paste());
            miSelectAll = new ToolStripMenuItem(null, null, (s, e) => editorTextBox?.SelectAll());
            menuEdit.DropDownItems.AddRange(new ToolStripItem[]
            {
                miUndo, miRedo, new ToolStripSeparator(),
                miCut, miCopy, miPaste, new ToolStripSeparator(), miSelectAll
            });

            menuText = new ToolStripMenuItem();
            miTask = new ToolStripMenuItem(null, null, (s, e) => ShowTask());
            miGrammar = new ToolStripMenuItem(null, null, (s, e) => ShowGrammar());
            miClass = new ToolStripMenuItem(null, null, (s, e) => ShowClassification());
            miMethod = new ToolStripMenuItem(null, null, (s, e) => ShowMethod());
            miTest = new ToolStripMenuItem(null, null, (s, e) => ShowTestExample());
            miLit = new ToolStripMenuItem(null, null, (s, e) => ShowLiterature());
            miSource = new ToolStripMenuItem(null, null, (s, e) => ShowSourceCode());
            menuText.DropDownItems.AddRange(new ToolStripItem[]
            {
                miTask, miGrammar, miClass, miMethod, miTest, miLit, miSource
            });

            menuRun = new ToolStripMenuItem();
            miRun = new ToolStripMenuItem(null, null, (s, e) => RunAnalysis());
            miRun.ShortcutKeyDisplayString = "F5";
            menuRun.DropDownItems.Add(miRun);

            menuHelp = new ToolStripMenuItem();
            miHelp = new ToolStripMenuItem(null, null, (s, e) => ShowHelp());
            miAbout = new ToolStripMenuItem(null, null, (s, e) => ShowAbout());
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { miHelp, miAbout });

            menuLanguage = new ToolStripMenuItem();
            miLangRu = new ToolStripMenuItem(null, null, (s, e) => Localization.SetLanguage(UiLanguage.Russian));
            miLangEn = new ToolStripMenuItem(null, null, (s, e) => Localization.SetLanguage(UiLanguage.English));
            menuLanguage.DropDownItems.AddRange(new ToolStripItem[] { miLangRu, miLangEn });

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                menuFile, menuEdit, menuText, menuRun, menuHelp, menuLanguage
            });

            toolStrip = new ToolStrip
            {
                ImageScalingSize = new Size(24, 24),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(5)
            };

            tbNew = new ToolStripButton(null, null, (s, e) => NewFile());
            tbOpen = new ToolStripButton(null, null, (s, e) => OpenFile());
            tbSave = new ToolStripButton(null, null, (s, e) => SaveFile());
            tbUndo = new ToolStripButton(null, null, (s, e) => Undo());
            tbRedo = new ToolStripButton(null, null, (s, e) => Redo());
            tbCut = new ToolStripButton(null, null, (s, e) => editorTextBox?.Cut());
            tbCopy = new ToolStripButton(null, null, (s, e) => editorTextBox?.Copy());
            tbPaste = new ToolStripButton(null, null, (s, e) => editorTextBox?.Paste());
            tbRun = new ToolStripButton(null, null, (s, e) => RunAnalysis());
            tbHelp = new ToolStripButton(null, null, (s, e) => ShowHelp());

            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                tbNew, tbOpen, tbSave, new ToolStripSeparator(),
                tbUndo, tbRedo, new ToolStripSeparator(),
                tbCut, tbCopy, tbPaste, new ToolStripSeparator(),
                tbRun, new ToolStripSeparator(), tbHelp
            });

            editorTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 14),
                BackColor = Color.LightGray,
                Text = @""
            };
            editorTextBox.TextChanged += (s, e) => { isModified = true; UpdateTitle(); };

            Panel editorPanel = new Panel { Dock = DockStyle.Fill };
            editorPanel.Controls.Add(editorTextBox);

            errorGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.Silver,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            errorGridView.Columns.Add("Fragment", "");
            errorGridView.Columns.Add("Location", "");
            errorGridView.Columns.Add("Description", "");
            errorGridView.Columns[0].Width = 150;
            errorGridView.Columns[1].Width = 150;
            errorGridView.Columns[2].Width = 300;
            errorGridView.CellClick += ErrorGridView_CellClick;

            errorsTableLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGray
            };

            Panel errorsTablePanel = new Panel { Dock = DockStyle.Fill };
            errorsTablePanel.Controls.Add(errorGridView);
            errorsTablePanel.Controls.Add(errorsTableLabel);
            errorsTableLabel.BringToFront();

            errorLogTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 11),
                BackColor = Color.MistyRose,
                ReadOnly = true,
                ForeColor = Color.DarkRed
            };

            errorsLogLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGray
            };

            Panel errorsLogPanel = new Panel { Dock = DockStyle.Fill };
            errorsLogPanel.Controls.Add(errorLogTextBox);
            errorsLogPanel.Controls.Add(errorsLogLabel);
            errorsLogLabel.BringToFront();

            errorsTabControl = new TabControl { Dock = DockStyle.Fill };
            tabErrorsTable = new TabPage { Controls = { errorsTablePanel } };
            tabErrorsLog = new TabPage { Controls = { errorsLogPanel } };
            errorsTabControl.TabPages.Add(tabErrorsTable);
            errorsTabControl.TabPages.Add(tabErrorsLog);

            SplitContainer mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };
            mainSplit.Panel1.Controls.Add(editorPanel);
            mainSplit.Panel2.Controls.Add(errorsTabControl);

            Controls.Add(mainSplit);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);

            menuStrip.Dock = DockStyle.Top;
            toolStrip.Dock = DockStyle.Top;
            mainSplit.Top = toolStrip.Bottom;

            KeyPreview = true;
            KeyDown += Form1_KeyDown;

            UpdateTitle();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N) { NewFile(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.O) { OpenFile(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.S) { SaveFile(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.Z) { Undo(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.Y) { Redo(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.X) { editorTextBox.Cut(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.C) { editorTextBox.Copy(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.V) { editorTextBox.Paste(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { RunAnalysis(); e.Handled = true; }
        }

        private void ApplyLocalization()
        {
            menuFile.Text = Localization.Get("menu_file");
            miNew.Text = Localization.Get("file_new");
            miOpen.Text = Localization.Get("file_open");
            miSave.Text = Localization.Get("file_save");
            miSaveAs.Text = Localization.Get("file_save_as");
            miExit.Text = Localization.Get("file_exit");

            menuEdit.Text = Localization.Get("menu_edit");
            miUndo.Text = Localization.Get("edit_undo");
            miRedo.Text = Localization.Get("edit_redo");
            miCut.Text = Localization.Get("edit_cut");
            miCopy.Text = Localization.Get("edit_copy");
            miPaste.Text = Localization.Get("edit_paste");
            miSelectAll.Text = Localization.Get("edit_select_all");

            menuText.Text = Localization.Get("menu_text");
            miTask.Text = Localization.Get("text_task");
            miGrammar.Text = Localization.Get("text_grammar");
            miClass.Text = Localization.Get("text_classification");
            miMethod.Text = Localization.Get("text_method");
            miTest.Text = Localization.Get("text_test");
            miLit.Text = Localization.Get("text_literature");
            miSource.Text = Localization.Get("text_source");

            menuRun.Text = Localization.Get("menu_run");
            miRun.Text = Localization.Get("run_analyze");

            menuHelp.Text = Localization.Get("menu_help");
            miHelp.Text = Localization.Get("help_help");
            miAbout.Text = Localization.Get("help_about");

            menuLanguage.Text = Localization.Get("menu_language");
            miLangRu.Text = Localization.Get("lang_russian");
            miLangEn.Text = Localization.Get("lang_english");
            miLangRu.Checked = Localization.Current == UiLanguage.Russian;
            miLangEn.Checked = Localization.Current == UiLanguage.English;

            tbNew.Text = Localization.Get("tb_new");
            tbOpen.Text = Localization.Get("tb_open");
            tbSave.Text = Localization.Get("tb_save");
            tbUndo.Text = Localization.Get("tb_undo");
            tbRedo.Text = Localization.Get("tb_redo");
            tbCut.Text = Localization.Get("tb_cut");
            tbCopy.Text = Localization.Get("tb_copy");
            tbPaste.Text = Localization.Get("tb_paste");
            tbRun.Text = Localization.Get("tb_run");
            tbHelp.Text = Localization.Get("tb_help");

            errorsTableLabel.Text = Localization.Get("label_errors_table");
            errorsLogLabel.Text = Localization.Get("label_errors_log");
            tabErrorsTable.Text = Localization.Get("tab_errors_table");
            tabErrorsLog.Text = Localization.Get("tab_errors_log");

            errorGridView.Columns[0].HeaderText = Localization.Get("col_fragment");
            errorGridView.Columns[1].HeaderText = Localization.Get("col_location");
            errorGridView.Columns[2].HeaderText = Localization.Get("col_description");

            UpdateTitle();
        }

        private void Undo()
        {
            if (editorTextBox.CanUndo)
                editorTextBox.Undo();
        }

        private void Redo()
        {
            if (editorTextBox.CanRedo)
                editorTextBox.Redo();
        }

        private void UpdateTitle()
        {
            string name = string.IsNullOrEmpty(currentFile)
                ? Localization.Get("title_new_file")
                : Path.GetFileName(currentFile);
            this.Text = $"{Localization.Get("title_app")} - {name}{(isModified ? "*" : "")}";
        }

        private void NewFile()
        {
            if (CheckSave())
            {
                editorTextBox.Clear();
                currentFile = null;
                isModified = false;
                UpdateTitle();
                ClearResults();
            }
        }

        private void OpenFile()
        {
            if (!CheckSave()) return;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = Localization.Get("dlg_filter");
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    editorTextBox.Text = File.ReadAllText(dlg.FileName);
                    currentFile = dlg.FileName;
                    isModified = false;
                    UpdateTitle();
                    ClearResults();
                }
            }
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(currentFile))
                SaveFileAs();
            else
            {
                File.WriteAllText(currentFile, editorTextBox.Text);
                isModified = false;
                UpdateTitle();
                MessageBox.Show(
                    Localization.Format("dlg_save_msg", Path.GetFileName(currentFile)),
                    Localization.Get("dlg_save_title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SaveFileAs()
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = Localization.Get("dlg_filter");
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, editorTextBox.Text);
                    currentFile = dlg.FileName;
                    isModified = false;
                    UpdateTitle();
                    MessageBox.Show(
                        Localization.Format("dlg_save_msg", Path.GetFileName(currentFile)),
                        Localization.Get("dlg_save_title"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private bool CheckSave()
        {
            if (isModified)
            {
                DialogResult res = MessageBox.Show(
                    Localization.Get("dlg_save_changes"),
                    Localization.Get("dlg_question"),
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes) SaveFile();
                return res != DialogResult.Cancel;
            }
            return true;
        }

        private void ClearResults()
        {
            errorGridView.Rows.Clear();
            errorLogTextBox.Clear();
        }

        private void RunAnalysis()
        {
            ClearResults();

            string code = editorTextBox.Text;

            if (string.IsNullOrWhiteSpace(code))
            {
                errorLogTextBox.AppendText(Localization.Get("log_no_code") + "\n");
                errorLogTextBox.AppendText(Localization.Get("log_enter_code") + "\n");
                errorGridView.Rows.Add("", "", Localization.Get("log_empty_grid"));
                return;
            }

            errorLogTextBox.AppendText(Localization.Get("log_start") + "\n\n");

            var errors = analyzer.Analyze(code);

            if (errors.Count == 0)
            {
                errorLogTextBox.AppendText(Localization.Get("log_no_lexical") + "\n");
                errorLogTextBox.AppendText(Localization.Get("log_no_syntax") + "\n\n");
                errorLogTextBox.AppendText(Localization.Get("log_success") + "\n");

                errorGridView.Rows.Add("✓", Localization.Get("log_ok_grid"), "");
                errorGridView.Rows[0].DefaultCellStyle.BackColor = Color.LightGreen;
            }
            else
            {
                int lexicalCount = 0, syntaxCount = 0;
                foreach (var err in errors)
                {
                    if (err.IsLexical) lexicalCount++;
                    else syntaxCount++;
                }

                errorLogTextBox.AppendText(Localization.Format("log_found", errors.Count) + "\n\n");
                errorLogTextBox.AppendText(Localization.Format("log_lexical_count", lexicalCount) + "\n");
                errorLogTextBox.AppendText(Localization.Format("log_syntax_count", syntaxCount) + "\n\n");

                if (lexicalCount > 0)
                {
                    errorLogTextBox.AppendText(Localization.Get("log_lexical_header") + "\n");
                    foreach (var err in errors)
                    {
                        if (err.IsLexical)
                        {
                            errorLogTextBox.AppendText(Localization.Format("log_lexical_line",
                                err.Line, err.Position, err.Fragment,
                                Localization.TranslateErrorDescription(err.Description)) + "\n");
                        }
                    }
                    errorLogTextBox.AppendText("\n");
                }

                if (syntaxCount > 0)
                {
                    errorLogTextBox.AppendText(Localization.Get("log_syntax_header") + "\n");
                    foreach (var err in errors)
                    {
                        if (!err.IsLexical)
                            errorLogTextBox.AppendText(Localization.TranslateErrorDescription(err.Description) + "\n");
                    }
                    errorLogTextBox.AppendText("\n");
                }

                foreach (var err in errors)
                {
                    int rowIndex = errorGridView.Rows.Add(
                        err.Fragment,
                        Localization.FormatErrorLocation(err),
                        Localization.TranslateErrorDescription(err.Description));
                    errorGridView.Rows[rowIndex].DefaultCellStyle.BackColor =
                        err.IsLexical ? Color.LightCoral : Color.LightGoldenrodYellow;
                    errorGridView.Rows[rowIndex].Tag = err;
                }

                errorLogTextBox.AppendText(Localization.Get("log_end") + "\n");
            }
        }

        private void ErrorGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var error = errorGridView.Rows[e.RowIndex].Tag as ErrorInfo;
                if (error != null && error.Fragment != "✓" && error.IsLexical)
                    NavigateToPosition(error.Line, error.Position);
            }
        }

        private void NavigateToPosition(int line, int position)
        {
            string[] lines = editorTextBox.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int charIndex = 0;

            for (int i = 0; i < line - 1 && i < lines.Length; i++)
                charIndex += lines[i].Length + 1;

            charIndex += position - 1;
            charIndex = Math.Max(0, Math.Min(charIndex, editorTextBox.Text.Length - 1));

            editorTextBox.Focus();
            editorTextBox.Select(charIndex, 1);
            editorTextBox.ScrollToCaret();

            Color originalColor = editorTextBox.SelectionBackColor;
            editorTextBox.SelectionBackColor = Color.Yellow;
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) =>
            {
                editorTextBox.SelectionBackColor = originalColor;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void ShowMessage(string textKey, string titleKey)
        {
            MessageBox.Show(Localization.Get(textKey), Localization.Get(titleKey),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowTask() => ShowMessage("task_text", "task_title");
        private void ShowGrammar() => ShowMessage("grammar_text", "grammar_title");
        private void ShowClassification() => ShowMessage("class_text", "class_title");
        private void ShowMethod() => ShowMessage("method_text", "method_title");
        private void ShowTestExample() => editorTextBox.Text = "type complex = record\n    re, im: real;\nend;";
        private void ShowLiterature() => ShowMessage("lit_text", "lit_title");
        private void ShowSourceCode() => ShowMessage("source_text", "source_title");
        private void ShowHelp() => ShowMessage("help_text", "help_title");
        private void ShowAbout() => ShowMessage("about_text", "about_title");

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!CheckSave()) e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
