using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuMapAlgorithm
{
    public class Block
    {
        public List<Sudoku> Palace = new List<Sudoku>();
        public int index;

        public Block Copy()
        {
            Block copy = new Block
            {
                index = index
            };
            foreach (Sudoku itemObj in Palace)
            {
                copy.Palace.Add(itemObj.Copy());
            }

            return copy;
        }

        /// <summary>
        /// 筛选存在唯一值节点
        /// </summary>
        public void Screen()
        {
            int unique = 0;
            do
            {
                unique = CheckBlockProbable();
                if (unique > 0)
                {
                    List<Sudoku> list = Palace.Where(x => x.Probable.Contains(unique)).ToList();
                    Sudoku itemObj = null;
                    if (list.Count == 1)
                        itemObj = Palace.Where(x => x.Probable.Contains(unique)).FirstOrDefault();
                    else
                        itemObj = Palace.Where(x => x.Probable.Contains(unique) && x.Probable.Count == 1).FirstOrDefault();
                    itemObj.Value = unique;
                    itemObj.Probable = new List<int>() { unique };
                }

            } while (unique > 0);
        }

        /// <summary>
        /// 检查宫(除值已确定的节点外)是否存在唯一可能性
        /// </summary>
        /// <returns>若存在，则返回唯一值；若不存在，则返回0</returns>
        public int CheckBlockProbable()
        {
            /**
             * 1.1-9数值中查找唯一可能性，虽然可能性组合中存在其他可能性，但其值唯一
             * 2.当可能性组合中唯一值时，该值为真
             * 3.筛选出值为0的项
             **/

            List<Sudoku> block_list = Palace.Where(x => x.Value == 0).ToList();
            if (block_list.Count <= 0)
                return 0;

            #region 可能性组合中Count只有1时，值为唯一值

            List<Sudoku> tmp_list = block_list.Where(x => x.Probable.Count == 1).ToList();
            if (tmp_list.Count > 0)
                return tmp_list.FirstOrDefault().Probable.FirstOrDefault();

            #endregion

            #region 1-9数值统计可能性组合数量

            List<int> probable = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 1; i < 10; i++)
            {
                int count = block_list.Select(x => x.Probable.Where(y => y == i)).Where(y => y.Count() > 0).Count();
                probable[i - 1] = count;
            }

            #endregion

            List<int> temp = new List<int>(probable);
            temp = temp.Except(new List<int>() { 0, 0, 0 }).ToList();
            if (temp.Count <= 0) return 0;
            int min_index = probable.IndexOf(temp.Min());
            if (probable[min_index] == 1)
                return min_index + 1;
            else
                return 0;
        }

        public List<int> GetProbableList()
        {
            List<Sudoku> block_list = Palace.Where(x => x.Value == 0).ToList();
            if (block_list.Count <= 0)
                return new List<int>();

            #region 1-9数值统计可能性组合数量

            List<int> probable = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<int> sort = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 1; i < 10; i++)
            {
                int count = block_list.Select(x => x.Probable.Where(y => y == i)).Where(y => y.Count() > 0).Count();
                probable[i - 1] = count;

                sort[i - 1] = i;
            }

            int temp;
            for (int i = 0; i < probable.Count - 1; i++)
            {
                for (int j = 0; j < probable.Count - 1 - i; j++)
                {
                    if (probable[j] > probable[j + 1])
                    {
                        temp = probable[j];
                        probable[j] = probable[j + 1];
                        probable[j + 1] = temp;

                        temp = sort[j];
                        sort[j] = sort[j + 1];
                        sort[j + 1] = temp;
                    }
                }
            }

            List<int> comb = probable.Distinct().ToList();
            int comb_min = probable.Min();

            for (int i = 0; i < probable.Count; i++)
            {
                Console.Write(probable[i] + "\t");
                if ((i + 1) % 3 == 0)
                    Console.WriteLine();
            }
            Console.WriteLine();

            #endregion

            return sort;
        }
    }

    public class Sudoku
    {
        private bool on_off = true;
        private int value = 0;

        public int Row;
        public int Column;
        public int Index;
        public List<Block> BlockSource = new List<Block>();
        public List<int> Probable = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        public Sudoku[][] DataSource;

        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;

                ReplaceProbable();
            }
        }

        public Sudoku Copy()
        {
            Sudoku tmp = new Sudoku
            {
                value = value,
                Row = Row,
                Column = Column,
                Index = Index,
                BlockSource = BlockSource,
                Probable = new List<int>(Probable)
            };

            return tmp;
        }

        /// <summary>
        /// 约束检测
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Constraint(int value)
        {
            /**
             * 共三个约束：宫、行、列
             */

            #region 宫约束检测

            Block block = BlockSource[Index];
            int result = block.CheckBlockProbable();
            if (result > 0)
            {
                Sudoku itemObj = block.Palace.Where(x => x.Probable.Contains(result)).FirstOrDefault();
                if (itemObj.Equals(this)
                    && result == value)
                    return true;
                else if (itemObj.Equals(this)
                    && result != value)
                    return false;
                else if (!itemObj.Equals(this)
                    && result == value)
                    return false;
            }

            #endregion

            #region 行约束检测

            List<Sudoku> row = new List<Sudoku>(DataSource[Row]);
            result = CheckProbable(row);
            if (result > 0)
            {
                Sudoku itemObj = row.Where(x => x.Probable.Contains(result)).FirstOrDefault();
                if (itemObj.Equals(this)
                    && result == value)
                    return true;
                else if (itemObj.Equals(this)
                    && result != value)
                    return false;
                else if (!itemObj.Equals(this)
                    && result == value)
                    return false;
            }

            #endregion

            #region 列约束检测

            List<Sudoku> column = new List<Sudoku>();
            foreach (Sudoku[] row_tmp in DataSource)
            {
                column.Add(row_tmp[Column]);
            }
            result = CheckProbable(column);
            if (result > 0)
            {
                Sudoku itemObj = column.Where(x => x.Probable.Contains(result)).FirstOrDefault();
                if (itemObj.Equals(this)
                    && result == value)
                    return true;
                else if (itemObj.Equals(this)
                    && result != value)
                    return false;
                else if (!itemObj.Equals(this)
                    && result == value)
                    return false;
            }

            #endregion

            return true;
        }

        /// <summary>
        /// 除已知节点外检测唯一值
        /// </summary>
        /// <param name="list"></param>
        public int CheckProbable(List<Sudoku> list)
        {
            list = list.Where(x => x.Value == 0).ToList();
            if (list.Count <= 0) return 0;

            #region 可能性组合中Count只有1时，值为唯一值

            List<Sudoku> tmp_list = list.Where(x => x.Probable.Count == 1).ToList();
            if (tmp_list.Count > 0)
                return tmp_list.FirstOrDefault().Probable.FirstOrDefault();

            #endregion

            #region 1-9数值统计可能性组合数量

            List<int> probable = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 1; i < 10; i++)
            {
                int count = list.Select(x => x.Probable.Where(y => y == i)).Where(y => y.Count() > 0).Count();
                probable[i - 1] = count;
            }

            #endregion

            List<int> temp = new List<int>(probable);
            temp = temp.Except(new List<int>() { 0, 0, 0 }).ToList();
            if (temp.Count <= 0)
                return 0;
            int min_index = probable.IndexOf(temp.Min());
            if (probable[min_index] == 1)
                return min_index + 1;
            else
                return 0;
        }

        public void ReplaceProbable()
        {
            #region 相同宫可能性筛除

            Block block = BlockSource[Index];
            foreach (Sudoku tmp in block.Palace)
            {
                if (!tmp.Equals(this))
                {
                    tmp.Probable.Remove(Value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && on_off)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }

            #endregion

            #region 相同行可能性筛除

            Sudoku[] row_arr = DataSource[Row];
            foreach (Sudoku tmp in row_arr)
            {
                if (!tmp.Equals(this))
                {
                    tmp.Probable.Remove(Value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && on_off)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }

            #endregion

            #region 相同列可能性筛除

            List<Sudoku> column_list = new List<Sudoku>();
            foreach (Sudoku[] row in DataSource)
            {
                foreach (Sudoku tmp in row)
                {
                    if (tmp.Column == Column
                        && !tmp.Equals(this))
                    {
                        tmp.Probable.Remove(Value);
                        if (tmp.Probable.Count == 1
                            && tmp.Value == 0
                            && on_off)
                            tmp.Value = tmp.Probable.FirstOrDefault();
                    }
                }
            }

            #endregion
        }
    }

    public class BlockAlgorithm
    {
        public static void CalcNum(ref Sudoku[][] dataSource)
        {
            List<Block> blockSource = dataSource[0][0].BlockSource;

            foreach (Block block in blockSource)
            {
                do
                {
                    block.Screen();

                    List<Sudoku> list = block.Palace.Where(x => x.Value == 0).OrderBy(y => y.Probable.Count).ToList();
                    if (list.Count <= 0) break;
                    Sudoku itemObj = list.FirstOrDefault();

                    int value = 0;
                    if (itemObj.Probable.Count <= 0)
                        value = 10;
                    else
                        value = itemObj.Probable[GetRandom(itemObj.Probable.Count)];
                    if (itemObj.Constraint(value))
                    {
                        itemObj.Value = value;
                        itemObj.Probable = new List<int>() { itemObj.Value };
                    }
                    else
                    {
                        if (itemObj.Probable.Contains(value))
                            itemObj.Probable.Remove(value);
                    }

                } while (block.Palace.Where(x => x.Value == 0).Count() > 0);
            }
        }

        /// <summary>
        /// 随机打乱数组
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static List<int> Sort(List<int> arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                var index = new Random(unchecked((int)DateTime.Now.Ticks)).Next(i, arr.Count);
                var tmp = arr[i];
                var ran = arr[index];
                arr[i] = ran;
                arr[index] = tmp;
            }
            return arr;
        }

        /// <summary>
        /// 通过DateTime.Now.Ticks 获取随机数
        /// </summary>
        /// <param name="max">最大值</param>
        /// <returns>获取随机数</returns>
        private static int GetRandom(int max)
        {
            Random rd = new Random(unchecked((int)DateTime.Now.Ticks));
            //System.Threading.Thread.Sleep(100);

            return rd.Next(max);
        }

        public static Sudoku[][] GenerateSource()
        {
            List<Block> blockSource = new List<Block>();
            Sudoku[][] dataSource = new Sudoku[9][];
            for (int i = 0; i < dataSource.Length; i++)
            {
                dataSource[i] = new Sudoku[9];
            }

            for (int i = 0; i < 9; i++)
            {
                Block block = new Block
                {
                    index = i
                };

                blockSource.Add(block);

                for (int j = 0; j < 9; j++)
                {
                    Sudoku itemObj = new Sudoku
                    {
                        Row = j / 3 + i / 3 * 3,
                        Column = j % 3 + i % 3 * 3,
                        Index = i,
                        BlockSource = blockSource,
                        DataSource = dataSource
                    };

                    block.Palace.Add(itemObj);
                    dataSource[itemObj.Row][itemObj.Column] = itemObj;
                }
            }

            return dataSource;
        }

        /// <summary>
        /// 初始化第一宫的内容后开始推演剩余节点
        /// </summary>
        /// <param name="dataSource"></param>
        public static void InitSudokuMap(ref Sudoku[][] dataSource)
        {
            List<int> arr = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            arr = Sort(arr);

            //arr = new List<int>() { 5, 6, 2, 7, 4, 8, 3, 9, 1 };

            Block block = dataSource[0][0].BlockSource.FirstOrDefault();
            for (int i = 0; i < arr.Count; i++)
            {
                Sudoku itemObj = block.Palace[i];
                itemObj.Value = arr[i];
                itemObj.Probable = new List<int>() { arr[i] };
            }

            /*
            arr = new List<int>() { 4, 3, 7, 9, 1, 6, 5, 2, 8 };
            block = dataSource[0][0].BlockSource[1];
            for (int i = 0; i < arr.Count; i++)
            {
                Sudoku itemObj = block.Palace[i];
                itemObj.Value = arr[i];
                itemObj.Probable = new List<int>() { arr[i] };
            }

            arr = new List<int>() { 9, 8, 1, 5, 3, 2, 7, 6, 4 };
            block = dataSource[0][0].BlockSource[2];
            for (int i = 0; i < arr.Count; i++)
            {
                Sudoku itemObj = block.Palace[i];
                itemObj.Value = arr[i];
                itemObj.Probable = new List<int>() { arr[i] };
            }

            Sudoku itemObj1 = dataSource[0][3];
            itemObj1.Value = 4;
            itemObj1.Probable = new List<int>() { 4 };

            Sudoku itemObj2 = dataSource[0][4];
            itemObj2.Value = 3;
            itemObj2.Probable = new List<int>() { 3 };

            Sudoku itemObj3 = dataSource[0][5];
            itemObj3.Value = 7;
            itemObj3.Probable = new List<int>() { 7 };

            Sudoku itemObj4 = dataSource[1][3];
            itemObj4.Probable = new List<int>() { 1, 6, 9 };

            Sudoku itemObj5 = dataSource[1][3];
            itemObj5.Probable = new List<int>() { 1, 6, 9 };

            Sudoku itemObj6 = dataSource[1][3];
            itemObj6.Probable = new List<int>() { 1, 6, 9 };

            Sudoku itemObj7 = dataSource[2][3];
            itemObj7.Value = 5;
            itemObj7.Probable = new List<int>() { 5 };

            Sudoku itemObj8 = dataSource[2][4];
            itemObj8.Value = 2;
            itemObj8.Probable = new List<int>() { 2 };

            Sudoku itemObj9 = dataSource[2][5];
            itemObj9.Probable = new List<int>() { 6, 8 };
            */
        }
    }
}