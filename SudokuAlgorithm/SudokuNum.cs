using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuAlgorithm
{
    public class SudokuOrigin
    {
        public static readonly bool Switch = true;
        public static int Depth = 100;
        public int Level = 4;

        public SudokuNum[][] Origin = new SudokuNum[9][];
        public SudokuMap Map = new SudokuMap();

        public SudokuOrigin()
        {
            for (int i = 0; i < Origin.Length; i++)
            {
                Origin[i] = new SudokuNum[9];
            }

            for (int i = 0; i < 9; i++)
            {
                #region 初始化宫

                SudokuBlock block = new SudokuBlock
                {
                    Index = i
                };

                Map.BlockMap.Add(block);

                for (int j = 0; j < 9; j++)
                {
                    SudokuNum itemObj = new SudokuNum
                    {
                        Row = j / 3 + i / 3 * 3,
                        Column = j % 3 + i % 3 * 3,
                        Index = i,
                        Origin = this
                    };

                    block.Palace.Add(itemObj);
                    Origin[itemObj.Row][itemObj.Column] = itemObj;
                }

                #endregion
            }

            for (int i = 0; i < 9; i++)
            {
                #region 初始化行

                SudokuRow row = new SudokuRow
                {
                    Palace = new List<SudokuNum>(Origin[i]),
                    Row = i
                };
                Map.RowMap.Add(row);

                #endregion

                #region 初始化列

                SudokuColumn column = new SudokuColumn
                {
                    Column = i
                };
                for (int j = 0; j < 9; j++)
                {
                    column.Palace.Add(Origin[j][i]);
                }
                Map.ColumnMap.Add(column);

                #endregion
            }
        }

        public SudokuBlock GetBlock(int index = 0)
        {
            return Map.BlockMap[index];
        }

        public SudokuRow GetRow(int index = 0)
        {
            return Map.RowMap[index];
        }

        public SudokuColumn GetColumn(int index = 0)
        {
            return Map.ColumnMap[index];
        }

        public void GenerateMap()
        {
            //bool status = true;
            Algorithm.InitMap(this);

            foreach (SudokuBlock block in Map.BlockMap)
            {
                do
                {
                    block.Screen();

                    List<SudokuNum> list = block.Palace.Where(x => x.Value == 0).OrderBy(y => y.Probable.Count).ToList();
                    if (list.Count <= 0) break;
                    SudokuNum itemObj = list.FirstOrDefault();

                    int value = 0;
                    if (itemObj.Probable.Count <= 0)
                        value = 10;
                    else
                        value = itemObj.Probable[Algorithm.GetRandom(itemObj.Probable.Count)];
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

            //List<List<SudokuNum>> temp = Origin.Select(x => x.Where(y => y.Probable.Count == 0).ToList()).ToList();
            //status = temp.Where(x => x.Count > 0).Count() > 0;
        }

        public void Deduction()
        {
            /**
             * 1.当宫行列内存在可能性组合长度等于相同数量时，如两个25|25，则其余节点必不可能存在2和5
             * 2.当宫内存在可能性数字成行或列时且宫其他节点不存在该数字时，同行或列内其余节点必不存在该数字
             * 3.节点存在唯一可能性值
             */

            /**
             * 1.先查找全域唯一值，当有节点确定时从新开始遍历
             * 2.查找宫内相同项，判断是否成行列
             * 3.元素相同项筛除其他节点互斥元素
             */

            int circulate = Depth;
            bool status = true;
            while (circulate > 0)
            {
                List<SudokuNum> unique_list = UniqueValue();
                while (unique_list.Count > 0)
                {
                    foreach (SudokuNum itemObj in unique_list)
                    {
                        itemObj.Value = itemObj.GetUniqueValue();
                        itemObj.Probable = new List<int>() { itemObj.Value };
                    }

                    unique_list = UniqueValue();

                    status = false;
                }

                BlockSameItemMutualExclusion();

                CombinationScreen();

                if (status)
                {
                    SudokuNum tempItem = BranchDeduction();
                    if (tempItem != null)
                    {
                        SudokuNum itemObj = Origin[tempItem.Row][tempItem.Column];
                        if (tempItem.Probable.Count == 1)
                        {
                            itemObj.Value = tempItem.Value;
                            itemObj.Probable = new List<int>() { itemObj.Value };
                        }
                        else
                        {
                            itemObj.Probable.Remove(tempItem.Value);
                        }
                    }
                }

                if (CheckResult())
                    break;

                circulate--;
                status = true;
            }
        }

        public SudokuNum BranchDeduction()
        {
            SudokuOrigin dataSource = Copy();

            SudokuNum itemObj = dataSource.GetVoidNum().FirstOrDefault();

            if (itemObj != null)
            {
                int value = itemObj.Probable[Algorithm.GetRandom(itemObj.Probable.Count)];
                List<int> probable = new List<int>(itemObj.Probable);
                itemObj.Value = value;
                itemObj.Probable = new List<int>() { itemObj.Value };

                dataSource.Deduction();

                bool status = true;
                foreach (SudokuNum[] row in dataSource.Origin)
                {
                    foreach (SudokuNum item in row)
                    {
                        status = item.Probable.Count == 1;
                        if (!status) break;
                    }
                    if (!status) break;
                }

                if (status)
                    itemObj.Probable = new List<int>() { itemObj.Value };
                else
                    itemObj.Probable = new List<int>(probable);
            }
            return itemObj;
        }

        public void StartGame()
        {
            Clear();
            GenerateMap();

            int count = 0;
            if (Level == 4)
                count = 57;
            else if (Level == 3)
                count = 55;
            else if (Level == 2)
                count = 50;
            else if (Level == 1)
                count = 48;

            List<int> curList = new List<int>();
            for (int i = 0; i < count; i++)
            {
                int randomNum = Algorithm.GetRandom(81);
                while (curList.Contains(randomNum))
                    randomNum = Algorithm.GetRandom(81);
                int row = randomNum / 9;
                int column = randomNum % 9;

                SudokuNum itemObj = Origin[row][column];
                itemObj.Value = 0;
                itemObj.Probable = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                curList.Add(randomNum);
            }

            foreach (SudokuNum[] row in Origin)
            {
                foreach (SudokuNum itemObj in row)
                {
                    if(itemObj.Value != 0)
                    {
                        itemObj.Value = itemObj.Value;
                    }
                }
            }
        }

        /// <summary>
        /// 查找全域唯一值
        /// </summary>
        public List<SudokuNum> UniqueValue()
        {
            List<SudokuNum> unique_arr = new List<SudokuNum>();

            for (int i = 1; i < 10; i++)
            {
                // 宫查询
                foreach (SudokuBlock block in Map.BlockMap)
                {
                    List<SudokuNum> result = block.Palace.Where(x => x.Probable.Contains(i) && x.Value == 0).ToList();
                    if (result.Count == 1)
                        unique_arr.Add(result.FirstOrDefault());
                }

                // 行查询
                foreach (SudokuRow row in Map.RowMap)
                {
                    List<SudokuNum> result = row.Palace.Where(x => x.Probable.Contains(i) && x.Value == 0).ToList();
                    if (result.Count == 1)
                        unique_arr.Add(result.FirstOrDefault());
                }

                // 列查询
                foreach (SudokuColumn column in Map.ColumnMap)
                {
                    List<SudokuNum> result = column.Palace.Where(x => x.Probable.Contains(i) && x.Value == 0).ToList();
                    if (result.Count == 1)
                        unique_arr.Add(result.FirstOrDefault());
                }
            }

            return unique_arr.Distinct().ToList();
        }

        /// <summary>
        /// 宫元素成行列且唯一判断
        /// </summary>
        public void BlockSameItemMutualExclusion()
        {
            foreach (SudokuBlock block in Map.BlockMap)
            {
                for (int i = 1; i < 10; i++)
                {
                    List<SudokuNum> comb = block.Palace.Where(x => x.Probable.Contains(i) && x.Value == 0).ToList();
                    if (comb.Count > 1)
                    {
                        bool isRow = true;
                        bool isColumn = true;
                        SudokuNum firstItem = comb.First();
                        for (int j = 1; j < comb.Count; j++)
                        {
                            if (isRow)
                                isRow = firstItem.Row == comb[j].Row;
                            if (isColumn)
                                isColumn = firstItem.Column == comb[j].Column;
                        }

                        if (isRow)
                        {
                            SudokuRow row = GetRow(firstItem.Row);
                            row.Replace(i, comb);
                        }
                        else if (isColumn)
                        {
                            SudokuColumn column = GetColumn(firstItem.Column);
                            column.Replace(i, comb);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查找宫行列可能性组合相同项，判断长度与个数相等时，筛除同宫、行、列其他节点可能性组合中存在元素
        /// </summary>
        public void CombinationScreen()
        {
            List<List<SudokuNum>> list = new List<List<SudokuNum>>();

            foreach (SudokuBlock block in Map.BlockMap)
            {
                for (int i = 0; i < 9; i++)
                {
                    SudokuNum itemObj1 = block.Palace[i];
                    if (itemObj1.Value != 0) continue;

                    List<SudokuNum> num = new List<SudokuNum> { itemObj1 };
                    for (int j = i + 1; j < 9; j++)
                    {
                        SudokuNum itemObj2 = block.Palace[j];
                        if (itemObj2.Value != 0) continue;

                        if (itemObj1.Probable.Except(itemObj2.Probable).Count() == 0
                            && itemObj1.Probable.Count == itemObj2.Probable.Count)
                        {
                            num.Add(itemObj2);
                        }
                    }

                    if (num.Count > 1)
                    {
                        if (num.Count == num.FirstOrDefault().Probable.Count)
                        {
                            List<int> probable = new List<int>(itemObj1.Probable);
                            foreach (int m in probable)
                            {
                                block.Replace(m, num);
                            }
                        }
                    }
                }
            }

            foreach (SudokuRow row in Map.RowMap)
            {
                for (int i = 0; i < 9; i++)
                {
                    SudokuNum itemObj1 = row.Palace[i];
                    if (itemObj1.Value != 0) continue;
                    List<SudokuNum> num = new List<SudokuNum> { itemObj1 };
                    for (int j = i + 1; j < 9; j++)
                    {
                        SudokuNum itemObj2 = row.Palace[j];
                        if (itemObj2.Value != 0) continue;

                        if (itemObj1.Probable.Except(itemObj2.Probable).Count() == 0
                            && itemObj1.Probable.Count == itemObj2.Probable.Count)
                        {
                            num.Add(itemObj2);
                        }
                    }

                    if (num.Count > 1)
                    {
                        if (num.Count == num.FirstOrDefault().Probable.Count)
                        {
                            List<int> probable = new List<int>(itemObj1.Probable);
                            foreach (int m in probable)
                            {
                                row.Replace(m, num);
                            }
                        }
                    }
                }
            }

            foreach (SudokuColumn column in Map.ColumnMap)
            {
                for (int i = 0; i < 9; i++)
                {
                    SudokuNum itemObj1 = column.Palace[i];
                    if (itemObj1.Value != 0) continue;
                    List<SudokuNum> num = new List<SudokuNum> { itemObj1 };
                    for (int j = i + 1; j < 9; j++)
                    {
                        SudokuNum itemObj2 = column.Palace[j];
                        if (itemObj2.Value != 0) continue;

                        if (itemObj1.Probable.Except(itemObj2.Probable).Count() == 0
                            && itemObj1.Probable.Count == itemObj2.Probable.Count)
                        {
                            num.Add(itemObj2);
                        }
                    }

                    if (num.Count > 1)
                    {
                        if (num.Count == num.FirstOrDefault().Probable.Count)
                        {
                            List<int> probable = new List<int>(itemObj1.Probable);
                            foreach (int m in probable)
                            {
                                column.Replace(m, num);
                            }
                        }
                    }
                }
            }
        }

        public bool CheckResult()
        {
            bool result = true;
            foreach (SudokuNum[] row in Origin)
            {
                foreach (SudokuNum itemObj in row)
                {
                    result = itemObj.Value != 0;

                    if (!result) break;
                }

                if (!result) break;
            }
            return result;
        }

        /// <summary>
        /// 复制当前数据源
        /// </summary>
        /// <returns></returns>
        public SudokuOrigin Copy()
        {
            SudokuOrigin origin = new SudokuOrigin();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    SudokuNum itemObjNex = origin.Origin[i][j];
                    SudokuNum itemObjPrev = Origin[i][j];
                    if (itemObjPrev.Value != 0)
                    {
                        itemObjNex.Value = itemObjPrev.Value;
                        itemObjNex.Probable = new List<int>() { itemObjNex.Value };
                    }
                }
            }
            return origin;
        }

        public List<SudokuNum> GetVoidNum()
        {
            List<SudokuNum> list = new List<SudokuNum>();
            foreach (SudokuNum[] row in Origin)
            {
                foreach (SudokuNum itemObj in row)
                {
                    if (itemObj.Value == 0)
                        list.Add(itemObj);
                }
            }
            return list;
        }

        public void Clear()
        {
            foreach (SudokuNum[] row in Origin)
            {
                foreach (SudokuNum itemObj in row)
                {
                    itemObj.Value = 0;
                    itemObj.Probable = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                }
            }
        }

        public int CountValue()
        {
            int count = 0;
            foreach (SudokuNum[] row in Origin)
            {
                foreach (SudokuNum itemObj in row)
                {
                    if (itemObj.Value > 0
                        && itemObj.Value < 10)
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 复制另一个数据源
        /// </summary>
        /// <param name="target"></param>
        public void Convert(SudokuOrigin target)
        {
            foreach (SudokuNum[] row in target.Origin)
            {
                foreach (SudokuNum itemObj in row)
                {
                    SudokuNum current = Origin[itemObj.Row][itemObj.Column];
                    current.Value = itemObj.Value;
                    current.Probable = new List<int>(itemObj.Probable);
                }
            }
        }
    }

    public class SudokuMap
    {
        public List<SudokuBlock> BlockMap = new List<SudokuBlock>();
        public List<SudokuRow> RowMap = new List<SudokuRow>();
        public List<SudokuColumn> ColumnMap = new List<SudokuColumn>();
    }

    public class SudokuBlock
    {
        public List<SudokuNum> Palace = new List<SudokuNum>();
        public int Index;

        /// <summary>
        /// 筛选存在唯一值节点
        /// </summary>
        public void Screen()
        {
            int unique = 0;
            do
            {
                unique = CheckProbable();
                if (unique > 0)
                {
                    List<SudokuNum> list = Palace.Where(x => x.Probable.Contains(unique)).ToList();
                    SudokuNum itemObj = null;
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
        public int CheckProbable()
        {
            /**
             * 1.1-9数值中查找唯一可能性，虽然可能性组合中存在其他可能性，但其值唯一
             * 2.当可能性组合中唯一值时，该值为真
             * 3.筛选出值为0的项
             **/

            List<SudokuNum> block_list = Palace.Where(x => x.Value == 0).ToList();
            if (block_list.Count <= 0)
                return 0;

            #region 可能性组合中Count只有1时，值为唯一值

            List<SudokuNum> tmp_list = block_list.Where(x => x.Probable.Count == 1).ToList();
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

        public void Replace(int value, List<SudokuNum> specList = null)
        {
            foreach (SudokuNum tmp in Palace)
            {
                if (specList == null
                    || (specList != null && !specList.Contains(tmp)))
                {
                    tmp.Probable.Remove(value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && SudokuOrigin.Switch)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }
        }
    }

    public class SudokuRow
    {
        public List<SudokuNum> Palace = new List<SudokuNum>();
        public int Row;

        /// <summary>
        /// 筛选存在唯一值节点
        /// </summary>
        public void Screen()
        {
            int unique = 0;
            do
            {
                unique = CheckProbable();
                if (unique > 0)
                {
                    List<SudokuNum> list = Palace.Where(x => x.Probable.Contains(unique)).ToList();
                    SudokuNum itemObj = null;
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
        /// 除已知节点外检测唯一值
        /// </summary>
        /// <param name="list"></param>
        public int CheckProbable()
        {
            List<SudokuNum> list = Palace.Where(x => x.Value == 0).ToList();
            if (list.Count <= 0) return 0;

            #region 可能性组合中Count只有1时，值为唯一值

            List<SudokuNum> tmp_list = list.Where(x => x.Probable.Count == 1).ToList();
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

        public void Replace(int value, List<SudokuNum> specList = null)
        {
            foreach (SudokuNum tmp in Palace)
            {
                if (specList == null
                    || (specList != null && !specList.Contains(tmp)))
                {
                    tmp.Probable.Remove(value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && SudokuOrigin.Switch)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }
        }
    }

    public class SudokuColumn
    {
        public List<SudokuNum> Palace = new List<SudokuNum>();
        public int Column;

        /// <summary>
        /// 筛选存在唯一值节点
        /// </summary>
        public void Screen()
        {
            int unique = 0;
            do
            {
                unique = CheckProbable();
                if (unique > 0)
                {
                    List<SudokuNum> list = Palace.Where(x => x.Probable.Contains(unique)).ToList();
                    SudokuNum itemObj = null;
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
        /// 除已知节点外检测唯一值
        /// </summary>
        /// <param name="list"></param>
        public int CheckProbable()
        {
            List<SudokuNum> list = Palace.Where(x => x.Value == 0).ToList();
            if (list.Count <= 0) return 0;

            #region 可能性组合中Count只有1时，值为唯一值

            List<SudokuNum> tmp_list = list.Where(x => x.Probable.Count == 1).ToList();
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

        public void Replace(int value, List<SudokuNum> specList = null)
        {
            foreach (SudokuNum tmp in Palace)
            {
                if (specList == null
                    || (specList != null && !specList.Contains(tmp)))
                {
                    tmp.Probable.Remove(value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && SudokuOrigin.Switch)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }
        }
    }

    public class SudokuNum
    {
        private int value;

        public SudokuOrigin Origin;

        public int Row;
        public int Column;
        public int Index;

        public List<int> Probable = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;

                if (value != 0)
                    Replace();
            }
        }

        public void Replace()
        {
            #region 相同宫可能性筛除

            BlockReplace(Value);

            #endregion

            #region 相同行可能性筛除

            RowReplace(Value);

            #endregion

            #region 相同列可能性筛除

            ColumnReplace(Value);

            #endregion
        }

        public void BlockReplace(int value)
        {
            SudokuBlock block = Origin.GetBlock(Index);
            foreach (SudokuNum tmp in block.Palace)
            {
                if (!tmp.Equals(this))
                {
                    tmp.Probable.Remove(value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && SudokuOrigin.Switch)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }
        }

        public void RowReplace(int value)
        {
            SudokuRow row = Origin.GetRow(Row);
            foreach (SudokuNum tmp in row.Palace)
            {
                if (!tmp.Equals(this))
                {
                    tmp.Probable.Remove(value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && SudokuOrigin.Switch)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }
        }

        public void ColumnReplace(int value)
        {
            SudokuColumn column = Origin.GetColumn(Column);
            foreach (SudokuNum tmp in column.Palace)
            {
                if (!tmp.Equals(this))
                {
                    tmp.Probable.Remove(value);
                    if (tmp.Probable.Count == 1
                        && tmp.Value == 0
                        && SudokuOrigin.Switch)
                        tmp.Value = tmp.Probable.FirstOrDefault();
                }
            }
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

            SudokuBlock block = Origin.GetBlock(Index);
            int result = block.CheckProbable();
            if (result > 0)
            {
                SudokuNum itemObj = block.Palace.Where(x => x.Probable.Contains(result)).FirstOrDefault();
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

            SudokuRow row = Origin.GetRow(Row);
            result = row.CheckProbable();
            if (result > 0)
            {
                SudokuNum itemObj = row.Palace.Where(x => x.Probable.Contains(result)).FirstOrDefault();
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

            SudokuColumn column = Origin.GetColumn(Column);
            result = column.CheckProbable();
            if (result > 0)
            {
                SudokuNum itemObj = column.Palace.Where(x => x.Probable.Contains(result)).FirstOrDefault();
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

        public int GetUniqueValue()
        {
            int result = 0;
            foreach (int tmp in Probable)
            {
                SudokuBlock block = Origin.Map.BlockMap[Index];
                List<SudokuNum> tmpList = block.Palace.Where(x => x.Probable.Contains(tmp)).ToList();
                if (tmpList.Count == 1)
                {
                    result = tmp;
                    break;
                }

                SudokuRow row = Origin.Map.RowMap[Row];
                tmpList = row.Palace.Where(x => x.Probable.Contains(tmp)).ToList();
                if (tmpList.Count == 1)
                {
                    result = tmp;
                    break;
                }

                SudokuColumn column = Origin.Map.ColumnMap[Column];
                tmpList = column.Palace.Where(x => x.Probable.Contains(tmp)).ToList();
                if (tmpList.Count == 1)
                {
                    result = tmp;
                    break;
                }
            }

            return result;
        }
    }

    public static class Algorithm
    {
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
        public static int GetRandom(int max)
        {
            Random rd = new Random(unchecked((int)DateTime.Now.Ticks));
            //System.Threading.Thread.Sleep(100);

            return rd.Next(max);
        }

        /// <summary>
        /// 默认第一宫节点
        /// </summary>
        /// <param name="dataSource"></param>
        public static void InitMap(SudokuOrigin dataSource)
        {
            List<int> arr = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            arr = Sort(arr);

            //arr = new List<int>() { 5, 6, 2, 7, 4, 8, 3, 9, 1 };

            SudokuBlock block = dataSource.GetBlock();
            for (int i = 0; i < arr.Count; i++)
            {
                SudokuNum itemObj = block.Palace[i];
                itemObj.Value = arr[i];
                itemObj.Probable = new List<int>() { arr[i] };
            }
        }
    }
}