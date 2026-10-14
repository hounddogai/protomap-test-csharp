namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback statement of work checkpoint.</summary>
public sealed record FallbackStatementOfWorkCheckpointView(string CodeText, string TitleText, string ExpiresAt, string GraceDays, string Unit, string Archived, string Score, string Pinned, string Approved, string StatusLabel)
{
    public static FallbackStatementOfWorkCheckpointView ToFallbackStatementOfWorkCheckpointView(FallbackStatementOfWorkCheckpoint item)
    {
        return new FallbackStatementOfWorkCheckpointView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            $"{item.Archived}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackStatementOfWorkCheckpointView> ToFallbackStatementOfWorkCheckpointViews(List<FallbackStatementOfWorkCheckpoint> items)
    {
        var views = new List<FallbackStatementOfWorkCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackStatementOfWorkCheckpointView(item));
        }
        return views;
    }

    public static double SumFallbackStatementOfWorkCheckpointView(FallbackStatementOfWorkCheckpointView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedGraceDays + parsedScore;
    }
}
