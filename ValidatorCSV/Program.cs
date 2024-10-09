using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using FluentValidation;
using FluentValidation.Results;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using static ValidatorCSV.Program;



namespace ValidatorCSV
{
    class Program
    {

        // Метод для исправления длины строки и заполнения пустых полей
        static string FixFieldLength(string field, int maxLength, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(field))
                return defaultValue;

            return field.Length > maxLength ? field.Substring(0, maxLength) : field;
        }

        public class CsvRecord
        {
            public string EVENT { get; set; }
            public string OMS_SERIAL { get; set; }
            public string OMS_NUM { get; set; }
            public string ENP { get; set; }
            public string FAM { get; set; }
            public string NAME { get; set; }
            public string PATR { get; set; }
            public string SEX { get; set; }
            public string DATE_BIRTH { get; set; }
            public string MESTO_ROGDEN { get; set; }
            public string OMS_DOC_TYPE { get; set; }
            public string TRUTH_BIRTHDAY { get; set; }
            public string DOC_SERIA { get; set; }
            public string DOC_NUM { get; set; }
            public string DATE_DOC { get; set; }
            public string ORGAN_DOC { get; set; }
            public string SNILS { get; set; }
            public string REGION { get; set; }
            public string DISTRICT { get; set; }
            public string CITY { get; set; }
            public string STREET { get; set; }
            public string HOUSE { get; set; }
            public string COURP { get; set; }
            public string FLAT { get; set; }
            public string SMO { get; set; }
            public string MO { get; set; }
            public string IDENT_DOC_TYPE { get; set; }
            public string DATE_START { get; set; }
            public string DATE_END { get; set; }
            public string SNILS_DOC_FAP { get; set; }
            public string KATEGOR_MED_FAP { get; set; }
            public string KOD_UCHASTKA_FAP { get; set; }
            public string DATE_PRIK_FAP { get; set; }
            public string DATE_OTKR_FAP { get; set; }
            public string FLAG_PHONE { get; set; }
            public string FLAG_SMS { get; set; }
            public string PHONE { get; set; }
            public string ZAIAV_PRIK { get; set; }
            public string DATE_PRIK { get; set; }
            public string DATE_OTKR { get; set; }
            public string OID_LPU { get; set; }
            public string KOD_PODRAZD { get; set; }
            public string KOD_UCHASTKA { get; set; }
            public string SNILS_DOC { get; set; }
            public string KATEGOR_MED { get; set; }
            public string EMAIL { get; set; }
            public string DATE_MODIF { get; set; }
            public string EMP_ID { get; set; }
            public string USER_ID_MO { get; set; }
            public string CONFIRM { get; set; }
            public string KOD_PODRAZD_FAP { get; set; }
            public string LastEmptyField { get; set; } = string.Empty;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Введите имя файла CSV (с расширением):");
            string fileName = Console.ReadLine();
            string filePath = Path.Combine("D:\\Documents", fileName);

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                MissingFieldFound = null,
                BadDataFound = null,
                HeaderValidated = null,
            };

            var encoding1251 = Encoding.GetEncoding(1251);
            var errors = new List<string>();

            // Этап 1: Проверка и исправление данных
            var records = new List<CsvRecord>();
            using (var reader = new StreamReader(filePath, encoding1251))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                int totalLines = File.ReadLines(filePath).Count();
                int currentLine = 0;
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    currentLine++;
                    var record = csv.GetRecord<CsvRecord>();
                    var validationResult = ValidateAndFixCsvRecord(record); // Используем функцию ValidateAndFix

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = new StringBuilder();
                        errorMessages.AppendLine($"Строка {currentLine + 1} ENP: {record.ENP} содержит ошибки:");
                        foreach (var error in validationResult.Errors)
                        {
                            errorMessages.AppendLine($"Столбец '{error.PropertyName}': {error.ErrorMessage}");
                        }
                        errors.Add(errorMessages.ToString());
                    }

                    records.Add(record); // Запоминаем записи (корректные и исправленные)

                    Console.SetCursorPosition(0, Console.CursorTop - 2);
                    Console.WriteLine(GetProgressBar(currentLine, totalLines));
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.WriteLine($"Проверено записей: {currentLine}");
                }
            }

            if (errors.Count > 0)
            {
                Console.WriteLine("Обнаружены ошибки:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }
            else
            {
                Console.WriteLine("Ошибок не обнаружено.");
            }

            // Этап 2: Удаление дубликатов
            var uniqueRecords = records
                .GroupBy(r => r.ENP)
                .Select(g => g.First())
                .ToList();

            // Этап 3: Сохранение файла
            SaveRecordsToCsv(uniqueRecords);

            Console.WriteLine("Процесс завершен.");
            Console.ReadLine();
        }

        // Метод для валидации и исправления записей
        static ValidationResult ValidateAndFixCsvRecord(CsvRecord record)
        {
            var validator = new CsvRecordValidator();
            var validationResult = validator.Validate(record);

            // Исправляем записи при необходимости
            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    switch (failure.PropertyName)
                    {
                        case "EVENT":
                            record.EVENT = FixFieldLength(record.EVENT, 1, "Р");
                            break;
                        case "OMS_SERIAL":
                            record.OMS_SERIAL = FixFieldLength(record.OMS_SERIAL, 10);
                            break;
                        case "OMS_NUM":
                            record.OMS_NUM = FixFieldLength(record.OMS_NUM, 25);
                            break;
                        case "PHONE":
                            record.PHONE = FixFieldLength(record.PHONE, 23);
                            break;
                    }
                }
            }

            return validationResult;
        }

        // Сохранение записей в новый CSV-файл
        static void SaveRecordsToCsv(List<CsvRecord> records)
        {
            foreach (var record in records)
            {
                record.LastEmptyField = string.Empty; // Добавляем пустое поле
            }

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                NewLine = Environment.NewLine,
                HasHeaderRecord = false, // Убираем заголовок
            };

            using (var writer = new StreamWriter("D:\\Documents\\P_output.csv", false, Encoding.GetEncoding(1251)))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                csv.WriteRecords(records);
            }
        }

        // Метод для создания строки индикатора прогресса
        static string GetProgressBar(int current, int total, int barSize = 100)
        {
            int size = barSize - 1;
            double percentage = (double)current / total;
            int progress = (int)(percentage * barSize);
            string progressBar = new string('#', progress).PadRight(size, '.');
            return $"Прогресс: [{progressBar}] {percentage:P0}";
        }

    }

    class CsvRecordValidator : AbstractValidator<CsvRecord>
    {
        public CsvRecordValidator()
        {
            RuleFor(record => record.EVENT).NotEmpty().WithMessage("Поле EVENT не должно быть пустым.")
                .Length(1);
            RuleFor(record => record.OMS_SERIAL).MaximumLength(10);
            RuleFor(record => record.OMS_NUM).MaximumLength(25);
            RuleFor(record => record.ENP)
                .NotEmpty().WithMessage("Поле ENP не должно быть пустым.")
                .Length(16).WithMessage("Поле ENP должно содержать ровно 16 символов.");
            RuleFor(record => record.FAM).NotEmpty().WithMessage("Поле FAM не должно быть пустым.")
                .MaximumLength(40);
            RuleFor(record => record.NAME).MaximumLength(40);
            RuleFor(record => record.PATR).MaximumLength(40);
            RuleFor(record => record.SEX).NotEmpty().WithMessage("Поле SEX не должно быть пустым.")
                .MaximumLength(1);
            RuleFor(record => record.DATE_BIRTH).NotEmpty().WithMessage("Поле DATE_BIRTH не должно быть пустым.")
                .MaximumLength(10);
            RuleFor(record => record.MESTO_ROGDEN).MaximumLength(200);
            RuleFor(record => record.OMS_DOC_TYPE).NotEmpty().WithMessage("Поле OMS_DOC_TYPE не должно быть пустым.")
                .MaximumLength(1);
            RuleFor(record => record.TRUTH_BIRTHDAY).NotEmpty().WithMessage("Поле TRUTH_BIRTHDAY не должно быть пустым.")
                .MaximumLength(1);
            RuleFor(record => record.DOC_SERIA).MaximumLength(20);
            RuleFor(record => record.DOC_NUM).NotEmpty().WithMessage("Поле DOC_NUM не должно быть пустым.")
                .MaximumLength(20);
            RuleFor(record => record.DATE_DOC).MaximumLength(10);
            RuleFor(record => record.ORGAN_DOC).MaximumLength(200);
            RuleFor(record => record.SNILS).MaximumLength(30);
            RuleFor(record => record.REGION).MaximumLength(40);
            RuleFor(record => record.DISTRICT).MaximumLength(40);
            RuleFor(record => record.CITY).MaximumLength(40);
            RuleFor(record => record.STREET).MaximumLength(50);
            RuleFor(record => record.HOUSE).MaximumLength(10);
            RuleFor(record => record.COURP).MaximumLength(10);
            RuleFor(record => record.FLAT).MaximumLength(10);
            RuleFor(record => record.SMO).MaximumLength(20);
            RuleFor(record => record.MO).NotEmpty().WithMessage("Поле MO не должно быть пустым.")
                .MaximumLength(20);
            RuleFor(record => record.IDENT_DOC_TYPE).MaximumLength(20);
            RuleFor(record => record.DATE_START).MaximumLength(10);
            RuleFor(record => record.DATE_END).MaximumLength(10);
            RuleFor(record => record.SNILS_DOC_FAP).MaximumLength(11);
            RuleFor(record => record.KATEGOR_MED_FAP).MaximumLength(1);
            RuleFor(record => record.KOD_UCHASTKA_FAP).MaximumLength(64);
            RuleFor(record => record.DATE_PRIK_FAP).MaximumLength(10);
            RuleFor(record => record.DATE_OTKR_FAP).MaximumLength(10);
            RuleFor(record => record.FLAG_PHONE).NotEmpty().WithMessage("Поле FLAG_PHONE не должно быть пустым.")
                .MaximumLength(1);
            RuleFor(record => record.FLAG_SMS).MaximumLength(1);
            RuleFor(record => record.PHONE).MaximumLength(23);
            RuleFor(record => record.ZAIAV_PRIK).NotEmpty().WithMessage("Поле ZAIAV_PRIK не должно быть пустым.")
                .MaximumLength(1);
            RuleFor(record => record.DATE_PRIK).NotEmpty().WithMessage("Поле DATE_PRIK не должно быть пустым.")
                .MaximumLength(10);
            RuleFor(record => record.DATE_OTKR).MaximumLength(10);
            RuleFor(record => record.OID_LPU).MaximumLength(30);
            RuleFor(record => record.KOD_PODRAZD).NotEmpty().WithMessage("Поле KOD_PODRAZD не должно быть пустым.")
                .MaximumLength(64);
            RuleFor(record => record.KOD_UCHASTKA).MaximumLength(64);
            RuleFor(record => record.SNILS_DOC).MaximumLength(11);
            RuleFor(record => record.KATEGOR_MED).MaximumLength(1);
            RuleFor(record => record.EMAIL).MaximumLength(40);
            RuleFor(record => record.DATE_MODIF).NotEmpty().WithMessage("Поле DATE_MODIF не должно быть пустым.")
                .MaximumLength(10);
            RuleFor(record => record.EMP_ID).MaximumLength(22);
            RuleFor(record => record.USER_ID_MO).NotEmpty().WithMessage("Поле USER_ID_MO не должно быть пустым.")
                .MaximumLength(20);
            RuleFor(record => record.CONFIRM).NotEmpty().WithMessage("Поле CONFIRM не должно быть пустым.")
                .MaximumLength(22);
            RuleFor(record => record.KOD_PODRAZD_FAP).MaximumLength(64);
        }
    }
}
