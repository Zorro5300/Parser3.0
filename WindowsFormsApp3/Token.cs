namespace LexicalAnalyzer
{
    public class Token
    {
        public int Code { get; set; }          // Условный код типа лексемы
        public string Type { get; set; }       // Тип лексемы 
        public string Lexeme { get; set; }     // Лексема
        public int Line { get; set; }          // Номер строки
        public int StartPos { get; set; }      // Начальная позиция в строке
        public int EndPos { get; set; }        // Конечная позиция в строке
        public bool IsError { get; set; }      // Флаг ошибки

        public override string ToString()
        {
            return $"{Lexeme} [{Type}] at {Line}:{StartPos}-{EndPos}";
        }
    }
}