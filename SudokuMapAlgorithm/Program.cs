using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuMapAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            bool on_off = false;

            DateTime beforDT = DateTime.Now;

            if (on_off)
            {
                Sudoku[][] dataSource = BlockAlgorithm.GenerateSource();

                #region 初始化第一宫

                BlockAlgorithm.InitSudokuMap(ref dataSource);

                #endregion

                #region 开始计算

                BlockAlgorithm.CalcNum(ref dataSource);

                #endregion

                DateTime afterDT = DateTime.Now;
                TimeSpan ts = afterDT.Subtract(beforDT);
                Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);
                Console.WriteLine();

                #region 打印Map

                int m = 0;
                for (int i = 0; i < dataSource.Length; i++)
                {
                    int n = 0;
                    for (int j = 0; j < dataSource[i].Length; j++)
                    {
                        Sudoku itemObj = dataSource[i][j];
                        Console.Write(itemObj.Value + ",");

                        if ((n + 1) % 3 == 0)
                            Console.Write("\t");

                        n++;
                    }

                    Console.WriteLine();
                    if ((m + 1) % 3 == 0)
                        Console.WriteLine();
                    m++;
                }

                #endregion

                #region 打印所有可能性
                /*
                for (int i = 0; i < dataSource.Length; i++)
                {
                    for (int j = 0; j < dataSource[i].Length; j++)
                    {
                        Sudoku itemObj = dataSource[i][j];
                        Console.Write(string.Join(",", itemObj.Probable.Select(x => x.ToString()).ToArray()) + "|");

                        if ((j + 1) % 3 == 0)
                            Console.Write("\t");
                    }

                    Console.WriteLine();
                    if ((i + 1) % 3 == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
                */
                #endregion
            }
            else
            {
                #region 初始化数据源

                SudokuOrigin dataSource = new SudokuOrigin();

                #endregion

                #region 数独游戏推演正确答案

                bool can = true;

                if (can)
                {
                    #region 字符串初始化
                    
                    List<string> deductionList = new List<string>()
                    {
                        "0,0,2",
                        "0,4,8",
                        "0,7,1",
                        "1,1,4",
                        "1,3,1",
                        "1,6,3",
                        "2,4,9",
                        "3,3,7",
                        "3,6,8",
                        "3,7,4",
                        "4,1,9",
                        "4,4,2",
                        "4,5,1",
                        "5,2,6",
                        "6,1,3",
                        "6,6,5",
                        "6,7,8",
                        "7,0,1",
                        "7,3,8",
                        "7,8,7",
                        "8,1,2",
                        "8,2,7",
                        "8,4,5",
                        "8,5,3"
                    };

                    foreach (string str in deductionList)
                    {
                        string[] arr = str.Split(',');

                        SudokuNum itemObj = dataSource.Origin[Convert.ToInt32(arr[0])][Convert.ToInt32(arr[1])];
                        itemObj.Value = Convert.ToInt32(arr[2]);
                        itemObj.Probable = new List<int>() { itemObj.Value };
                    }
                    #endregion

                    #region 按宫初始化节点值
                    /*
                    {
                        // 第一宫
                        SudokuBlock block = dataSource.GetBlock(0);

                        SudokuNum itemObj1 = block.Palace[0];
                        itemObj1.Value = 2;
                        itemObj1.Probable = new List<int>() { 6 };

                        SudokuNum itemObj2 = block.Palace[4];
                        itemObj2.Value = 8;
                        itemObj2.Probable = new List<int>() { 2 };
                    }

                    {
                        // 第二宫
                        SudokuBlock block = dataSource.GetBlock(1);

                        SudokuNum itemObj1 = block.Palace[1];
                        itemObj1.Value = 2;
                        itemObj1.Probable = new List<int>() { 2 };

                        SudokuNum itemObj2 = block.Palace[4];
                        itemObj2.Value = 7;
                        itemObj2.Probable = new List<int>() { 7 };
                    }

                    {
                        // 第三宫
                        SudokuBlock block = dataSource.GetBlock(2);

                        SudokuNum itemObj1 = block.Palace[1];
                        itemObj1.Value = 8;
                        itemObj1.Probable = new List<int>() { 8 };

                        SudokuNum itemObj2 = block.Palace[2];
                        itemObj2.Value = 5;
                        itemObj2.Probable = new List<int>() { 5 };

                        SudokuNum itemObj3 = block.Palace[7];
                        itemObj3.Value = 1;
                        itemObj3.Probable = new List<int>() { 1 };
                    }

                    {
                        // 第四宫
                        SudokuBlock block = dataSource.GetBlock(3);

                        SudokuNum itemObj1 = block.Palace[0];
                        itemObj1.Value = 7;
                        itemObj1.Probable = new List<int>() { 7 };

                        SudokuNum itemObj2 = block.Palace[4];
                        itemObj2.Value = 4;
                        itemObj2.Probable = new List<int>() { 4 };

                        SudokuNum itemObj3 = block.Palace[8];
                        itemObj3.Value = 9;
                        itemObj3.Probable = new List<int>() { 9 };
                    }

                    {
                        // 第五宫
                        SudokuBlock block = dataSource.GetBlock(4);

                        SudokuNum itemObj1 = block.Palace[0];
                        itemObj1.Value = 9;
                        itemObj1.Probable = new List<int>() { 9 };

                        SudokuNum itemObj2 = block.Palace[2];
                        itemObj2.Value = 4;
                        itemObj2.Probable = new List<int>() { 4 };

                        SudokuNum itemObj3 = block.Palace[8];
                        itemObj3.Value = 5;
                        itemObj3.Probable = new List<int>() { 5 };
                    }

                    {
                        // 第六宫
                        SudokuBlock block = dataSource.GetBlock(5);

                        SudokuNum itemObj1 = block.Palace[4];
                        itemObj1.Value = 3;
                        itemObj1.Probable = new List<int>() { 3 };

                        SudokuNum itemObj2 = block.Palace[7];
                        itemObj2.Value = 2;
                        itemObj2.Probable = new List<int>() { 2 };
                    }

                    {
                        // 第七宫
                        SudokuBlock block = dataSource.GetBlock(6);

                        SudokuNum itemObj1 = block.Palace[2];
                        itemObj1.Value = 5;
                        itemObj1.Probable = new List<int>() { 5 };

                        SudokuNum itemObj2 = block.Palace[3];
                        itemObj2.Value = 2;
                        itemObj2.Probable = new List<int>() { 2 };

                        SudokuNum itemObj3 = block.Palace[7];
                        itemObj3.Value = 3;
                        itemObj3.Probable = new List<int>() { 3 };
                    }

                    {
                        // 第八宫
                        SudokuBlock block = dataSource.GetBlock(7);

                        SudokuNum itemObj1 = block.Palace[4];
                        itemObj1.Value = 8;
                        itemObj1.Probable = new List<int>() { 8 };

                        SudokuNum itemObj2 = block.Palace[5];
                        itemObj2.Value = 6;
                        itemObj2.Probable = new List<int>() { 6 };

                        SudokuNum itemObj3 = block.Palace[8];
                        itemObj3.Value = 2;
                        itemObj3.Probable = new List<int>() { 2 };
                    }

                    {
                        // 第九宫
                        SudokuBlock block = dataSource.GetBlock(8);

                        SudokuNum itemObj1 = block.Palace[0];
                        itemObj1.Value = 2;
                        itemObj1.Probable = new List<int>() { 2 };

                        SudokuNum itemObj2 = block.Palace[1];
                        itemObj2.Value = 6;
                        itemObj2.Probable = new List<int>() { 6 };

                        SudokuNum itemObj3 = block.Palace[3];
                        itemObj3.Value = 1;
                        itemObj3.Probable = new List<int>() { 1 };

                        SudokuNum itemObj4 = block.Palace[7];
                        itemObj4.Value = 7;
                        itemObj4.Probable = new List<int>() { 7 };
                    }
                    */
                    #endregion

                    dataSource.Deduction();
                }
                else
                {
                    dataSource.GenerateMap();
                }

                #endregion

                DateTime afterDT = DateTime.Now;
                TimeSpan ts = afterDT.Subtract(beforDT);
                Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);
                Console.WriteLine();

                #region 打印Map

                int m = 0;
                for (int i = 0; i < dataSource.Origin.Length; i++)
                {
                    int n = 0;
                    for (int j = 0; j < dataSource.Origin[i].Length; j++)
                    {
                        SudokuNum itemObj = dataSource.Origin[i][j];
                        Console.Write(itemObj.Value + ",");

                        if ((n + 1) % 3 == 0)
                            Console.Write("\t");

                        n++;
                    }

                    Console.WriteLine();
                    if ((m + 1) % 3 == 0)
                        Console.WriteLine();
                    m++;
                }

                #endregion

                #region 打印所有可能性

                for (int i = 0; i < dataSource.Origin.Length; i++)
                {
                    for (int j = 0; j < dataSource.Origin[i].Length; j++)
                    {
                        SudokuNum itemObj = dataSource.Origin[i][j];
                        Console.Write(string.Join(",", itemObj.Probable.Select(x => x.ToString()).ToArray()) + "|");

                        if ((j + 1) % 3 == 0)
                            Console.Write("\t");
                    }

                    Console.WriteLine();
                    if ((i + 1) % 3 == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }

                #endregion
            }


            Console.ReadKey(false);
        }
    }
}