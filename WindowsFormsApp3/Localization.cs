using System;
using System.Collections.Generic;

namespace LexicalAnalyzer
{
    public enum UiLanguage
    {
        Russian,
        English
    }

    public static class Localization
    {
        public static event EventHandler LanguageChanged;

        public static UiLanguage Current { get; private set; } = UiLanguage.Russian;

        private static readonly Dictionary<string, string[]> Strings = new Dictionary<string, string[]>
        {
            // Меню
            { "menu_file", new[] { "Файл", "File" } },
            { "menu_edit", new[] { "Правка", "Edit" } },
            { "menu_text", new[] { "Текст", "Text" } },
            { "menu_run", new[] { "Пуск", "Run" } },
            { "menu_help", new[] { "Справка", "Help" } },
            { "menu_language", new[] { "Язык", "Language" } },
            { "lang_russian", new[] { "Русский", "Russian" } },
            { "lang_english", new[] { "English", "English" } },

            { "file_new", new[] { "Новый", "New" } },
            { "file_open", new[] { "Открыть...", "Open..." } },
            { "file_save", new[] { "Сохранить", "Save" } },
            { "file_save_as", new[] { "Сохранить как...", "Save As..." } },
            { "file_exit", new[] { "Выход", "Exit" } },

            { "edit_undo", new[] { "Отменить", "Undo" } },
            { "edit_redo", new[] { "Повторить", "Redo" } },
            { "edit_cut", new[] { "Вырезать", "Cut" } },
            { "edit_copy", new[] { "Копировать", "Copy" } },
            { "edit_paste", new[] { "Вставить", "Paste" } },
            { "edit_select_all", new[] { "Выделить всё", "Select All" } },

            { "text_task", new[] { "Постановка задачи", "Problem Statement" } },
            { "text_grammar", new[] { "Грамматика", "Grammar" } },
            { "text_classification", new[] { "Классификация грамматики", "Grammar Classification" } },
            { "text_method", new[] { "Метод анализа", "Analysis Method" } },
            { "text_test", new[] { "Тестовый пример", "Test Example" } },
            { "text_literature", new[] { "Список литературы", "References" } },
            { "text_source", new[] { "Исходный код программы", "Source Code" } },

            { "run_analyze", new[] { "Запустить анализ", "Run Analysis" } },

            { "help_help", new[] { "Вызов справки", "Help" } },
            { "help_about", new[] { "О программе", "About" } },

            // Панель инструментов
            { "tb_new", new[] { "📄 Новый", "📄 New" } },
            { "tb_open", new[] { "📂 Открыть", "📂 Open" } },
            { "tb_save", new[] { "💾 Сохранить", "💾 Save" } },
            { "tb_undo", new[] { "↩️ Отменить", "↩️ Undo" } },
            { "tb_redo", new[] { "↪️ Повторить", "↪️ Redo" } },
            { "tb_cut", new[] { "✂️ Вырезать", "✂️ Cut" } },
            { "tb_copy", new[] { "📋 Копировать", "📋 Copy" } },
            { "tb_paste", new[] { "📌 Вставить", "📌 Paste" } },
            { "tb_run", new[] { "▶️ Запуск", "▶️ Run" } },
            { "tb_help", new[] { "❓ Справка", "❓ Help" } },

            // Панели
            { "label_errors_table", new[] { " ТАБЛИЦА ОШИБОК", " ERROR TABLE" } },
            { "label_errors_log", new[] { " ЛОГ ОШИБОК", " ERROR LOG" } },
            { "tab_errors_table", new[] { "Таблица ошибок", "Error Table" } },
            { "tab_errors_log", new[] { "Лог ошибок", "Error Log" } },

            { "col_fragment", new[] { "Неверный фрагмент", "Invalid Fragment" } },
            { "col_location", new[] { "Местоположение", "Location" } },
            { "col_description", new[] { "Описание ошибки", "Error Description" } },

            { "title_app", new[] { "Pascal Анализатор", "Pascal Analyzer" } },
            { "title_new_file", new[] { "Новый файл", "New File" } },

            { "dlg_save_title", new[] { "Сохранение", "Save" } },
            { "dlg_save_msg", new[] { "Файл сохранен: {0}", "File saved: {0}" } },
            { "dlg_save_changes", new[] { "Сохранить изменения?", "Save changes?" } },
            { "dlg_question", new[] { "Вопрос", "Question" } },
            { "dlg_filter", new[] { "Pascal files|*.pas|Text files|*.txt|All files|*.*", "Pascal files|*.pas|Text files|*.txt|All files|*.*" } },

            { "err_location_syntax", new[] { "синтаксическая ошибка", "syntax error" } },
            { "err_location_line", new[] { "строка {0}, позиция {1}", "line {0}, position {1}" } },

            // Лог анализа
            { "log_no_code", new[] { "ОШИБКА: Нет кода для анализа", "ERROR: No code to analyze" } },
            { "log_enter_code", new[] { "Пожалуйста, введите код Pascal в редактор.", "Please enter Pascal code in the editor." } },
            { "log_empty_grid", new[] { "Редактор пуст - нет кода для анализа", "Editor is empty - no code to analyze" } },
            { "log_start", new[] { " НАЧАЛО АНАЛИЗА ", " ANALYSIS START " } },
            { "log_no_lexical", new[] { "✓ ЛЕКСИЧЕСКИХ ОШИБОК НЕ ОБНАРУЖЕНО", "✓ NO LEXICAL ERRORS FOUND" } },
            { "log_no_syntax", new[] { "✓ СИНТАКСИЧЕСКИХ ОШИБОК НЕ ОБНАРУЖЕНО", "✓ NO SYNTAX ERRORS FOUND" } },
            { "log_success", new[] { " АНАЛИЗ ЗАВЕРШЕН УСПЕШНО ", " ANALYSIS COMPLETED SUCCESSFULLY " } },
            { "log_ok_grid", new[] { "Ошибок не найдено", "No errors found" } },
            { "log_found", new[] { "*** НАЙДЕНО {0} ОШИБОК ***", "*** FOUND {0} ERROR(S) ***" } },
            { "log_lexical_count", new[] { "Лексических ошибок: {0}", "Lexical errors: {0}" } },
            { "log_syntax_count", new[] { "Синтаксических ошибок: {0}", "Syntax errors: {0}" } },
            { "log_lexical_header", new[] { " ЛЕКСИЧЕСКИЕ ОШИБКИ ", " LEXICAL ERRORS " } },
            { "log_syntax_header", new[] { " СИНТАКСИЧЕСКИЕ ОШИБКИ ", " SYNTAX ERRORS " } },
            { "log_lexical_line", new[] { "Строка {0}, позиция {1}: '{2}' - {3}", "Line {0}, position {1}: '{2}' - {3}" } },
            { "log_end", new[] { " КОНЕЦ АНАЛИЗА ", " ANALYSIS END " } },

            // Диалоги справки
            { "about_title", new[] { "О программе", "About" } },
            { "about_text", new[] { "Лексический анализатор Pascal\nВерсия 3.0\nРазработчик: Зоркольцев И.А.", "Pascal Lexical Analyzer\nVersion 3.0\nDeveloper: Zorkoltsev I.A." } },
            { "task_title", new[] { "Постановка задачи", "Problem Statement" } },
            { "task_text", new[] { "Постановка задачи: Вариант 13: Объявление и определение записи (record) в Pascal", "Problem: Variant 13 — record declaration and definition in Pascal" } },
            { "grammar_title", new[] { "Грамматика", "Grammar" } },
            { "grammar_text", new[] { "Грамматика G[Z]\nZ → \"type\" ID \"=\" \"record\" FIELD_LIST \"end\" \";\"\nFIELD_LIST → ( FIELD_DEF \";\" )* FIELD_DEF\nFIELD_DEF → ID_LIST \":\" TYPE_NAME\nID_LIST → ID ( \",\" ID )*\nTYPE_NAME → \"real\" | \"integer\" | \"string\"", "Grammar G[Z]\nZ → \"type\" ID \"=\" \"record\" FIELD_LIST \"end\" \";\"\nFIELD_LIST → ( FIELD_DEF \";\" )* FIELD_DEF\nFIELD_DEF → ID_LIST \":\" TYPE_NAME\nID_LIST → ID ( \",\" ID )*\nTYPE_NAME → \"real\" | \"integer\" | \"string\"" } },
            { "class_title", new[] { "Классификация", "Classification" } },
            { "class_text", new[] { "Классификация грамматики\nКонтекстно-свободная (тип 2)", "Grammar classification\nContext-free (type 2)" } },
            { "method_title", new[] { "Метод анализа", "Analysis Method" } },
            { "method_text", new[] { "Метод анализа\nНисходящий разбор + метод Айронса", "Analysis method\nTop-down parsing + Irons method" } },
            { "lit_title", new[] { "Литература", "References" } },
            { "lit_text", new[] { "Список литературы\nАхо А., Ульман Дж. Теория синтаксического анализа", "References\nAho A., Ullman J. Principles of Compiler Design" } },
            { "source_title", new[] { "Исходный код", "Source Code" } },
            { "source_text", new[] { "Исходный код программы", "Program source code" } },
            { "help_title", new[] { "Справка", "Help" } },
            { "help_text", new[] { "Справка: используйте меню «Файл», «Правка», «Пуск» (F5 — анализ), «Текст», «Язык» для смены интерфейса.", "Help: use File, Edit, Run (F5 — analyze), Text, and Language menus to switch UI language." } },
        };

        private static readonly Dictionary<string, string> ErrorTranslations = new Dictionary<string, string>
        {
            { "Недопустимый символ", "Invalid character" },
            { "Некорректное число", "Invalid number" },
            { "Некорректное вещественное число", "Invalid real number" },
            { "Синтаксическая ошибка: недопустимый символ", "Syntax error: invalid character" },
            { "Синтаксическая ошибка: недопустимый пробел", "Syntax error: invalid space" },
            { "Синтаксическая ошибка: отсутствует ':' перед 'real'", "Syntax error: missing ':' before 'real'" },
            { "Синтаксическая ошибка: отсутствует ';' после 'end'", "Syntax error: missing ';' after 'end'" },
            { "Синтаксическая ошибка: отсутствует ';' перед 'end'", "Syntax error: missing ';' before 'end'" },
            { "Синтаксическая ошибка: отсутствует '=' после 'complex'", "Syntax error: missing '=' after 'complex'" },
            { "Синтаксическая ошибка: идентификаторы полей должны быть разделены запятой", "Syntax error: field identifiers must be separated by a comma" },
            { "Лексическая ошибка: число недопустимо в объявлении record", "Lexical error: number not allowed in record declaration" },
            { "Лексическая ошибка: недопустимая цифра в ключевом слове", "Lexical error: invalid digit in keyword" },
            { "Лексическая ошибка: искажённое или неполное ключевое слово", "Lexical error: distorted or incomplete keyword" },
            { "Лексическая ошибка: некорректное число", "Lexical error: invalid number" },
            { "Лексическая ошибка: некорректное вещественное число", "Lexical error: invalid real number" },
            { "Ожидается", "Expected" },
            { "найдено", "found" },
            { "в ключевом слове", "in keyword" },
            { "код ASCII", "ASCII code" },
            { "несколько точек", "multiple dots" },
            { "точка в начале или конце", "dot at start or end" },
        };

        public static void SetLanguage(UiLanguage language)
        {
            if (Current == language)
                return;

            Current = language;
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        public static string Get(string key)
        {
            if (!Strings.TryGetValue(key, out string[] values))
                return key;

            return values[Current == UiLanguage.English ? 1 : 0];
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        public static string TranslateErrorDescription(string description)
        {
            if (Current == UiLanguage.Russian || string.IsNullOrEmpty(description))
                return description;

            string result = description;
            foreach (var pair in ErrorTranslations)
            {
                result = result.Replace(pair.Key, pair.Value);
            }
            return result;
        }

        public static string FormatErrorLocation(ErrorInfo err)
        {
            if (!err.IsLexical)
                return Get("err_location_syntax");

            if (err.Location == "синтаксическая ошибка" || err.Location == "syntax error")
                return Get("err_location_syntax");

            return Format("err_location_line", err.Line, err.Position);
        }
    }
}
