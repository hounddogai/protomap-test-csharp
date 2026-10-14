namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled statement of work window.</summary>
public sealed record ScheduledStatementOfWorkWindowView(string Code, string Label, string Country, string Active, string Rating, string StatusLabel)
{
    public static ScheduledStatementOfWorkWindowView ToScheduledStatementOfWorkWindowView(ScheduledStatementOfWorkWindow item)
    {
        return new ScheduledStatementOfWorkWindowView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Country, 120),
            $"{item.Active}",
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledStatementOfWorkWindowView> ToScheduledStatementOfWorkWindowViews(List<ScheduledStatementOfWorkWindow> items)
    {
        var views = new List<ScheduledStatementOfWorkWindowView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledStatementOfWorkWindowView(item));
        }
        return views;
    }

    public static double SumScheduledStatementOfWorkWindowView(ScheduledStatementOfWorkWindowView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
