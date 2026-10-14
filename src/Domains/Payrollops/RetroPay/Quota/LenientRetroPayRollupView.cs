namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient retro pay rollup.</summary>
public sealed record LenientRetroPayRollupView(string Code, string TitleText, string UpdatedAtText, string Rate, string Retries, string RegionText, string FteText, string Maximum, string LocationText, string BatchSizeText, string Amount, string StatusLabel)
{
    public static LenientRetroPayRollupView ToLenientRetroPayRollupView(LenientRetroPayRollup item)
    {
        return new LenientRetroPayRollupView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientRetroPayRollupView> ToLenientRetroPayRollupViews(List<LenientRetroPayRollup> items)
    {
        var views = new List<LenientRetroPayRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLenientRetroPayRollupView(item));
        }
        return views;
    }

    public static double SumLenientRetroPayRollupView(LenientRetroPayRollupView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedRate + (double)parsedRetries;
    }
}
