using System;

namespace Main.Logic
{
    internal class FamilyCodeGenerator
    {
        public string GenerateUniqueFamilyCode()
        {
            string newCode;
            do
            {
                newCode = GenerateRandomCode();
            }
            while (Service.IsFamilyCodeInUse(newCode));

            return newCode;
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new char[9];
            for (int i = 0; i < 9; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }
            return new string(code);
        }
    }
}
