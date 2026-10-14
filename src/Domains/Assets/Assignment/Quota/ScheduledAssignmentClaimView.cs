namespace Acme.Workforce.Assets.Assignment.Quota;

using System.Globalization;

/// <summary>How screens and exports show a scheduled assignment claim.</summary>
public sealed record ScheduledAssignmentClaimView(string CodeText, string Title, string Fte, string Department, string Hours, string Channel, string Ceiling, string Visible, string Rating, string StatusLabel)
{
    public static ScheduledAssignmentClaimView ToScheduledAssignmentClaimView(ScheduledAssignmentClaim item)
    {
        return new ScheduledAssignmentClaimView(
            item.Code,
            item.Title,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledAssignmentClaimView> ToScheduledAssignmentClaimViews(List<ScheduledAssignmentClaim> items)
    {
        var views = new List<ScheduledAssignmentClaimView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledAssignmentClaimView(item));
        }
        return views;
    }

    public static double SumScheduledAssignmentClaimView(ScheduledAssignmentClaimView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedFte + parsedHours + parsedCeiling;
    }
}
