namespace Acme.Workforce.Compliance.Exception.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception segment.</summary>
public sealed record ExceptionSegmentView(string Code, string Title, string ManagerId, string Source, string Notes, string Limit, string RegionText, string StatusLabel)
{
    public static ExceptionSegmentView ToExceptionSegmentView(ExceptionSegment item)
    {
        return new ExceptionSegmentView(
            item.Code,
            item.Title,
            Text.Truncate(item.ManagerId, 120),
            item.Source,
            Text.Truncate(item.Notes, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionSegmentView> ToExceptionSegmentViews(List<ExceptionSegment> items)
    {
        var views = new List<ExceptionSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionSegmentView(item));
        }
        return views;
    }

    public static double SumExceptionSegmentView(ExceptionSegmentView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
