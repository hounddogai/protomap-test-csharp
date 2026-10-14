namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a retro pay bundle.</summary>
public sealed record RetroPayBundleView(string Code, string Label, string ActiveText, string ScoreText, string Minimum, string UpdatedAtText, string TenureMonths, string RevisionText, string FloorValue, string StatusLabel)
{
    public static RetroPayBundleView ToRetroPayBundleView(RetroPayBundle item)
    {
        return new RetroPayBundleView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayBundleView> ToRetroPayBundleViews(List<RetroPayBundle> items)
    {
        var views = new List<RetroPayBundleView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayBundleView(item));
        }
        return views;
    }

    public static double SumRetroPayBundleView(RetroPayBundleView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return parsedScore + parsedMinimum + (double)parsedUpdatedAt;
    }
}
