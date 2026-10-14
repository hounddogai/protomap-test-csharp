namespace Acme.Workforce.Talent.Pool.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool baseline.</summary>
public sealed record PoolBaselineView(string Code, string Name, string Priority, string EndsAt, string EmployeeId, string Title, string GradeText, string Badge, string StatusLabel)
{
    public static PoolBaselineView ToPoolBaselineView(PoolBaseline item)
    {
        return new PoolBaselineView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.Grade, 80),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolBaselineView> ToPoolBaselineViews(List<PoolBaseline> items)
    {
        var views = new List<PoolBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPoolBaselineView(item));
        }
        return views;
    }

    public static double SumPoolBaselineView(PoolBaselineView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedEndsAt;
    }
}
