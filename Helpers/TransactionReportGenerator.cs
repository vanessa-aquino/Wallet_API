using WalletAPI.Interfaces.Repositories;
using System.Globalization;
using WalletAPI.Models;
using System.Text;

namespace WalletAPI.Helpers
{
    public class TransactionReportGenerator
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;

        public TransactionReportGenerator(IWalletRepository walletRepository, ITransactionRepository transactionRepository)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }

        private string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            {
                input = input.Replace("\"", "\"\"");
                return $"\"{input}\"";
            }
            return input;
        }

        public byte[]GenerateCsvReport(IEnumerable<Transaction> transactions) { 

            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var lines = new List<string>
            {
                $"Id{separator}Data{separator}Tipo{separator}Valor{separator}Status{separator}Descrição"
            };

            foreach (var t in transactions)
            {
                var line = $"{t.Id}{separator}{t.Date:dd/MM/yyyy HH:mm}{separator}{t.TransactionType}{separator}R${t.Amount:F2}{separator}{t.Status}{separator}{EscapeCsv(t.Description)}";
                lines.Add(line);
            }

            var csv = string.Join(Environment.NewLine, lines);
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

            return bytes;
        }
    }
}
