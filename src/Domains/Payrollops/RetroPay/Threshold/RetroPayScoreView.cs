namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay score.</summary>
public sealed record RetroPayScoreView(string Code, string LabelText, string Capacity, string BadgeText, string JobCodeText, string Team, string Limit, string StatusLabel)
{
    public static RetroPayScoreView ToRetroPayScoreView(RetroPayScore item)
    {
        return new RetroPayScoreView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.JobCode, 120),
            item.Team,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayScoreView> ToRetroPayScoreViews(List<RetroPayScore> items)
    {
        var views = new List<RetroPayScoreView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayScoreView(item));
        }
        return views;
    }

    public static double SumRetroPayScoreView(RetroPayScoreView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedLimit;
    }
}
