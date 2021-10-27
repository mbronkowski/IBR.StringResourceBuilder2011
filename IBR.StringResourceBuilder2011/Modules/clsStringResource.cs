using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using IBR.StringResourceBuilder2011.Annotations;


namespace IBR.StringResourceBuilder2011.Modules
{
    /// <summary>
    /// StringResource class which holds a resource name, string literal and its location.
    /// </summary>
    class StringResource : INotifyPropertyChanged
    {
        string[] standardStrings = new[] {
          "name", "description", "comment", "key","value"
      };

        private string[] standardLinesString = new[]
        {
          "SqlCommand(", "AddParameter(", ".Include(" , "AddComponent("
      };
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StringResource"/> class.
        /// </summary>
        public StringResource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringResource"/> class.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="text">The string literal.</param>
        /// <param name="location">The location.</param>
        /// <param name="lineText"></param>
        /// <param name="isAttribut"></param>
        public StringResource(string name,
            string text,
            Point location, string lineText, bool isAttribut, string sqlCodedValue, string rawTextValue)
        {
            this.Name = name;
            this.Text = text;
            this.Location = location;
            this.IsAttribut = isAttribut;
            this.LineText = lineText.TrimStart().TrimEnd();
            this.SqlCodedValue = sqlCodedValue;
            this.RawText = rawTextValue;
            checkStardards(lineText);
            try
            {
                var strings = new List<string>(); 
                if(_forSql)
                    strings = ReplaceSqlParams(lineText).Split(new[] { text }, StringSplitOptions.None).ToList();
                else
                    strings = lineText.Split(new[] { text }, StringSplitOptions.None).ToList();

                this.TextBefore = strings[0];
                this.TextAfter = strings[1];
            }
            catch (Exception)
            {

            }

        }

        string ReplaceSqlParams(string val) 
        {
            var modifiers = new List<char>();
            modifiers.AddRange("diosuxX");

            foreach (var mod in modifiers)
            {
                val = val.Replace("%" + mod, "{}");
            }
            var count = val.Split(new string[] { "{}" }, StringSplitOptions.None).Length - 1;
            for (int i = 0; i < count; i++)
            {
                var regex = new Regex(Regex.Escape("{}"));
                val = regex.Replace(val, ("{" + i + "}"), 1);
            }

            return val;
        }

        bool _forSql => SqlCodedValue != null;

        public string TextAfter { get; set; }

        public string TextBefore { get; set; }

        public string SqlCodedValue { get; set; }

        private void checkStardards(string lineText)
        {
            if (!_forSql)
            {
                this.IsAttribut = IsAttribut || lineText.StartsWith("[") && lineText.EndsWith("]");

                this.SkipAsAt = standardStrings.Contains(Text.ToLower()) ||
                                lineText.Contains("Name = \"" + Text) ||
                                lineText.Contains("LookUpColumnInfo(\"" + Text) ||
                                lineText.Contains("[\"" + Text + "\"]") ||
                                lineText.Contains("Guid(\"" + Text) ||
                                lineText.Contains("case \"" + Text) ||
                                lineText.Contains("IsInRole(\"" + Text) ||
                                lineText.ToLower().Contains(("\"id\", \"" + Text).ToLower()) ||
                                standardLinesString.Any(x => lineText.Contains(x));
            }
            else
            {
                this.IsAttribut = false;
                this.SkipAsAt = !lineText.Contains("raiserror");
            }
        }



        #endregion //Constructor -----------------------------------------------------------------------

        #region Types
        #endregion //Types -----------------------------------------------------------------------------

        #region Fields
        #endregion //Fields ----------------------------------------------------------------------------

        #region Properties

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        /// <value>
        /// The resource name.
        /// </value>
        ///
        public string Name { get; set; }

        /// <summary>
        /// Get or sets if resource is for SQL script
        /// </summary>
        public bool ForSql => _forSql;

        /// <summary>
        /// Gets the string literal (text).
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets the string raw text of value
        /// </summary>
        public string RawText { get; private set; }

        //must be defined explicitly because otherwise Offset() would not work!
        private System.Drawing.Point m_Location = System.Drawing.Point.Empty;
        private bool m_SkipAsAt;
        private bool _aiSugestionAsAt;
        private bool _isAttribut;
        private int _precision;

        /// <summary>
        /// Gets the location of the string literal (X is line number and Y is column number).
        /// </summary>
        public System.Drawing.Point Location { get { return (m_Location); } private set { m_Location = value; } }
        public string LineText { get; private set; }

        public bool IsAttribut
        {
            get => _isAttribut;
            set
            {
                if (value == _isAttribut) return;
                _isAttribut = value;
                OnPropertyChanged();
            }
        }

        public bool SkipAsAt
        {
            get => m_SkipAsAt;
            set { m_SkipAsAt = value; OnPropertyChanged(); }
        }

        public bool AISugestionAsAt
        {
            get => _aiSugestionAsAt;
            set
            {
                if (value == _aiSugestionAsAt) return;
                _aiSugestionAsAt = value;
                OnPropertyChanged();
            }
        }

        public int Precision
        {
            get => _precision;
            set
            {
                if (value == _precision) return;
                _precision = value;
                OnPropertyChanged();
            }
        }

        #endregion //Properties ------------------------------------------------------------------------

        #region Events
        #endregion //Events ----------------------------------------------------------------------------

        #region Private methods
        #endregion //Private methods -------------------------------------------------------------------

        #region Public methods

        /// <summary>
        /// Translates the location by the specified amount.
        /// </summary>
        /// <param name="lineOffset">The line offset.</param>
        /// <param name="columnOffset">The column offset.</param>
        public void Offset(int lineOffset,
                           int columnOffset)
        {
            if ((lineOffset == 0) && (columnOffset == 0))
                return;

            m_Location.Offset(lineOffset, columnOffset);
        }

        public string GetCSVLine()
        {
            var stringResult = SkipAsAt ? "2" : (IsAttribut ? "1" : "0");
            return
                $"{StringToCSVCell(Text)},{StringToCSVCell(LineText)},{StringToCSVCell(TextBefore)},{StringToCSVCell(TextAfter)},{stringResult}";
        }
        #endregion //Public methods --------------------------------------------------------------------
        public static string StringToCSVCell(string str)
        {
            try
            {
                str = str ?? "";
                bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
                if (mustQuote)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("\"");
                    foreach (char nextChar in str)
                    {
                        sb.Append(nextChar);
                        if (nextChar == '"')
                            sb.Append("\"");
                    }
                    sb.Append("\"");
                    return sb.ToString();
                }
            }
            catch (Exception)
            {
                return "";
            }


            return str;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    } //class
} //namespace
