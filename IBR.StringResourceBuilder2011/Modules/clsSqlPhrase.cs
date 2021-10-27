namespace IBR.StringResourceBuilder2011.Modules
{
    public class SqlPhrase
    {
        public int Start;
        public int Line;
        public string Text;

        public SqlPhrase(int start, string text, int line)
        {
            Start = start;
            Text = text;
            Line = line;
        }
    }
}
