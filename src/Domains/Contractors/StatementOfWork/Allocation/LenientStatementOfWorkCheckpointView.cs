namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient statement of work checkpoint.</summary>
public sealed record LenientStatementOfWorkCheckpointView(string Code, string NameText, string NotesText, string Margin, string Limit, string RateText, string ContactEmail, string Ratio, string VersionText, string StatusLabel)
{
    public static LenientStatementOfWorkCheckpointView ToLenientStatementOfWorkCheckpointView(LenientStatementOfWorkCheckpoint item)
    {
        return new LenientStatementOfWorkCheckpointView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Notes,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientStatementOfWorkCheckpointView> ToLenientStatementOfWorkCheckpointViews(List<LenientStatementOfWorkCheckpoint> items)
    {
        var views = new List<LenientStatementOfWorkCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToLenientStatementOfWorkCheckpointView(item));
        }
        return views;
    }

    public static double SumLenientStatementOfWorkCheckpointView(LenientStatementOfWorkCheckpointView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedLimit + parsedRate;
    }
}
