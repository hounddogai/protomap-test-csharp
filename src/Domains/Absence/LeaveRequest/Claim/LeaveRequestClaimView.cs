namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a leave request claim.</summary>
public sealed record LeaveRequestClaimView(string Code, string Title, string Currency, string ContactEmail, string SalaryBand, string Description, string PhoneText, string UpdatedAt, string ExpiresAt, string DiscountText, string StatusLabel)
{
    public static LeaveRequestClaimView ToLeaveRequestClaimView(LeaveRequestClaim item)
    {
        return new LeaveRequestClaimView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Currency,
            item.ContactEmail,
            item.SalaryBand,
            item.Description,
            item.Phone,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LeaveRequestClaimView> ToLeaveRequestClaimViews(List<LeaveRequestClaim> items)
    {
        var views = new List<LeaveRequestClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLeaveRequestClaimView(item));
        }
        return views;
    }

    public static double SumLeaveRequestClaimView(LeaveRequestClaimView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedExpiresAt + parsedDiscount;
    }
}
