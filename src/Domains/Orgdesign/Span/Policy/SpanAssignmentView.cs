namespace Acme.Workforce.Orgdesign.Span.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span assignment.</summary>
public sealed record SpanAssignmentView(string CodeText, string Name, string UpdatedAt, string MinimumText, string WindowDays, string Score, string Budget, string StatusLabel)
{
    public static SpanAssignmentView ToSpanAssignmentView(SpanAssignment item)
    {
        return new SpanAssignmentView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanAssignmentView> ToSpanAssignmentViews(List<SpanAssignment> items)
    {
        var views = new List<SpanAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToSpanAssignmentView(item));
        }
        return views;
    }

    public static double SumSpanAssignmentView(SpanAssignmentView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMinimum + (double)parsedWindowDays;
    }
}
