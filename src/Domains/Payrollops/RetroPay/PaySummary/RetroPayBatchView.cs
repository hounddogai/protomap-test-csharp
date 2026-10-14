namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using System.Globalization;

/// <summary>How screens and exports show a retro pay batch.</summary>
public sealed record RetroPayBatchView(string Code, string Name, string Version, string Reviewer, string ScoreText, string Threshold, string StatusLabel)
{
    public static RetroPayBatchView ToRetroPayBatchView(RetroPayBatch item)
    {
        return new RetroPayBatchView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayBatchView> ToRetroPayBatchViews(List<RetroPayBatch> items)
    {
        var views = new List<RetroPayBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayBatchView(item));
        }
        return views;
    }

    public static double SumRetroPayBatchView(RetroPayBatchView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedScore + parsedThreshold;
    }
}
