using Garage3.Data;

namespace Garage3.Helpers
{
    public static class MemberHelper
    {
        public static DateTime? GetBirthDate(string id)
        {
            int year;
            int month;
            int day;

            if (!int.TryParse(id.Substring(0, 4), out year) ||
                !int.TryParse(id.Substring(4, 2), out month) ||
                !int.TryParse(id.Substring(6, 2), out day)) return null;

            return new DateTime(year, month, day);
        }

        internal static string Format(MembershipType value)
        {
            return Enum.GetName(typeof(MembershipType), value)?.Replace('_', ' ')!;
        }
    }
}
