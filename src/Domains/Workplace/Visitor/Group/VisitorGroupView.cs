namespace Acme.Workforce.Workplace.Visitor.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor group.</summary>
public sealed record VisitorGroupView(string CodeText, string Title, string Badge, string HoursText, string Grade, string StatusLabel)
{
    public static VisitorGroupView ToVisitorGroupView(VisitorGroup item)
    {
        return new VisitorGroupView(
            item.Code,
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Badge, 40),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorGroupView> ToVisitorGroupViews(List<VisitorGroup> items)
    {
        var views = new List<VisitorGroupView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorGroupView(item));
        }
        return views;
    }

    public static double SumVisitorGroupView(VisitorGroupView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedHours;
    }
}
