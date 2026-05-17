namespace LexicalAnalyzer
{
    public class ErrorInfo
    {
        public string Fragment { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public bool IsLexical { get; set; }

        public ErrorInfo(string fragment, string location, string description, int line, int position, bool isLexical)
        {
            Fragment = fragment;
            Location = location;
            Description = description;
            Line = line;
            Position = position;
            IsLexical = isLexical;
        }
        // Конструктор с 5 параметрами (для обратной совместимости)
        public ErrorInfo(string fragment, string location, string description, int line, int position)
        {
            Fragment = fragment;
            Location = location;
            Description = description;
            Line = line;
            Position = position;
            IsLexical = false; // по умолчанию синтаксическая ошибка
        }
    }
}