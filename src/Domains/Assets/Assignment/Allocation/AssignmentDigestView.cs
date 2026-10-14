namespace Acme.Workforce.Assets.Assignment.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a assignment digest.</summary>
public sealed record AssignmentDigestView(string CodeText, string Label, string DurationMinutesText, string Fte, string AttemptsText, string Summary, string TenureMonthsText, string CountryText, string Approved, string QuantityText, string ContactEmailText, string StatusLabel)
{
    public static AssignmentDigestView ToAssignmentDigestView(AssignmentDigest item)
    {
        return new AssignmentDigestView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 120),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AssignmentDigestView> ToAssignmentDigestViews(List<AssignmentDigest> items)
    {
        var views = new List<AssignmentDigestView>();
        foreach (var item in items)
        {
            views.Add(ToAssignmentDigestView(item));
        }
        return views;
    }

    public static double SumAssignmentDigestView(AssignmentDigestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedFte + (double)parsedAttempts;
    }
}
