namespace Acme.Workforce.Assets.Assignment.Summary;

using System.Globalization;

/// <summary>How screens and exports show a regional assignment entry.</summary>
public sealed record RegionalAssignmentEntryView(string CodeText, string TitleText, string Active, string GraceDaysText, string Minimum, string EndsAt, string StatusLabel)
{
    public static RegionalAssignmentEntryView ToRegionalAssignmentEntryView(RegionalAssignmentEntry item)
    {
        return new RegionalAssignmentEntryView(
            item.Code,
            item.Title,
            $"{item.Active}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAssignmentEntryView> ToRegionalAssignmentEntryViews(List<RegionalAssignmentEntry> items)
    {
        var views = new List<RegionalAssignmentEntryView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAssignmentEntryView(item));
        }
        return views;
    }

    public static double SumRegionalAssignmentEntryView(RegionalAssignmentEntryView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedMinimum + (double)parsedEndsAt;
    }
}
