namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using System.Globalization;

/// <summary>How screens and exports show a scheduled garnishment group.</summary>
public sealed record ScheduledGarnishmentGroupView(string Code, string NameText, string Hours, string MaximumText, string Priority, string Description, string Rating, string StatusLabel)
{
    public static ScheduledGarnishmentGroupView ToScheduledGarnishmentGroupView(ScheduledGarnishmentGroup item)
    {
        return new ScheduledGarnishmentGroupView(
            item.Code,
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledGarnishmentGroupView> ToScheduledGarnishmentGroupViews(List<ScheduledGarnishmentGroup> items)
    {
        var views = new List<ScheduledGarnishmentGroupView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledGarnishmentGroupView(item));
        }
        return views;
    }

    public static double SumScheduledGarnishmentGroupView(ScheduledGarnishmentGroupView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        return parsedHours + parsedMaximum + (double)parsedPriority;
    }
}
