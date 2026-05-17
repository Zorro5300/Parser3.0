using System;
using System.Collections.Generic;
using System.Text;

namespace LexicalAnalyzer
{
    public class Scanner
    {
        // Таблица ключевых слов 
        private readonly HashSet<string> keywords = new HashSet<string>
        {
            "type", "record", "end", "real"
        };

        // Таблица операторов и разделителей
        private readonly Dictionary<char, int> singleCharTokens = new Dictionary<char, int>
        {
            { '=', 20 }, { ',', 21 }, { ';', 22 }, { ':', 23 }, { '.', 24 }
        };

        private readonly Dictionary<string, int> doubleCharTokens = new Dictionary<string, int>
        {
            { ":=", 25 }
        };

        private List<Token> tokens;
        private string[] lines;
        private int currentLine;
        private int currentPos;
        private StringBuilder currentLexeme;

        public List<Token> Analyze(string text)
        {
            tokens = new List<Token>();
            lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            currentLexeme = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                currentLine = i + 1; // номера строк с 1
                currentPos = 0;
                string line = lines[i];

                while (currentPos < line.Length)
                {
                    char ch = line[currentPos];

                    if (char.IsWhiteSpace(ch))
                    {
                        // Пропускаем пробелы и табуляции
                        if (ch == '\t') currentPos++;
                        else if (ch == ' ') currentPos++;
                        else currentPos++; // другие пробельные символы
                        continue;
                    }

                    // Идентификатор или ключевое слово
                    if (char.IsLetter(ch))
                    {
                        ReadIdentifierOrKeyword(line);
                    }
                    // Число
                    else if (char.IsDigit(ch))
                    {
                        ReadNumber(line);
                    }
                    // Операторы и разделители
                    else if (singleCharTokens.ContainsKey(ch) || doubleCharTokens.ContainsKey(ch.ToString()))
                    {
                        ReadOperator(line);
                    }
                    else
                    {
                        // Ошибка: недопустимый символ
                        AddErrorToken(ch.ToString(), currentLine, currentPos, currentPos);
                        currentPos++;
                    }
                }
            }

            return tokens;
        }

        private void ReadIdentifierOrKeyword(string line)
        {
            currentLexeme.Clear();
            int startPos = currentPos;

            while (currentPos < line.Length && (char.IsLetterOrDigit(line[currentPos]) || line[currentPos] == '_'))
            {
                currentLexeme.Append(line[currentPos]);
                currentPos++;
            }

            string lexeme = currentLexeme.ToString();
            int endPos = currentPos - 1;

            if (keywords.Contains(lexeme))
            {
                // Ключевое слово
                tokens.Add(new Token
                {
                    Code = 2,
                    Type = "Ключевое слово",
                    Lexeme = lexeme,
                    Line = currentLine,
                    StartPos = startPos,
                    EndPos = endPos,
                    IsError = false
                });
            }
            else
            {
                // Идентификатор
                tokens.Add(new Token
                {
                    Code = 1,
                    Type = "Идентификатор",
                    Lexeme = lexeme,
                    Line = currentLine,
                    StartPos = startPos,
                    EndPos = endPos,
                    IsError = false
                });
            }
        }

        private void ReadNumber(string line)
        {
            currentLexeme.Clear();
            int startPos = currentPos;
            bool hasDot = false;

            while (currentPos < line.Length && (char.IsDigit(line[currentPos]) || line[currentPos] == '.'))
            {
                if (line[currentPos] == '.')
                {
                    if (hasDot)
                    {
                        // Вторая точка - ошибка
                        currentLexeme.Append(line[currentPos]);
                        currentPos++;
                        AddErrorToken(currentLexeme.ToString(), currentLine, startPos, currentPos - 1);
                        return;
                    }
                    hasDot = true;
                }
                currentLexeme.Append(line[currentPos]);
                currentPos++;
            }

            string lexeme = currentLexeme.ToString();
            int endPos = currentPos - 1;

            if (hasDot)
            {
                // Вещественное число
                // Проверка, что после точки есть цифры
                int dotIndex = lexeme.IndexOf('.');
                if (dotIndex == lexeme.Length - 1)
                {
                    AddErrorToken(lexeme, currentLine, startPos, endPos);
                    return;
                }
                tokens.Add(new Token
                {
                    Code = 4,
                    Type = "Вещественное число",
                    Lexeme = lexeme,
                    Line = currentLine,
                    StartPos = startPos,
                    EndPos = endPos,
                    IsError = false
                });
            }
            else
            {
                // Целое число
                tokens.Add(new Token
                {
                    Code = 3,
                    Type = "Целое число",
                    Lexeme = lexeme,
                    Line = currentLine,
                    StartPos = startPos,
                    EndPos = endPos,
                    IsError = false
                });
            }
        }

        private void ReadOperator(string line)
        {
            int startPos = currentPos;

            // Проверка двухсимвольного оператора
            if (currentPos + 1 < line.Length)
            {
                string doubleChar = line.Substring(currentPos, 2);
                if (doubleCharTokens.ContainsKey(doubleChar))
                {
                    tokens.Add(new Token
                    {
                        Code = doubleCharTokens[doubleChar],
                        Type = "Оператор",
                        Lexeme = doubleChar,
                        Line = currentLine,
                        StartPos = startPos,
                        EndPos = startPos + 1,
                        IsError = false
                    });
                    currentPos += 2;
                    return;
                }
            }

            // Односимвольный оператор или разделитель
            char ch = line[currentPos];
            if (singleCharTokens.ContainsKey(ch))
            {
                string type = (ch == ',' || ch == ';') ? "Разделитель" : "Оператор";
                tokens.Add(new Token
                {
                    Code = singleCharTokens[ch],
                    Type = type,
                    Lexeme = ch.ToString(),
                    Line = currentLine,
                    StartPos = startPos,
                    EndPos = startPos,
                    IsError = false
                });
                currentPos++;
            }
            else
            {
                AddErrorToken(ch.ToString(), currentLine, startPos, startPos);
                currentPos++;
            }
        }

        private void AddErrorToken(string lexeme, int line, int start, int end)
        {
            tokens.Add(new Token
            {
                Code = 99,
                Type = "ОШИБКА",
                Lexeme = lexeme,
                Line = line,
                StartPos = start,
                EndPos = end,
                IsError = true
            });
        }
    }
}