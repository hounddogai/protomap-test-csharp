namespace Acme.Workforce.Payrollops.Garnishment.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered garnishment target.</summary>
public sealed record TieredGarnishmentTargetView(string Code, string LabelText, string ThresholdText, string JobCode, string Skill, string Amount, string SalaryBand, string ScoreText, string QuantityText, string FloorValue, string TeamText, string StatusLabel)
{
    public static TieredGarnishmentTargetView ToTieredGarnishmentTargetView(TieredGarnishmentTarget item)
    {
        return new TieredGarnishmentTargetView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 120),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            Text.Truncate(item.Skill, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredGarnishmentTargetView> ToTieredGarnishmentTargetViews(List<TieredGarnishmentTarget> items)
    {
        var views = new List<TieredGarnishmentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToTieredGarnishmentTargetView(item));
        }
        return views;
    }

    public static double SumTieredGarnishmentTargetView(TieredGarnishmentTargetView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedAmount + parsedScore;
    }
}
