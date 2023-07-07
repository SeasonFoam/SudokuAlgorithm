using Microsoft.Office.Interop.Excel;
using SudokuAlgorithm;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Excel.Application;
using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;

namespace Sudoku
{
    public partial class FM_Main : Form
    {
        #region Variable

        private SudokuOrigin dataSource = new SudokuOrigin();
        private SudokuOrigin template = new SudokuOrigin();

        #endregion

        #region Constructor

        public FM_Main()
        {
            InitializeComponent();
        }

        #endregion

        #region System Event

        private void Event_Control_FormLoad(object sender, EventArgs e)
        {
            jisuancishu.Text = SudokuOrigin.Depth.ToString();
            comboBox1.SelectedIndex = 3;
        }
        private void Event_Control_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
                e.Handled = true;
        }
        private void Event_Control_TextChanged(object sender, EventArgs e)
        {
            TextBox text = sender as TextBox;

            if (text.Text == "")
                text.Text = 0.ToString();
            int number = int.Parse(text.Text);
            text.Text = number.ToString();
            if (number <= 9)
            {
                return;
            }
            text.Text = text.Text.Remove(text.Text.Length - 1);
            text.SelectionStart = text.Text.Length;
        }
        private void Event_Control_ButtonClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Equals(button1))
            {
                if (button1.Text.Equals("查看"))
                {
                    SudokuOrigin m = new SudokuOrigin();
                    m.Convert(dataSource);
                    dataSource.Convert(template);
                    template.Convert(dataSource);

                    UpdateMap(dataSource);
                }
                else if (button1.Text.Equals("开始计算"))
                {
                    InitData();

                    if (dataSource.CountValue() > 23)
                    {
                        button1.Enabled = false;
                        template.Convert(dataSource);

                        Thread td = new Thread(new ThreadStart(Deduction));
                        td.Start();
                    }
                    else
                    {
                        MessageBox.Show("节点数量不足以计算，请继续输入");
                    }
                }
            }
            else if (btn.Equals(button2))
            {
                SudokuOrigin m = new SudokuOrigin();
                m.Convert(dataSource);
                dataSource.Convert(template);
                template.Convert(dataSource);
                UpdateMap(dataSource);
            }
            else if (btn.Equals(button3))
            {
                dataSource.Clear();
                template.Clear();
                ClearMap();

                button1.Text = "开始计算";
                button2.Enabled = false;
                button4.Enabled = true;
            }
            else if (btn.Equals(button4))
            {
                dataSource.StartGame();
                UpdateMap(dataSource);
            }
        }
        private void Event_Control_ComboBoxSelectIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            dataSource.Level = combo.SelectedIndex + 1;
        }
        private void Event_Control_TSMIClick(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi_item = sender as ToolStripMenuItem;
            if (tsmi_item.Equals(tsmi_export))
            {
                Application excel = new Application();
                if (excel == null)
                {
                    MessageBox.Show("请确保您的电脑已经安装Excel", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                excel.UserControl = true;
                Workbooks workbooks = excel.Workbooks;
                Workbook workbook = workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                Worksheet worksheet = (Worksheet)workbook.Worksheets[1];
                if (worksheet == null)
                {
                    MessageBox.Show("请确保您的电脑已经安装Excel", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Range range;
                    long totalCount = dataSource.Origin.Length;
                    long rowRead = 0;
                    float percent = 0;
                    worksheet.Cells[1, 1] = "数独地图";//导出的标题
                    worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[1, dataSource.Map.ColumnMap.Count + 2]).MergeCells = true; //合并单元格---列数
                    worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[1, dataSource.Map.ColumnMap.Count + 2]).HorizontalAlignment = XlVAlign.xlVAlignCenter;//居中对齐

                    //写入数值
                    for (int r = 0; r < dataSource.Map.RowMap.Count; r++)
                    {
                        for (int i = 0; i < dataSource.Map.ColumnMap.Count; i++)
                        {
                            int value = dataSource.Origin[r][i].Value;
                            if (value == 0)
                                worksheet.Cells[r + 2 + r / 3, i + 1 + i / 3] = string.Join(",", dataSource.Origin[r][i].Probable.Select(x => x.ToString()).ToArray());
                            else
                                worksheet.Cells[r + 2 + r / 3, i + 1 + i / 3] = value;
                        }

                        rowRead++;
                        percent = ((float)(100 * rowRead)) / totalCount;
                        //System.Threading.Thread.Sleep(500);
                        //如果字的数量过多则自动换行。worksheet.Cells[r+1, 4]为worksheet.Cells[行, 列]
                        //worksheet.get_Range(worksheet.Cells[r + 3, 4], worksheet.Cells[r + 1, 4]).Columns.WrapText = true;   //自动换行
                        //worksheet.get_Range(worksheet.Cells[r + 3, 4], worksheet.Cells[r + 3, 4]).Rows.AutoFit(); //自动加行高
                        //worksheet.get_Range(worksheet.Cells[r + 3, 4], worksheet.Cells[r + 3, 4]).Columns.AutoFit();
                        //this.Text = "导出数据[" + percent.ToString("0.00") + "%]...";
                    }
                    range = worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[dataSource.Map.RowMap.Count + 3, dataSource.Map.ColumnMap.Count + 2]);
                    range.BorderAround(XlLineStyle.xlContinuous, XlBorderWeight.xlThin, XlColorIndex.xlColorIndexAutomatic, null);
                    range.Borders[XlBordersIndex.xlInsideHorizontal].ColorIndex = XlColorIndex.xlColorIndexAutomatic;
                    range.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                    range.Borders[XlBordersIndex.xlInsideHorizontal].Weight = XlBorderWeight.xlThin;
                    range.HorizontalAlignment = XlVAlign.xlVAlignCenter;
                    range.Columns.AutoFit();

                    if (dataSource.Map.ColumnMap.Count > 1)
                    {
                        range.Borders[XlBordersIndex.xlInsideVertical].ColorIndex = XlColorIndex.xlColorIndexAutomatic;
                        range.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
                        range.Borders[XlBordersIndex.xlInsideVertical].Weight = XlBorderWeight.xlThin;
                    }
                    excel.Visible = true;
                }
                catch
                {
                    MessageBox.Show("到出Excel失败！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbooks);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                    GC.Collect();//强行销毁
                }
            }
        }

        #endregion

        #region Private Event

        private void UpdateMap(SudokuOrigin source)
        {
            foreach (Control c in pl_content.Controls)
            {
                if (c is TextBox)
                {
                    string[] s_arr = c.Tag.ToString().Split(',');
                    SudokuNum itemObj = source.Origin[Convert.ToInt32(s_arr[0])][Convert.ToInt32(s_arr[1])];
                    c.Text = itemObj.Value.ToString();
                }
            }
        }
        private void InitData()
        {
            dataSource.Clear();
            template.Clear();

            foreach (Control c in pl_content.Controls)
            {
                if (c is TextBox
                    && !string.IsNullOrEmpty((c as TextBox).Text))
                {
                    int value = Convert.ToInt32((c as TextBox).Text);

                    if (value != 0)
                    {
                        string[] s_arr = c.Tag.ToString().Split(',');
                        SudokuNum itemObj = dataSource.Origin[Convert.ToInt32(s_arr[0])][Convert.ToInt32(s_arr[1])];
                        itemObj.Value = value;
                        itemObj.Probable = new System.Collections.Generic.List<int>() { value };
                    }
                }
            }
        }
        private void ClearMap()
        {
            foreach (Control c in pl_content.Controls)
            {
                if (c is TextBox)
                {
                    (c as TextBox).Text = string.Empty;
                }
            }
        }
        private void Deduction()
        {
            CheckForIllegalCrossThreadCalls = false;

            dataSource.Deduction();

            UpdateMap(dataSource);

            button2.Enabled = true;
            button1.Text = "查看";
            button4.Enabled = false;
            button1.Enabled = true;
        }

        #endregion
    }
}