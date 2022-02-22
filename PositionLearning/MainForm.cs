using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PositionLearning
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            initUI();
        }

        #region Announce

        private const string filePath = @"test.txt";
        private const string ansBtnName = "btn";
        private const string quesLabelName = "questionLabel";
        private const string scoreLabelName = "scoreLabel";
        private const string switchChkBoxName = "switchChkBox";

        private List<string> questionList = new List<string>();
        private List<string> answerList = new List<string>();
        private List<string> partList = new List<string>();
        private int[] randomAry; private int realAnsIdx = -1;
        private int iQ = 0;
        private Random rnd = new Random();

        #endregion

        #region Init

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!loadCsv(filePath, questionList, answerList, partList))
                return;

            displayQA();
        }

        private void initUI()
        {
            initForm(this);
            createUI();
        }

        private void initForm(Form form)
        {
            form.Text = "FakeIQ test";
            form.Controls.Clear();
            form.Size = new System.Drawing.Size(480, 350);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        private void createUI()
        {
            Label question = new Label
            {
                Name = quesLabelName,
                Location = new System.Drawing.Point(135, 40),
                Size = new System.Drawing.Size(250, 50),
                Text = string.Empty,
                Font = new System.Drawing.Font("微軟正黑體", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)))
            };

            Label score = new Label
            {
                Name = scoreLabelName,
                //Location = new System.Drawing.Point(135, 40),
                Size = new System.Drawing.Size(150, 50),
                Text = "Your IQ:",
                Font = new System.Drawing.Font("微軟正黑體", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136))),
                Dock = DockStyle.Right,
            };

            Button[] btns = new Button[4];
            for (int i = 0; i < btns.Length; i++)
            {
                btns[i] = new Button
                {
                    Name = ansBtnName,
                    Text = i.ToString(),
                    Width = 225,
                    Height = 100,
                    Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136))),
                };
                btns[i].Click += ansBtn_click;
            }

            CheckBox switchCheckBox = new CheckBox
            {
                Name = switchChkBoxName,
                Location = new System.Drawing.Point(10, 10),
                Text = "Switch Mode",
                Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136))),
            };
            switchCheckBox.CheckedChanged += SwitchCheckBox_CheckedChanged;

            FlowLayoutPanel layoutPanel = new FlowLayoutPanel
            {
                Name = "layoutPanel",
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(0, 100),
                Size = new Size(500, 210)
            };

            layoutPanel.Controls.AddRange(btns);

            this.Controls.Add(switchCheckBox);
            this.Controls.Add(layoutPanel);
            this.Controls.Add(question);
            this.Controls.Add(score);
        }

        private void SwitchCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            exChangeList(ref questionList, ref answerList);
            displayQA();
        }

        private void exChangeList(ref List<string> listA, ref List<string> listB)
        {
            var temp = listA.ToList();
            listA.Clear();
            listA = listB.ToList();
            listB.Clear();
            listB = temp.ToList();
        }

        private bool loadCsv(string path, List<string> cht, List<string> eng, List<string> part)
        {
            try
            {
                using (var reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split('\t');
                        if (values.Length == 2)
                        {
                            cht.Add(values[0]);
                            eng.Add(values[1]);
                        }
                        else if (values.Length == 1 && values[0] != string.Empty)
                        {
                            part.Add(values[0]);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return false;
        }

        #endregion

        #region Display the Label and the Buttons

        private void displayQA()
        {
            randomAry = getRandomAry(4, questionList.Count());

            displayBtnText(randomAry, answerList);
            displayQuestion(randomAry, questionList, ref realAnsIdx);
        }

        private int[] getRandomAry(int num, int range)
        {
            int[] randomArray = new int[num];
            for (int i = 0; i < num; i++)
            {
                randomArray[i] = rnd.Next(1, range);   //亂數產生，亂數產生的範圍是1~9

                for (int j = 0; j < i; j++)
                {
                    while (randomArray[j] == randomArray[i])    //檢查是否與前面產生的數值發生重複，如果有就重新產生
                    {
                        j = 0;  //如有重複，將變數j設為0，再次檢查 (因為還是有重複的可能)
                        randomArray[i] = rnd.Next(1, range);   //重新產生，存回陣列，亂數產生的範圍是1~9
                    }
                }
            }
            return randomArray;
        }

        private void displayBtnText(int[] random, List<string> srcList)
        {
            var cls = this.Controls.Find(ansBtnName, true);
            for (int i = 0; i < cls.Length; i++)
            {
                ((Button)cls[i]).Text = srcList[random[i]];
            }
        }

        private void displayQuestion(int[] randAry, List<string> srcList, ref int ansIndex)
        {
            ansIndex = rnd.Next(0, randAry.Length);
            var cls = this.Controls.Find(quesLabelName, true);
            ((Label)cls[0]).Text = srcList[randAry[ansIndex]];
        }

        #endregion

        #region Check Answer

        private void ansBtn_click(object sender, EventArgs e)
        {
            var cls = this.Controls.Find(scoreLabelName, true);

            Button btn = sender as Button;
            if (checkAns(btn.Text, randomAry, answerList))
            {
                //MessageBox.Show("Good");
                displayQA();
                iQ += 1;
            }
            else
            {
                iQ -= 2;
                //MessageBox.Show("Bad");
            }
            ((Label)cls[0]).Text = String.Format("Your IQ:{0:D}", iQ);
        }

        private bool checkAns(string ans, int[] randAry, List<string> ansList)
        {
            if (realAnsIdx == -1)
            {
                return false;
            }
            if (ans == ansList[randAry[realAnsIdx]])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
