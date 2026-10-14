namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient accrual override.</summary>
public sealed record LenientAccrualOverrideView(string CodeText, string LabelText, string FloorValueText, string Retries, string Pinned, string OwnerId, string Region, string JobCode, string ContactEmailText, string StartsAtText, string StatusLabel)
{
    public static LenientAccrualOverrideView ToLenientAccrualOverrideView(LenientAccrualOverride item)
    {
        return new LenientAccrualOverrideView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.OwnerId,
            item.Region,
            Text.Truncate(item.JobCode, 80),
            Text.Truncate(item.ContactEmail, 40),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAccrualOverrideView> ToLenientAccrualOverrideViews(List<LenientAccrualOverride> items)
    {
        var views = new List<LenientAccrualOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAccrualOverrideView(item));
        }
        return views;
    }

    public static double SumLenientAccrualOverrideView(LenientAccrualOverrideView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRetries + (double)parsedStartsAt;
    }
}
