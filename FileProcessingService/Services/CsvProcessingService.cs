using FileProcessingService.Models;
using FileProcessingService.Services.Interfaces;
using System.Globalization;

namespace FileProcessingService.Services
{
    public class CsvProcessingService : ICsvProcessingService
    {
        private readonly ILogger<CsvProcessingService> _logger;

        public CsvProcessingService(ILogger<CsvProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<CsvProcessingResult> CalculateColumnAverageAsync(Stream stream, string? preferredColumn)
        {
            using var reader = new StreamReader(stream);

            var headerLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(headerLine))
            {
                throw new InvalidOperationException(
                    "CSV file is empty or missing a header row.");
            }

            var headers = headerLine.Split(',');

            var rows = new List<string[]>();
            string? line;
            while((line = await reader.ReadLineAsync()) is not null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                rows.Add(line.Split(','));
            }

            if(rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "CSV file has a header but no data rows.");
            }

            int columnIndex;
            string columnName;

            if (!string.IsNullOrWhiteSpace(preferredColumn))
            {
                columnIndex = Array.FindIndex(headers,
                    h =>
                    string.Equals(h.Trim(), 
                    preferredColumn.Trim(), 
                    StringComparison.OrdinalIgnoreCase));

                if(columnIndex < 0)
                {
                    throw new InvalidOperationException(
                        $"Requested column '{preferredColumn}' was not found in the CSV header.");
                }

                columnName = headers[columnIndex];
            }
            else
            {
                columnIndex = -1;

                for(var col = 0; col < headers.Length; col++)
                {
                    var allNumeric = rows.All(r =>
                    col < r.Length &&
                    double.TryParse(
                        r[col], 
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture, 
                        out _));

                    if (allNumeric)
                    {
                        columnIndex = col;
                        break;
                    }
                }

                if(columnIndex < 0)
                {
                    throw new InvalidOperationException(
                        "No numeric column found to calculate an average from.");
                }

                columnName = headers[columnIndex];
            }

            var values = new List<double>();
            foreach(var row in rows)
            {
                if (columnIndex >= row.Length)
                    continue;

                if(double.TryParse(
                    row[columnIndex],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var value))
                {
                    values.Add(value);
                }
            }

            if(values.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Column '{columnName}' contained no numeric values that could be averaged.");
            }

            var average = values.Average();

            _logger.LogInformation(
                $"Processed CSV: {rows.Count} rows, column '{columnName}, average {average}");

            return new CsvProcessingResult
            {
                RowsProcessed = rows.Count,
                ColumnAnalyzed = columnName,
                Average = Math.Round(average, 4)
            };
        }
    }
}
