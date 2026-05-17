using System;
using System.Collections.Generic;
using System.Text;

namespace LexicalAnalyzer
{
    public class LexicalAnalyzer
    {
        private List<ErrorInfo> errors;
        private string[] lines;
        private int currentLine;
        private int currentPos;

        private struct WordToken
        {
            public string Text;
            public int StartPos; // 0-based в строке
        }

        private struct Line1Layout
        {
            public bool HasType;
            public int TypeStart, TypeEnd;

            public bool HasComplex;
            public int ComplexStart, ComplexEnd;

            public bool HasRecord;
            public int RecordStart, RecordEnd;

            public int EqIndex;
        }

        public List<ErrorInfo> Analyze(string text)
        {
            errors = new List<ErrorInfo>();
            lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // Этап 1: лексический анализ — недопустимые символы и некорректные числа
            ScanLexicalErrors();

            // Этап 2: синтаксис — недопустимые ; : внутри ключевых слов (в любой позиции)
            ValidateKeywordSeparatorErrors();

            // Этап 3: лексика — искажённые / неполные ключевые слова
            ValidateKeywordSpelling();

            // Этап 4: отсутствие '=' после complex
            ValidateEqualsAfterComplex();

            // Этап 5: структурный синтаксис
            ValidateStructure();

            // Этап 6: цифры внутри ключевых слов (type, complex, record, real, end)
            ValidateDigitsInKeywords();

            // Этап 7: недопустимые числа в объявлении полей
            ValidateInvalidNumbers();

            return errors;
        }

        private void ScanLexicalErrors()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                currentLine = i + 1;
                currentPos = 0;
                string line = lines[i];

                while (currentPos < line.Length)
                {
                    char ch = line[currentPos];

                    if (char.IsWhiteSpace(ch))
                    {
                        currentPos++;
                        continue;
                    }

                    if (char.IsLetter(ch))
                    {
                        ReadIdentifier(line);
                    }
                    else if (char.IsDigit(ch))
                    {
                        ReadNumber(line);
                    }
                    else if (IsValidOperator(ch))
                    {
                        currentPos++;
                    }
                    else
                    {
                        AddError(ch.ToString(), currentLine, currentPos + 1,
                            $"Недопустимый символ '{ch}' (код ASCII: {(int)ch})", true);
                        currentPos++;
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет ключевые слова по ЗОНАМ в программе, а не по фиксированным шаблонам compl;ex / rec:ord.
        /// </summary>
        private void ValidateKeywordSeparatorErrors()
        {
            if (lines.Length == 0)
                return;

            Line1Layout line1 = ParseLine1Layout(lines[0]);

            if (line1.HasType)
                ScanKeywordRegion(lines[0], 1, line1.TypeStart, line1.TypeEnd, "type", ';', ':');

            if (line1.HasComplex)
                ScanKeywordRegion(lines[0], 1, line1.ComplexStart, line1.ComplexEnd, "complex", ';', ':');

            if (line1.HasRecord)
                ScanKeywordRegion(lines[0], 1, line1.RecordStart, line1.RecordEnd, "record", ';', ':');

            //  Строки полей: real 
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNum = i + 1;

                if (line.Trim().ToLower().StartsWith("end"))
                    continue;

                int colonIdx = line.IndexOf(':');
                if (colonIdx >= 0)
                {
                    int fieldTypeStart = SkipSpaces(line, colonIdx + 1);
                    int fieldTypeEnd = EndOfFieldTypeToken(line, fieldTypeStart);

                    if (fieldTypeStart < fieldTypeEnd)
                    {
                        string fieldTypeKeyword = GuessTypeKeyword(line.Substring(fieldTypeStart, fieldTypeEnd - fieldTypeStart).Trim());
                        ScanKeywordRegion(line, lineNum, fieldTypeStart, fieldTypeEnd, fieldTypeKeyword, ';', ':');
                    }
                }
            }

            //  Строка end (первый токен строки) 
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string trimmed = line.TrimStart();
                if (trimmed.Length == 0 || char.ToLower(trimmed[0]) != 'e')
                    continue;

                int lineNum = i + 1;
                if (!trimmed.ToLower().StartsWith("end"))
                    continue;

                int leading = line.Length - trimmed.Length;
                int endTokenLen = EndOfEndKeywordToken(trimmed);
                ScanKeywordRegion(line, lineNum, leading, leading + endTokenLen, "end", ';', ':');
            }
        }

        /// <summary>
        /// Ищет ; : и пробелы внутри зоны, где должно быть одно ключевое слово.
        /// </summary>
        private void ScanKeywordRegion(string line, int lineNum, int start, int end, string keyword, params char[] forbidden)
        {
            if (start < 0 || end <= start || start >= line.Length)
                return;

            if (end > line.Length)
                end = line.Length;

            string region = line.Substring(start, end - start).Trim();
            if (region.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                return;

            // Пробел внутри ключевого слова (co mplex, r ecord, en d)
            for (int i = start; i < end; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                    continue;

                if (!HasLetterBefore(line, start, i) || !HasLetterAfter(line, i + 1, end))
                    continue;

                if (ErrorAlreadyExists(lineNum, i + 1, "пробел"))
                    return;

                AddError("пробел", lineNum, i + 1,
                    $"Синтаксическая ошибка: недопустимый пробел в ключевом слове '{keyword}'", false);
                return;
            }

            foreach (char ch in forbidden)
            {
                for (int i = start; i < end; i++)
                {
                    if (line[i] != ch)
                        continue;

                    if (ErrorAlreadyExists(lineNum, i + 1, ch.ToString()))
                        return;

                    AddError(ch.ToString(), lineNum, i + 1,
                        $"Синтаксическая ошибка: недопустимый символ '{ch}' в ключевом слове '{keyword}'", false);
                    return;
                }
            }
        }

        /// <summary>Разбор 1-й строки по словам: ty pe | complex | co mplex | r ecord.</summary>
        private static Line1Layout ParseLine1Layout(string line1)
        {
            var layout = new Line1Layout { EqIndex = line1.IndexOf('=') };
            int headerEnd = layout.EqIndex >= 0 ? layout.EqIndex : line1.Length;
            var words = ExtractLetterWords(line1.Substring(0, headerEnd));
            int idx = 0;

            if (words.Count >= 2 && (words[0].Text + words[1].Text).Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                layout.HasType = true;
                layout.TypeStart = words[0].StartPos;
                layout.TypeEnd = words[1].StartPos + words[1].Text.Length;
                idx = 2;
            }
            else if (words.Count >= 1)
            {
                layout.HasType = true;
                layout.TypeStart = words[0].StartPos;
                layout.TypeEnd = words[0].StartPos + words[0].Text.Length;
                idx = 1;
            }

            if (idx < words.Count)
            {
                layout.HasComplex = true;
                layout.ComplexStart = words[idx].StartPos;

                if (idx + 1 < words.Count &&
                    (words[idx].Text + words[idx + 1].Text).Equals("complex", StringComparison.OrdinalIgnoreCase))
                {
                    layout.ComplexEnd = words[idx + 1].StartPos + words[idx + 1].Text.Length;
                }
                else
                {
                    layout.ComplexEnd = words[idx].StartPos + words[idx].Text.Length;
                }
            }

            if (layout.EqIndex >= 0)
            {
                int recordOffset = layout.EqIndex + 1;
                var recordWords = ExtractLetterWords(line1.Substring(recordOffset));

                if (recordWords.Count >= 2 &&
                    (recordWords[0].Text + recordWords[1].Text).Equals("record", StringComparison.OrdinalIgnoreCase))
                {
                    layout.HasRecord = true;
                    layout.RecordStart = recordOffset + recordWords[0].StartPos;
                    layout.RecordEnd = recordOffset + recordWords[1].StartPos + recordWords[1].Text.Length;
                }
                else if (recordWords.Count >= 1)
                {
                    layout.HasRecord = true;
                    layout.RecordStart = recordOffset + recordWords[0].StartPos;
                    layout.RecordEnd = recordOffset + recordWords[0].StartPos + recordWords[0].Text.Length;
                }
            }

            return layout;
        }

        private static List<WordToken> ExtractLetterWords(string text)
        {
            var list = new List<WordToken>();
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsLetter(text[i]))
                    continue;

                int start = i;
                while (i < text.Length && char.IsLetter(text[i]))
                    i++;

                list.Add(new WordToken { Text = text.Substring(start, i - start), StartPos = start });
            }

            return list;
        }

        private static bool HasLetterBefore(string line, int start, int pos)
        {
            for (int i = pos - 1; i >= start; i--)
            {
                if (char.IsLetter(line[i]))
                    return true;
            }
            return false;
        }

        private static bool HasLetterAfter(string line, int pos, int end)
        {
            for (int i = pos; i < end; i++)
            {
                if (char.IsLetter(line[i]))
                    return true;
            }
            return false;
        }

        private void ValidateKeywordSpelling()
        {
            if (lines.Length == 0)
                return;

            Line1Layout line1 = ParseLine1Layout(lines[0]);

            if (line1.HasType)
                CheckKeywordSpelling(lines[0], 1, line1.TypeStart, line1.TypeEnd, "type");

            if (line1.HasComplex)
                CheckKeywordSpelling(lines[0], 1, line1.ComplexStart, line1.ComplexEnd, "complex");

            if (line1.HasRecord)
                CheckKeywordSpelling(lines[0], 1, line1.RecordStart, line1.RecordEnd, "record");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNum = i + 1;
                string trimmed = line.TrimStart();

                if (trimmed.Length == 0)
                    continue;

                if (trimmed.ToLower().StartsWith("end"))
                {
                    int leading = line.Length - trimmed.Length;
                    int endLettersLen = EndOfKeywordLetters(trimmed, 0);
                    CheckKeywordSpelling(line, lineNum, leading, leading + endLettersLen, "end");
                    continue;
                }

                ValidateFieldTypeKeywordSpelling(line, lineNum);
            }
        }

        private void ValidateFieldTypeKeywordSpelling(string line, int lineNum)
        {
            int colonIdx = line.IndexOf(':');
            if (colonIdx < 0)
            {
                var words = ExtractIdentifiers(line);
                if (words.Count == 0)
                    return;

                WordToken last = words[words.Count - 1];
                string fieldTypeKeyword = GuessTypeKeyword(last.Text);
                CheckKeywordSpelling(line, lineNum, last.StartPos, last.StartPos + last.Text.Length, fieldTypeKeyword);
                return;
            }

            int fieldTypeStart = SkipSpaces(line, colonIdx + 1);
            int fieldTypeEnd = EndOfFieldTypeToken(line, fieldTypeStart);

            if (fieldTypeStart >= fieldTypeEnd)
                return;

            string token = line.Substring(fieldTypeStart, fieldTypeEnd - fieldTypeStart).Trim();
            string fieldTypeKeywordName = GuessTypeKeyword(token);
            CheckKeywordSpelling(line, lineNum, fieldTypeStart, fieldTypeEnd, fieldTypeKeywordName);
        }

        private static string GuessTypeKeyword(string token)
        {
            if (token.Length > 0 && char.ToLower(token[0]) == 'i')
                return "integer";
            return "real";
        }

        private void CheckKeywordSpelling(string line, int lineNum, int start, int end, string expected)
        {
            if (start < 0 || end <= start || start >= line.Length)
                return;

            if (end > line.Length)
                end = line.Length;

            string raw = line.Substring(start, end - start);
            string region = raw.Trim();
            if (region.Length == 0)
                return;

            if (region.Equals(expected, StringComparison.OrdinalIgnoreCase))
                return;

            // Цифры в ключевом слове — отдельная лексическая проверка
            if (ContainsDigit(region))
                return;

            // Пробел, ; и : — отдельная синтаксическая проверка
            if (HasInternalWhitespace(line, start, end) ||
                region.IndexOf(';') >= 0 || region.IndexOf(':') >= 0)
                return;

            if (!IsDistortedOrIncompleteKeyword(region, expected))
                return;

            int pos = start;
            while (pos < end && char.IsWhiteSpace(line[pos]))
                pos++;

            string fragment = ExtractLettersOnly(region);
            if (fragment.Length == 0)
                fragment = region;

            if (ErrorAlreadyExists(lineNum, pos + 1, fragment))
                return;

            AddError(fragment, lineNum, pos + 1,
                $"Лексическая ошибка: искажённое или неполное ключевое слово. Ожидается '{expected}', найдено '{region}'", true);
        }

        private static string ExtractLettersOnly(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (char.IsLetter(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private static bool IsDistortedOrIncompleteKeyword(string found, string expected)
        {
            if (string.IsNullOrEmpty(found))
                return true;

            if (found.Equals(expected, StringComparison.OrdinalIgnoreCase))
                return false;

            if (ContainsDigit(found))
                return true;

            string letters = ExtractLettersOnly(found);
            if (letters.Length == 0)
                return false;

            // Неполное слово: typ, compl, rec, rea, en
            if (expected.StartsWith(letters, StringComparison.OrdinalIgnoreCase) && letters.Length < expected.Length)
                return true;
            if (letters.StartsWith(expected, StringComparison.OrdinalIgnoreCase) && letters.Length > expected.Length)
                return true;

            if (char.ToLower(letters[0]) != char.ToLower(expected[0]))
                return false;

            int distance = LevenshteinDistance(letters.ToLower(), expected.ToLower());
            int maxDistance = expected.Length <= 4 ? 2 : 3;
            return distance > 0 && distance <= maxDistance;
        }

        private static int LevenshteinDistance(string a, string b)
        {
            int[,] d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[a.Length, b.Length];
        }

        private void ValidateEqualsAfterComplex()
        {
            if (lines.Length == 0)
                return;

            Line1Layout line1 = ParseLine1Layout(lines[0]);
            if (!line1.HasComplex)
                return;

            if (line1.EqIndex < 0)
            {
                AddError("=", 1, line1.ComplexEnd + 1,
                    "Синтаксическая ошибка: отсутствует '=' после 'complex'", false);
            }
        }

        private void ValidateStructure()
        {
            int fieldLine = -1;
            string fieldText = "";
            int endLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNum = i + 1;

                if (lineNum == 1)
                    continue;

                if (ContainsWholeWord(line, "real") || ContainsWholeWord(line, "integer"))
                {
                    fieldLine = lineNum;
                    fieldText = line;

                    int realPos = IndexOfWholeWord(line, "real");
                    if (realPos < 0)
                        realPos = IndexOfWholeWord(line, "integer");

                    if (realPos >= 0)
                    {
                        string beforeType = line.Substring(0, realPos);
                        if (!beforeType.Contains(":"))
                        {
                            AddError(":", lineNum, realPos + 1,
                                "Синтаксическая ошибка: отсутствует ':' перед 'real'", false);
                        }
                        else
                        {
                            ValidateCommaBetweenFieldNames(line, lineNum, realPos);
                        }
                    }
                }

                if (line.Trim().ToLower().StartsWith("end"))
                {
                    endLine = lineNum;
                    if (!line.Trim().EndsWith(";"))
                    {
                        int endPos = line.IndexOf("end", StringComparison.OrdinalIgnoreCase);
                        AddError(";", lineNum, endPos + 4,
                            "Синтаксическая ошибка: отсутствует ';' после 'end'", false);
                    }
                }
            }

            if (fieldLine != -1 && endLine != -1 && !fieldText.Trim().EndsWith(";"))
            {
                int semiPos = fieldText.Length;
                int realIdx = IndexOfWholeWord(fieldText, "real");
                if (realIdx < 0)
                    realIdx = IndexOfWholeWord(fieldText, "integer");
                if (realIdx >= 0)
                    semiPos = realIdx + (ContainsWholeWord(fieldText, "real") ? 4 : 7);

                AddError(";", fieldLine, semiPos,
                    "Синтаксическая ошибка: отсутствует ';' перед 'end'", false);
            }
        }

        /// <summary>
        /// re im: real — нет запятой между идентификаторами.
        /// </summary>
        private void ValidateCommaBetweenFieldNames(string line, int lineNum, int typeKeywordStart)
        {
            int colonIdx = line.LastIndexOf(':', typeKeywordStart);
            if (colonIdx < 0)
                return;

            string beforeColon = line.Substring(0, colonIdx);
            var identifiers = ExtractIdentifiers(beforeColon);

            if (identifiers.Count < 2)
                return;

            for (int i = 1; i < identifiers.Count; i++)
            {
                WordToken prev = identifiers[i - 1];
                WordToken curr = identifiers[i];

                int betweenStart = prev.StartPos + prev.Text.Length;
                int betweenEnd = curr.StartPos;
                string between = beforeColon.Substring(betweenStart, betweenEnd - betweenStart);

                if (!between.Contains(","))
                {
                    AddError(",", lineNum, curr.StartPos + 1,
                        "Синтаксическая ошибка: идентификаторы полей должны быть разделены запятой", false);
                    return;
                }
            }
        }

        private void ValidateDigitsInKeywords()
        {
            if (lines.Length == 0)
                return;

            Line1Layout line1 = ParseLine1Layout(lines[0]);

            if (line1.HasType)
                ScanDigitsInKeywordRegion(lines[0], 1, line1.TypeStart, line1.TypeEnd, "type");

            if (line1.HasComplex)
                ScanDigitsInKeywordRegion(lines[0], 1, line1.ComplexStart, line1.ComplexEnd, "complex");

            if (line1.HasRecord)
                ScanDigitsInKeywordRegion(lines[0], 1, line1.RecordStart, line1.RecordEnd, "record");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNum = i + 1;
                string trimmed = line.TrimStart();

                if (trimmed.Length == 0)
                    continue;

                if (trimmed.ToLower().StartsWith("end"))
                {
                    int leading = line.Length - trimmed.Length;
                    int endLen = EndOfKeywordLetters(trimmed, 0);
                    ScanDigitsInKeywordRegion(line, lineNum, leading, leading + endLen, "end");
                    continue;
                }

                int colonIdx = line.IndexOf(':');
                if (colonIdx >= 0)
                {
                    int fieldTypeStart = SkipSpaces(line, colonIdx + 1);
                    int fieldTypeEnd = EndOfFieldTypeToken(line, fieldTypeStart);

                    if (fieldTypeStart < fieldTypeEnd)
                    {
                        string kw = GuessTypeKeyword(line.Substring(fieldTypeStart, fieldTypeEnd - fieldTypeStart).Trim());
                        ScanDigitsInKeywordRegion(line, lineNum, fieldTypeStart, fieldTypeEnd, kw);
                    }
                }
            }
        }

        private void ScanDigitsInKeywordRegion(string line, int lineNum, int start, int end, string keyword)
        {
            if (start < 0 || end <= start)
                return;

            if (end > line.Length)
                end = line.Length;

            for (int i = start; i < end; i++)
            {
                if (!char.IsDigit(line[i]))
                    continue;

                string digit = line[i].ToString();
                if (ErrorAlreadyExists(lineNum, i + 1, digit))
                    continue;

                AddError(digit, lineNum, i + 1,
                    $"Лексическая ошибка: недопустимая цифра в ключевом слове '{keyword}'", true);
            }
        }

        private static bool ContainsDigit(string text)
        {
            foreach (char c in text)
            {
                if (char.IsDigit(c))
                    return true;
            }
            return false;
        }

        private void ValidateInvalidNumbers()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int lineNum = i + 1;

                if (lineNum == 1)
                    continue;

                if (line.Trim().ToLower().StartsWith("end"))
                    continue;

                for (int j = 0; j < line.Length; j++)
                {
                    if (!char.IsDigit(line[j]))
                        continue;

                    if (j > 0 && (char.IsLetter(line[j - 1]) || line[j - 1] == '_'))
                        continue;

                    int startPos = j;
                    var num = new StringBuilder();

                    while (j < line.Length && (char.IsDigit(line[j]) || line[j] == '.'))
                    {
                        num.Append(line[j]);
                        j++;
                    }

                    string number = num.ToString();
                    if (number.Length == 0)
                        continue;

                    if (ErrorAlreadyExists(lineNum, startPos + 1, number))
                        continue;

                    AddError(number, lineNum, startPos + 1,
                        "Лексическая ошибка: число недопустимо в объявлении record", true);

                    j--;
                }
            }
        }

        private static List<WordToken> ExtractIdentifiers(string text)
        {
            var list = new List<WordToken>();
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsLetter(text[i]) && text[i] != '_')
                    continue;

                int start = i;
                var sb = new StringBuilder();
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
                {
                    sb.Append(text[i]);
                    i++;
                }

                list.Add(new WordToken { Text = sb.ToString(), StartPos = start });
            }

            return list;
        }

        private static int EndOfFirstToken(string line, int from)
        {
            int i = from;
            while (i < line.Length && !char.IsWhiteSpace(line[i]))
                i++;
            return i;
        }

        /// <summary>Конец токена из букв и цифр (1type, co7mplex).</summary>
        private static int EndOfAlphanumericToken(string line, int from)
        {
            int i = from;
            while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_'))
                i++;
            return i;
        }

        /// <summary>Только буквы ключевого слова — без ; : = , после слова.</summary>
        private static int EndOfKeywordLetters(string line, int from)
        {
            int i = from;
            while (i < line.Length && char.IsLetter(line[i]))
                i++;
            return i;
        }

        /// <summary>Токен end до ';' (для проверки en d).</summary>
        private static int EndOfEndKeywordToken(string trimmed)
        {
            int i = 0;
            while (i < trimmed.Length && trimmed[i] != ';')
                i++;
            return i;
        }

        /// <summary>Тип поля после ':' до ';' (для проверки re al).</summary>
        private static int EndOfFieldTypeToken(string line, int from)
        {
            int i = from;
            while (i < line.Length && line[i] != ';')
                i++;
            return i;
        }

        private static bool HasInternalWhitespace(string line, int start, int end)
        {
            if (end > line.Length)
                end = line.Length;

            for (int i = start; i < end; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                    continue;
                if (HasLetterBefore(line, start, i) && HasLetterAfter(line, i + 1, end))
                    return true;
            }
            return false;
        }

        private static int SkipSpaces(string line, int from)
        {
            while (from < line.Length && char.IsWhiteSpace(line[from]))
                from++;
            return from;
        }

        private bool ErrorAlreadyExists(int line, int position, string fragment)
        {
            foreach (var err in errors)
            {
                if (err.Line == line && err.Position == position && err.Fragment == fragment)
                    return true;
            }
            return false;
        }

        private bool IsValidOperator(char ch)
        {
            return ch == '=' || ch == ',' || ch == ';' || ch == ':' || ch == '.';
        }

        private void ReadIdentifier(string line)
        {
            while (currentPos < line.Length && (char.IsLetterOrDigit(line[currentPos]) || line[currentPos] == '_'))
            {
                currentPos++;
            }
        }

        private void ReadNumber(string line)
        {
            StringBuilder sb = new StringBuilder();
            int startPos = currentPos;
            bool hasDot = false;

            while (currentPos < line.Length && (char.IsDigit(line[currentPos]) || line[currentPos] == '.'))
            {
                if (line[currentPos] == '.')
                {
                    if (hasDot)
                    {
                        AddError(sb.ToString() + ".", currentLine, startPos + 1,
                            "Лексическая ошибка: некорректное число (несколько точек)", true);
                        currentPos++;
                        return;
                    }
                    hasDot = true;
                }
                sb.Append(line[currentPos]);
                currentPos++;
            }

            string number = sb.ToString();

            if (number.Length == 0)
                return;

            if (hasDot && (number.EndsWith(".") || number.StartsWith(".")))
            {
                AddError(number, currentLine, startPos + 1,
                    "Лексическая ошибка: некорректное вещественное число (точка в начале или конце)", true);
            }
        }

        private bool ContainsWholeWord(string line, string word)
        {
            return IndexOfWholeWord(line, word) >= 0;
        }

        private static int IndexOfWholeWord(string line, string word)
        {
            int index = 0;
            while (index < line.Length)
            {
                int found = line.IndexOf(word, index, StringComparison.OrdinalIgnoreCase);
                if (found < 0)
                    return -1;

                bool leftOk = found == 0 || !char.IsLetterOrDigit(line[found - 1]);
                int after = found + word.Length;
                bool rightOk = after >= line.Length || !char.IsLetterOrDigit(line[after]);
                if (leftOk && rightOk)
                    return found;

                index = found + 1;
            }

            return -1;
        }

        private void AddError(string fragment, int line, int position, string description, bool isLexical)
        {
            string location = isLexical
                ? $"строка {line}, позиция {position}"
                : "синтаксическая ошибка";
            errors.Add(new ErrorInfo(fragment, location, description, line, position, isLexical));
        }
    }
}
