using System;
using System.Collections.Generic;
using System.Text;

namespace GHMatti.MySQL
{
    /// <summary>
    /// MultiRow Parsing Class
    /// </summary>
    public class MultiRow
    {
        /// <summary>
        /// Return attributes for Command Text and Parameters
        /// </summary>
        public string CommandText => mysqlCommandText.ToString();
        public IDictionary<string, dynamic> Parameters => mysqlParameters;

        /// <summary>
        /// Actual content, making sure that only this class modifys it
        /// </summary>
        private StringBuilder mysqlCommandText;
        private Dictionary<string, dynamic> mysqlParameters;

        /// <summary>
        /// Helper list to make sure the user does not provide bad arguments
        /// </summary>
        private List<string> mysqlColumns;

        /// <summary>
        /// Constructor nothing special
        /// </summary>
        public MultiRow()
        {
            mysqlCommandText = new StringBuilder();
            mysqlParameters = new Dictionary<string, dynamic>();
            mysqlColumns = new List<string>();
        }

        /// <summary>
        /// Actual Function to call to Parse the MultiRow data
        /// </summary>
        /// <param name="tablename">name of the table that is inserted into</param>
        /// <param name="parameters">List of dictionarys presenting the rows to be inserted</param>
        /// <returns></returns>
        public static MultiRow TryParse(string tablename, dynamic parameters)
        {
            return (new MultiRow()).Parse(tablename, parameters);
        }

        /// <summary>
        /// Parsing Work, just throw in case stuff goes wrong.
        /// </summary>
        /// <param name="tablename">name of the table</param>
        /// <param name="parameters">List of dictionarys presenting the rows to be inserted</param>
        /// <returns>this multirow object</returns>
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

        /// <summary>
        /// Create the values section of the INSERT statement
        /// </summary>
        /// <param name="parametersToParse">List of dictionaries that represent the rows to be inserted</param>
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

        /// <summary>
        /// Creates a parameter name to use
        /// </summary>
        /// <param name="key">column name</param>
        /// <param name="currentRow">number of the row</param>
        /// <returns>Parameter name</returns>
        private string BuildParameterName(string key, uint currentRow)
        {
            StringBuilder stringBuilder = new StringBuilder("@");
            stringBuilder.Append(key);
            stringBuilder.Append(currentRow);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Build the table section of the INSERT statement, also populating the columns List to double check
        /// </summary>
        /// <param name="tablename">name of the table that is inserted into</param>
        /// <param name="row">first row in dictionary form</param>
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

        /// <summary>
        /// Function to add the first column in a row to the command text
        /// </summary>
        /// <param name="name">column name</param>
        private void AppendFirstColumn(string name)
        {
            mysqlCommandText.Append(" (");
            mysqlCommandText.Append(name);
        }

        /// <summary>
        /// Function to add any other column of a row to the command text that is not the first
        /// </summary>
        /// <param name="name">column name</param>
        private void AppendNotFirstColumn(string name)
        {
            mysqlCommandText.Append(", ");
            mysqlCommandText.Append(name);
        }
    }
}
