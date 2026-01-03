// ListViewItemComparer.cs
using System;
using System.Collections;
using System.Windows.Forms;

namespace DreamDiary
{
    public class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder order;

        public ListViewItemComparer()
        {
            col = 0;
            order = SortOrder.Ascending;
        }

        public ListViewItemComparer(int column, SortOrder sortOrder)
        {
            col = column;
            order = sortOrder;
        }

        public int Compare(object x, object y)
        {
            int returnVal;
            // Попробуем сравнить как даты
            if (col == 2)
            {
                // Столбец "Дата"
                System.DateTime dt1 = System.DateTime.Parse(((ListViewItem)x).SubItems[col].Text);
                System.DateTime dt2 = System.DateTime.Parse(((ListViewItem)y).SubItems[col].Text);
                returnVal = dt1.CompareTo(dt2);
            }
            else
            {
                // Для остальных столбцов сравниваем строки
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            }

            if (order == SortOrder.Descending)
                returnVal *= -1;

            return returnVal;
        }
    }
}
