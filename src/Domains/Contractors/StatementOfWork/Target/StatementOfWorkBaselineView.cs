namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work baseline.</summary>
public sealed record StatementOfWorkBaselineView(string Code, string Label, string StartsAt, string GraceDaysText, string ExternalRefText, string EndsAtText, string FteText, string Ceiling, string Hours, string SequenceText, string StatusLabel)
{
    public static StatementOfWorkBaselineView ToStatementOfWorkBaselineView(StatementOfWorkBaseline item)
    {
        return new StatementOfWorkBaselineView(
            item.Code,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 120),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkBaselineView> ToStatementOfWorkBaselineViews(List<StatementOfWorkBaseline> items)
    {
        var views = new List<StatementOfWorkBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkBaselineView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkBaselineView(StatementOfWorkBaselineView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedGraceDays + (double)parsedEndsAt;
    }
}
