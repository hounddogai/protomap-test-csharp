namespace Acme.Workforce.Talent.Nomination.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a nomination bundle.</summary>
public sealed record NominationBundleView(string Code, string NameText, string Rate, string ChecksumText, string Badge, string MarginText, string DurationMinutes, string Grade, string ExpiresAt, string DepartmentText, string StatusLabel)
{
    public static NominationBundleView ToNominationBundleView(NominationBundle item)
    {
        return new NominationBundleView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Badge,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationBundleView> ToNominationBundleViews(List<NominationBundle> items)
    {
        var views = new List<NominationBundleView>();
        foreach (var item in items)
        {
            views.Add(ToNominationBundleView(item));
        }
        return views;
    }

    public static double SumNominationBundleView(NominationBundleView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedRate + parsedMargin + (double)parsedDurationMinutes;
    }
}
