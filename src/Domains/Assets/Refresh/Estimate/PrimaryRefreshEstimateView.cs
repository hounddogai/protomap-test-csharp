namespace Acme.Workforce.Assets.Refresh.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary refresh estimate.</summary>
public sealed record PrimaryRefreshEstimateView(string CodeText, string Label, string Badge, string Score, string Amount, string Fte, string BatchSize, string EndsAtText, string Visible, string StatusLabel)
{
    public static PrimaryRefreshEstimateView ToPrimaryRefreshEstimateView(PrimaryRefreshEstimate item)
    {
        return new PrimaryRefreshEstimateView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 120),
            item.Badge,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryRefreshEstimateView> ToPrimaryRefreshEstimateViews(List<PrimaryRefreshEstimate> items)
    {
        var views = new List<PrimaryRefreshEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryRefreshEstimateView(item));
        }
        return views;
    }

    public static double SumPrimaryRefreshEstimateView(PrimaryRefreshEstimateView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedScore + parsedAmount + parsedFte;
    }
}
