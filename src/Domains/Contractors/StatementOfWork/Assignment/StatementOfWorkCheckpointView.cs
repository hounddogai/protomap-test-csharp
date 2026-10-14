namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work checkpoint.</summary>
public sealed record StatementOfWorkCheckpointView(string Code, string TitleText, string Maximum, string Badge, string StartsAt, string StatusLabel)
{
    public static StatementOfWorkCheckpointView ToStatementOfWorkCheckpointView(StatementOfWorkCheckpoint item)
    {
        return new StatementOfWorkCheckpointView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkCheckpointView> ToStatementOfWorkCheckpointViews(List<StatementOfWorkCheckpoint> items)
    {
        var views = new List<StatementOfWorkCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkCheckpointView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkCheckpointView(StatementOfWorkCheckpointView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedStartsAt;
    }
}
