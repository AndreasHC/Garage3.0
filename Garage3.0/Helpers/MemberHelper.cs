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


        public static bool IsProMember(Member member, Membership membership)
        {
            ArgumentNullException.ThrowIfNull(member);
            ArgumentNullException.ThrowIfNull(membership);

            if (membership.EndDate < DateTime.Today) return true;
            var birthDate = MemberHelper.GetBirthDate(member.PersonalIdentificationNumber);

            if (birthDate is null) throw new InvalidOperationException($"Unable to get birth date on member `{member.Id}`.");

            return birthDate?.AddYears(65) < DateTime.Today;
        }
    }
}
