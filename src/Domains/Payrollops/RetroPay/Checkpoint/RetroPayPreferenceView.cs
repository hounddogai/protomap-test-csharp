namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay preference.</summary>
public sealed record RetroPayPreferenceView(string Code, string Name, string Fte, string Maximum, string Label, string Archived, string CostCenterText, string ScoreText, string BatchSize, string StatusLabel)
{
    public static RetroPayPreferenceView ToRetroPayPreferenceView(RetroPayPreference item)
    {
        return new RetroPayPreferenceView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Archived}",
            Text.Truncate(item.CostCenter, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayPreferenceView> ToRetroPayPreferenceViews(List<RetroPayPreference> items)
    {
        var views = new List<RetroPayPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayPreferenceView(item));
        }
        return views;
    }

    public static double SumRetroPayPreferenceView(RetroPayPreferenceView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedFte + parsedMaximum + parsedScore;
    }
}
