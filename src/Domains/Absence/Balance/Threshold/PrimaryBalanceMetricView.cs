namespace Acme.Workforce.Absence.Balance.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a primary balance metric.</summary>
public sealed record PrimaryBalanceMetricView(string Code, string Title, string TeamText, string Locale, string PinnedText, string NotesText, string Currency, string ManagerId, string ArchivedText, string Amount, string StatusLabel)
{
    public static PrimaryBalanceMetricView ToPrimaryBalanceMetricView(PrimaryBalanceMetric item)
    {
        return new PrimaryBalanceMetricView(
            item.Code,
            item.Title,
            item.Team,
            item.Locale,
            $"{item.Pinned}",
            item.Notes,
            item.Currency,
            item.ManagerId,
            $"{item.Archived}",
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryBalanceMetricView> ToPrimaryBalanceMetricViews(List<PrimaryBalanceMetric> items)
    {
        var views = new List<PrimaryBalanceMetricView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryBalanceMetricView(item));
        }
        return views;
    }

    public static double SumPrimaryBalanceMetricView(PrimaryBalanceMetricView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
