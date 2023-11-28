
namespace Infrastructure.Utilities
{
    public static class PasswordValidator
    {
        public static bool Validate(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (password.Length < 8) return false;
            if (!HasNumber(password)) return false;
            if (!HasUpperCase(password)) return false;
            if (!HasLowerCase(password)) return false;   

            return true; 
        }

       
        #region private methods

        private static bool HasNumber(string password)
        {
            int counter = 0;
            foreach (char c in password)
            {
                if (char.IsNumber(c)) counter++;
            }
            if (counter == 0) return false;
            return true;
        }
        private static bool HasUpperCase(string password)
        {
            int counter = 0;
            foreach (char c in password)
            {
                if (char.IsUpper(c)) counter++;
            }
            if (counter == 0) return false;
            return true;
        }
        private static bool HasLowerCase(string password)
        {
            int counter = 0;
            foreach (char c in password)
            {
                if (char.IsLower(c)) counter++;
            }
            if (counter == 0) return false;
            return true;
        }       
        #endregion
    }


}
