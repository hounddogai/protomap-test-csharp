namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a statement of work estimate.</summary>
public sealed record StatementOfWorkEstimateView(string Code, string Label, string ScoreText, string Checksum, string Attempts, string Description, string StartsAt, string Ceiling, string OwnerIdText, string StatusLabel)
{
    public static StatementOfWorkEstimateView ToStatementOfWorkEstimateView(StatementOfWorkEstimate item)
    {
        return new StatementOfWorkEstimateView(
            item.Code,
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Description,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StatementOfWorkEstimateView> ToStatementOfWorkEstimateViews(List<StatementOfWorkEstimate> items)
    {
        var views = new List<StatementOfWorkEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToStatementOfWorkEstimateView(item));
        }
        return views;
    }

    public static double SumStatementOfWorkEstimateView(StatementOfWorkEstimateView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedAttempts + (double)parsedStartsAt;
    }
}
