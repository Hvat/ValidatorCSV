using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using FluentValidation;
using FluentValidation.Results;
using static ValidatorCSV.Program;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ValidatorCSV
{
    class Program
    {
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
            public string CC_R { get; set; }
            public string RRR_R { get; set; }
            public string GGG_R { get; set; }
            public string PPP_R { get; set; }
            public string YYYY_R { get; set; }
            public string FLAG_PHONE { get; set; }
            public string FLAG_SMS { get; set; }
            public string PHONE { get; set; }
            public string ZAIAV_PRIK { get; set; }
            public string TYPE_PRIK { get; set; }
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
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Введите путь к файлу CSV:");
            string filePath = Console.ReadLine(); // Пользователь вводит путь к файлу CSV

            Console.WriteLine("Выберите операцию: 1 - Устранение дубликатов, 2 - Поиск ошибок");

            int operationChoice;

            if (!int.TryParse(Console.ReadLine(), out operationChoice))
            {
                Console.WriteLine("Некорректный выбор операции. Завершение программы.");
                return;
            }

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",
                MissingFieldFound = null,
                BadDataFound = null,
            };

            var encoding1251 = Encoding.GetEncoding(1251);

            using (var reader = new StreamReader(filePath, encoding1251))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var record = csv.GetRecord<CsvRecord>();

                switch (operationChoice)
                {
                    case 1:
                        RemoveDuplicatesAndSave(record, csv, csvConfig);
                        break;
                    case 2:
                        DetectAndPrintErrors(record, csv);
                        break;
                    default:
                        Console.WriteLine("Некорректный выбор операции.");
                        break;
                }
            }
            Console.WriteLine("Завершено.");
            Console.ReadLine();
        }

        static void DetectAndPrintErrors(dynamic rec, dynamic csv)
        {
            int lineNumber = 1;

            while (csv.Read())
            {
                lineNumber++;

                var validationResult = ValidateCsvRecord(rec);

                if (!validationResult.IsValid)
                {
                    Console.WriteLine($"Строка {lineNumber} содержит ошибки:");

                    foreach (var error in validationResult.Errors)
                    {
                        Console.WriteLine($"Столбец '{error.PropertyName}': {error.ErrorMessage}");
                    }
                }
            }
        }

        // Проверяем наличие дубликатов в столбце ENP
        static void RemoveDuplicatesAndSave(dynamic rec, dynamic csv, dynamic conf)
        {
            var uniqueENPSet = new HashSet<string>();
            var records = new List<CsvRecord>();
            int lineNumber = 1;

            while (csv.Read())
            {
                lineNumber++;

                if (uniqueENPSet.Add(rec.ENP))
                {
                    records.Add(rec); // ENP уникален, добавим запись в список
                }
                else
                {
                    Console.WriteLine($"Строка {lineNumber} - дубликат");
                }
            }
            
            SaveRecordsToCsv(records, conf); // Сохранить записи в новый файл CSV
        }

        // Метод для валидации записи CSV
        static ValidationResult ValidateCsvRecord(dynamic rec)
        {
            var validator = new CsvRecordValidator();
            return validator.Validate(rec);
        }

        // Сохранение записей в новый файл CSV
        static void SaveRecordsToCsv(List<CsvRecord> records, dynamic conf)
        {
            using (var writer = new StreamWriter("D:\\Documents\\P_output.csv", false, Encoding.GetEncoding(1251)))
            using (var csv = new CsvWriter(writer, conf))
            {
                csv.WriteRecords(records);
            }
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
            RuleFor(record => record.SEX).NotEmpty().MaximumLength(1);
            RuleFor(record => record.DATE_BIRTH).MaximumLength(10);
            RuleFor(record => record.MESTO_ROGDEN).MaximumLength(100);
            RuleFor(record => record.OMS_DOC_TYPE).MaximumLength(1);
            RuleFor(record => record.TRUTH_BIRTHDAY).NotEmpty().WithMessage("Поле TRUTH_BIRTHDAY не должно быть пустым.")
                .MaximumLength(1);
            RuleFor(record => record.DOC_SERIA).MaximumLength(20);
            RuleFor(record => record.DOC_NUM).MaximumLength(20);
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
            RuleFor(record => record.MO).MaximumLength(20);
            RuleFor(record => record.IDENT_DOC_TYPE).MaximumLength(20);
            RuleFor(record => record.DATE_START).MaximumLength(10);
            RuleFor(record => record.DATE_END).MaximumLength(10);
            RuleFor(record => record.CC_R).MaximumLength(2);
            RuleFor(record => record.RRR_R).MaximumLength(3);
            RuleFor(record => record.GGG_R).MaximumLength(3);
            RuleFor(record => record.PPP_R).MaximumLength(3);
            RuleFor(record => record.YYYY_R).MaximumLength(4);
            RuleFor(record => record.FLAG_PHONE).MaximumLength(22);
            RuleFor(record => record.FLAG_SMS).MaximumLength(22);
            RuleFor(record => record.PHONE).MaximumLength(20);
            RuleFor(record => record.ZAIAV_PRIK).NotEmpty().MaximumLength(1);
            RuleFor(record => record.TYPE_PRIK).MaximumLength(3);
            RuleFor(record => record.DATE_PRIK).NotEmpty().WithMessage("Поле DATE_PRIK не должно быть пустым.")
                .MaximumLength(10);
            RuleFor(record => record.DATE_OTKR).MaximumLength(10);
            RuleFor(record => record.OID_LPU).MaximumLength(30);
            RuleFor(record => record.KOD_PODRAZD).MaximumLength(64);
            RuleFor(record => record.KOD_UCHASTKA).MaximumLength(64);
            RuleFor(record => record.SNILS_DOC).MaximumLength(11);
            RuleFor(record => record.KATEGOR_MED).NotEmpty().WithMessage("Поле KATEGOR_MED не должно быть пустым.")
                .MaximumLength(1);
            RuleFor(record => record.DATE_MODIF).MaximumLength(10);
            RuleFor(record => record.EMP_ID).MaximumLength(22);
            RuleFor(record => record.USER_ID_MO).MaximumLength(20);
        }
    }
}
