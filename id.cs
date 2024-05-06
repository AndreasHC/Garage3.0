using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
{
        string[] testData = { "990101-1234", "231212-8765", "111111-1111", "000212-3456", "000212-3457", "19990101-1234" };

        foreach (string testInput in testData)
        {
            bool isValid = ValidateSwedishPersonalIdNumber(testInput);
            Console.WriteLine($"{testInput}: {(isValid ? "Valid" : "Invalid")}");
        }
    }

    static bool ValidateSwedishPersonalIdNumber(string personNum)
{
        // Regex pattern for Swedish Personal Identity Number with optional century
string pattern = @"^(?:(?:1[9-9]|20)\d{2}|[12]\d{3})(?:0[1-9]|1[0-2])(?:0[1-9]|[12][0-9]|3[01])(?:-?\d{4})?$";

        // Check if the input matches the pattern
if (!Regex.IsMatch(personNum, pattern))
            return false;

        // Extract the date components and the checksum from the input string
int year, month, day, checksum;
        if (!TryExtractDateComponents(personNum, out year, out month, out day, out checksum))
            return false;

        // Check if the extracted date components form a valid date
DateTime date;
        if (!DateTime.TryParse($"{month:D2}/{day:D2}/{year:D4}", out date))
            return false;

        // Validate the checksum
return ValidateChecksum(year, month, day, checksum);
    }

    static bool TryExtractDateComponents(string input, out int year, out int month, out int day, out int checksum)
{
        if (input.Length == 12)
        {
            if (!int.TryParse(input.Substring(0, 4), out year) ||
                !int.TryParse(input.Substring(4, 2), out month) ||
                !int.TryParse(input.Substring(6, 2), out day) ||
                !int.TryParse(input.Substring(9, 1), out checksum))
            {
                return false;
            }
        }
        else
{
            if (!int.TryParse(input.Substring(0, 2), out year) ||
                !int.TryParse(input.Substring(2, 2), out month) ||
		
		!int.TryParse(input.Substring(4, 2), out day) ||
                !int.TryParse(input.Substring(7, 1), out checksum))
            {
                return false;
            }
        }

        return true;
    }

    static bool ValidateChecksum(int year, int month, int day, int checksum)
    {
        int sum = (year % 10) + (month % 10) + (day % 10);

        return (10 - (sum % 10)) % 10 == checksum;
    }
}
