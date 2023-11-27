﻿namespace Entity
{
    public class Expense
    {
        public Expense(string title, double amount, DateTime date)
        {
            Title = title;
            Amount = amount;
            Date = date;
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public User User { get; set; }
    }
}
