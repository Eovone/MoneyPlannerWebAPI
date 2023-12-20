namespace Infrastructure.Utilities
{
    public static class Validator
    {
        public static bool IsValidLength(string title)
        {
            if (string.IsNullOrEmpty(title)) return false;
            if (title.Length > 50) return false;
            if (title.Length < 2) return false;
            return true;
        }

        public static bool IsValidAmount(double amount)
        {
            if (amount < 1) return false;
            if (amount > 10000000) return false;
            return true;
        }
    }
}
