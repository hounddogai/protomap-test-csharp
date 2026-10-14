namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped visitor window.</summary>
public sealed record CappedVisitorWindowView(string Code, string Title, string RegionText, string SummaryText, string JobCode, string Priority, string Threshold, string StatusLabel)
{
    public static CappedVisitorWindowView ToCappedVisitorWindowView(CappedVisitorWindow item)
    {
        return new CappedVisitorWindowView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Region,
            item.Summary,
            Text.Truncate(item.JobCode, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedVisitorWindowView> ToCappedVisitorWindowViews(List<CappedVisitorWindow> items)
    {
        var views = new List<CappedVisitorWindowView>();
        foreach (var item in items)
        {
            views.Add(ToCappedVisitorWindowView(item));
        }
        return views;
    }

    public static double SumCappedVisitorWindowView(CappedVisitorWindowView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedThreshold;
    }
}
