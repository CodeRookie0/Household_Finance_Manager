using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.GUI
{
    public class TransactionSummaryViewModel
    {
        public ObservableCollection<Transaction> Transactions;

        public TransactionSummaryViewModel(List<Transaction> transactions)
        {
            Transactions = new ObservableCollection<Transaction>(transactions); 
        }


        public string Przychody
        {
            get
            {
                var incomes = Transactions.Where(t => t.TransactionTypeID == 1);
                double totalIncomes = incomes.Sum(t => (double)t.Amount);
                return totalIncomes.ToString("C");
            }
        }

        public string Wydatki
        {
            get
            {
                var expenses = Transactions.Where(t => t.TransactionTypeID == 2);
                double totalExpenses = expenses.Sum(t => (double)t.Amount);
                return totalExpenses.ToString("C");
            }
        }

        public string Bilans
        {
            get
            {
                var incomes = Transactions.Where(t => t.TransactionTypeID == 1).Sum(t => (double)t.Amount);
                var expenses = Transactions.Where(t => t.TransactionTypeID == 2).Sum(t => (double)t.Amount);
                var difference = incomes + expenses;
                return difference.ToString("C");
            }
        }
    }
}
