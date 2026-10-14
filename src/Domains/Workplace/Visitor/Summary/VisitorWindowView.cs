namespace Acme.Workforce.Workplace.Visitor.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor window.</summary>
public sealed record VisitorWindowView(string CodeText, string Name, string ExternalRef, string EmployeeId, string Capacity, string Segment, string LimitText, string RegionText, string Score, string StatusLabel)
{
    public static VisitorWindowView ToVisitorWindowView(VisitorWindow item)
    {
        return new VisitorWindowView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.ExternalRef,
            Text.Truncate(item.EmployeeId, 64),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorWindowView> ToVisitorWindowViews(List<VisitorWindow> items)
    {
        var views = new List<VisitorWindowView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorWindowView(item));
        }
        return views;
    }

    public static double SumVisitorWindowView(VisitorWindowView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedLimit + parsedScore;
    }
}
