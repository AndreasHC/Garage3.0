using Garage3.Data;

namespace Garage3.Helpers
{
    public static class MembershipHelper
    {
        public static bool IsProMember(Member member)
        {
            return member.EndDate >= DateTime.Today;
        }

        public static double CalculateDiscount(Member member, int numberOfSpaces)
        {
            if (!IsProMember(member)) return 0;

            var discountRate = 0.1 * numberOfSpaces;
            discountRate = discountRate > 0.4 ? 0.4 : discountRate; // Max 40% rabatt
            return discountRate;
        }
    }

}