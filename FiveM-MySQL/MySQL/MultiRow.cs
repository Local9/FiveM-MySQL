using System;
using System.Collections.Generic;
using System.Text;

namespace GHMatti.MySQL
{
    public class MultiRow
    {

        public string CommandText => mysqlCommandText.ToString();
        public IDictionary<string, dynamic> Parameters => mysqlParameters;

        private StringBuilder mysqlCommandText;
        private Dictionary<string, dynamic> mysqlParameters;

        private List<string> mysqlColumns;

        public MultiRow()
        {
            mysqlCommandText = new StringBuilder();
            mysqlParameters = new Dictionary<string, dynamic>();
            mysqlColumns = new List<string>();
        }

        public static MultiRow TryParse(string tablename, dynamic parameters)
        {
            return (new MultiRow()).Parse(tablename, parameters);
        }

        private MultiRow Parse(string tablename, dynamic parameters)
        {
            try
            {
                IList<dynamic> parametersToParse = (IList<dynamic>)parameters;
                if (parametersToParse.Count == 0)
                    throw new Exception("[GHMattiMySQL] No Rows to Insert");
                BuildTableSection(tablename, parametersToParse[0]);
                BuildValuesSection(parametersToParse);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return this;
        }

        private void BuildValuesSection(IList<dynamic> parametersToParse)
        {
            uint currentRow = 0, currentColumn = 0;
            foreach (IDictionary<string, dynamic> row in parametersToParse)
            {
                foreach (KeyValuePair<string, dynamic> column in row)
                {
                    string parameterName = BuildParameterName(column.Key, currentRow);
                    mysqlParameters.Add(parameterName, column.Value);
                    if (currentColumn == 0)
                    {
                        if (currentRow > 0)
                            mysqlCommandText.Append(", ");
                        AppendFirstColumn(parameterName);
                    }
                    else
                    {
                        AppendNotFirstColumn(parameterName);
                    }
                    if (row.Count != mysqlColumns.Count || !mysqlColumns.Contains(column.Key))
                        throw new Exception("[GHMattiMySQL] Detected a partial Insert");
                    currentColumn++;
                }
                mysqlCommandText.Append(")");
                currentColumn = 0;
                currentRow++;
            }
            mysqlCommandText.Append(";");
        }

        private string BuildParameterName(string key, uint currentRow)
        {
            StringBuilder stringBuilder = new StringBuilder("@");
            stringBuilder.Append(key);
            stringBuilder.Append(currentRow);
            return stringBuilder.ToString();
        }

        private void BuildTableSection(string tablename, dynamic row)
        {
            mysqlCommandText.Append("INSERT INTO ");
            mysqlCommandText.Append(tablename);
            IDictionary<string, dynamic> firstRow = row;
            uint currentColumn = 0;
            foreach (KeyValuePair<string, dynamic> column in (IDictionary<string, dynamic>)firstRow)
            {
                mysqlColumns.Add(column.Key);
                if (currentColumn == 0)
                    AppendFirstColumn(column.Key);
                else
                    AppendNotFirstColumn(column.Key);
                currentColumn++;
            }
            mysqlCommandText.Append(") VALUES ");
        }

        private void AppendFirstColumn(string name)
        {
            mysqlCommandText.Append(" (");
            mysqlCommandText.Append(name);
        }

        private void AppendNotFirstColumn(string name)
        {
            mysqlCommandText.Append(", ");
            mysqlCommandText.Append(name);
        }
    }
}
