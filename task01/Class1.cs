using System.Text;

namespace task01;

public static class StringExtensions
{
    public static bool IsPalindrome(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        var sb = new StringBuilder();
        
        foreach (char c in input.ToLower())
        {
            if (!char.IsPunctuation(c) && !char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        string cleaned = sb.ToString();

        if (cleaned.Length == 0)
        {
            return false;
        }

        char[] charArray = cleaned.ToCharArray();
        Array.Reverse(charArray);
        string reversed = new string(charArray);

        return cleaned == reversed;
    }
}
