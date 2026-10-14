namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted bonus pool variance.</summary>
public sealed record WeightedBonusPoolVarianceView(string CodeText, string TitleText, string Hours, string GraceDays, string Minimum, string Checksum, string StatusLabel)
{
    public static WeightedBonusPoolVarianceView ToWeightedBonusPoolVarianceView(WeightedBonusPoolVariance item)
    {
        return new WeightedBonusPoolVarianceView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBonusPoolVarianceView> ToWeightedBonusPoolVarianceViews(List<WeightedBonusPoolVariance> items)
    {
        var views = new List<WeightedBonusPoolVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBonusPoolVarianceView(item));
        }
        return views;
    }

    public static double SumWeightedBonusPoolVarianceView(WeightedBonusPoolVarianceView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedGraceDays + parsedMinimum;
    }
}
