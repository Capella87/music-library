using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicLibrary;

namespace MusicLibrary.Utilities
{ 
    // Source : https://genert.org/blog/csharp-programming/
    public class Table
    {
        private List<object> _columns;
        private List<object?[]> _rows;
        // private List<Type> _columnTypes;

        public Table(params string[] columns)
        {
            if (columns == null || columns.Length == 0)
                throw new ArgumentException("Parameter cannot be null nor empty.");

            _columns = new List<object>(columns);
            _rows = new List<object?[]>();
        }

        public void Add(params object?[] values)
        {
            if (values == null)
                throw new ArgumentException("Parameter cannot be null");
            if (values.Length != _columns.Count)
                throw new Exception("The number of values in the row was not matched to column count.");

            _rows.Add(values);
        }

        private List<int> GetColumnsMaxStrLengths()
        {
            var columnsLen = new List<int>();
            
            for (int i = 0; i < _columns.Count; i++)
            {
                List<object?> columnRow = new List<object?>();
                int max = 0;

                columnRow.Add(_columns[i]);
                for (int j = 0; j < _rows.Count; j++)
                    columnRow.Add(_rows[j][i]);

                foreach (var e in columnRow)
                {
                    int length = (e == null) ? 4 : e.ToString().Length;
                    if (max < length) max = length;
                }

                if (max >= 25) max = 25;
                columnsLen.Add(max);
            }
            
            return columnsLen;
        }
        
        public override string ToString()
        {
            var tableString = new StringBuilder();
            var colsLength = GetColumnsMaxStrLengths();
            var consoleWindowWidth = Console.WindowWidth;
            
            var rowStringFormat = Enumerable
                .Range(0, _columns.Count)
                .Select(i => " | {" + i + ",-" + colsLength[i] + "}")
                .Aggregate((total, nextValue) => total + nextValue) + " |";

            string columnHeaders = String.Format(rowStringFormat, _columns.ToArray());
            List<string> results = _rows.Select(row => string.Format(rowStringFormat, row)).ToList();

            int maximumRowLength = Math.Max(0,
                _rows.Any() ? _rows.Max(row => string.Format(rowStringFormat, row).Length) : 0);
            int maximumLineLength = Math.Min(maximumRowLength, columnHeaders.Length);

            string dividerLine = string.Join("", Enumerable.Repeat("-", maximumLineLength - 1));
            string divider = $" {dividerLine} ";

            tableString.AppendLine(divider);
            tableString.AppendLine(columnHeaders);

            foreach (var row in results)
            {
                tableString.AppendLine(divider);
                tableString.AppendLine(row);
            }

            tableString.AppendLine(divider);

            return tableString.ToString();
        }

        public void Print()
        {
            Console.WriteLine(ToString());
        }
    }
}
