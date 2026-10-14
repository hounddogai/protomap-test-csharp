namespace Acme.Workforce.Compliance.Exception.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception assignment.</summary>
public sealed record ExceptionAssignmentView(string Code, string Title, string Archived, string Locale, string Country, string CostCenter, string Rate, string TeamText, string Summary, string Location, string StatusLabel)
{
    public static ExceptionAssignmentView ToExceptionAssignmentView(ExceptionAssignment item)
    {
        return new ExceptionAssignmentView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            Text.Truncate(item.Locale, 120),
            item.Country,
            item.CostCenter,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Summary,
            Text.Truncate(item.Location, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionAssignmentView> ToExceptionAssignmentViews(List<ExceptionAssignment> items)
    {
        var views = new List<ExceptionAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionAssignmentView(item));
        }
        return views;
    }

    public static double SumExceptionAssignmentView(ExceptionAssignmentView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedRate;
    }
}
