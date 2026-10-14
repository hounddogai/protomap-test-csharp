namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool forecast.</summary>
public sealed record BonusPoolForecastView(string Code, string TitleText, string Revision, string SummaryText, string Sequence, string AttemptsText, string Cost, string Maximum, string Threshold, string Discount, string Pinned, string StatusLabel)
{
    public static BonusPoolForecastView ToBonusPoolForecastView(BonusPoolForecast item)
    {
        return new BonusPoolForecastView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolForecastView> ToBonusPoolForecastViews(List<BonusPoolForecast> items)
    {
        var views = new List<BonusPoolForecastView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolForecastView(item));
        }
        return views;
    }

    public static double SumBonusPoolForecastView(BonusPoolForecastView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedSequence + (double)parsedAttempts;
    }
}
