namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient leave request claim.</summary>
public sealed record LenientLeaveRequestClaimView(string Code, string Label, string EmployeeId, string Reviewer, string Amount, string Phone, string TenureMonths, string Version, string ExpiresAtText, string Name, string StatusLabel)
{
    public static LenientLeaveRequestClaimView ToLenientLeaveRequestClaimView(LenientLeaveRequestClaim item)
    {
        return new LenientLeaveRequestClaimView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.EmployeeId,
            Text.Truncate(item.Reviewer, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientLeaveRequestClaimView> ToLenientLeaveRequestClaimViews(List<LenientLeaveRequestClaim> items)
    {
        var views = new List<LenientLeaveRequestClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLenientLeaveRequestClaimView(item));
        }
        return views;
    }

    public static double SumLenientLeaveRequestClaimView(LenientLeaveRequestClaimView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedTenureMonths + (double)parsedVersion;
    }
}
