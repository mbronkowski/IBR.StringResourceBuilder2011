using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using IBR.StringResourceBuilder2011.Annotations;


namespace IBR.StringResourceBuilder2011.Modules
{
  /// <summary>
  /// StringResource class which holds a resource name, string literal and its location.
  /// </summary>
  class StringResource : INotifyPropertyChanged
  {
      string[] standardStrings = new [] {
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
        Point location, string lineText, bool isAttribut)
    {
      this.Name     = name;
      this.Text     = text;
      this.Location = location;
      this.IsAttribut = isAttribut;
      this.LineText = lineText.TrimStart().TrimEnd();
      checkStardards(lineText);
      try
      {
          var strings = lineText.Split(new []{text},StringSplitOptions.None);
          this.TextBefore = strings[0];
          this.TextAfter = strings[1];
      }
      catch (Exception)
      {
          
      }
      
    }

    public string TextAfter { get; set; }

    public string TextBefore { get; set; }

    private void checkStardards(string lineText)
    {
        this.IsAttribut = IsAttribut || lineText.StartsWith("[") && lineText.EndsWith("]");
     
        this.SkipAsAt = standardStrings.Contains(Text.ToLower()) ||
                        lineText.Contains("Name = \"" + Text) ||
                        lineText.Contains("LookUpColumnInfo(\"" + Text) ||
                        lineText.Contains("[\"" + Text+"\"]") ||
                        lineText.Contains("Guid(\"" + Text) ||
                        lineText.Contains("case \"" + Text) ||
                        lineText.Contains("IsInRole(\"" + Text) ||
                        lineText.ToLower().Contains(("\"id\", \"" + Text).ToLower()) ||
                        standardLinesString.Any(x=> lineText.Contains(x));    
      

       

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
    /// Gets the string literal (text).
    /// </summary>
    public string Text { get; private set; }

    //must be defined explicitly because otherwise Offset() would not work!
    private System.Drawing.Point m_Location = System.Drawing.Point.Empty;
    private bool m_SkipAsAt;

    /// <summary>
    /// Gets the location of the string literal (X is line number and Y is column number).
    /// </summary>
    public System.Drawing.Point Location { get { return (m_Location); } private set { m_Location = value; } }
    public string LineText { get; private set; }
    public bool IsAttribut { get;  set; }

    public bool SkipAsAt
    {
        get => m_SkipAsAt;
        set { m_SkipAsAt = value;  OnPropertyChanged();}
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
