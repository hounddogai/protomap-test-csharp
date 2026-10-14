namespace Acme.Workforce.Engagement.ActionPlan.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback action plan request.</summary>
public sealed record FallbackActionPlanRequestView(string Code, string Name, string Attempts, string Currency, string Grade, string Label, string ChannelText, string GraceDays, string TeamText, string Cost, string StatusLabel)
{
    public static FallbackActionPlanRequestView ToFallbackActionPlanRequestView(FallbackActionPlanRequest item)
    {
        return new FallbackActionPlanRequestView(
            item.Code,
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            item.Grade,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Channel, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackActionPlanRequestView> ToFallbackActionPlanRequestViews(List<FallbackActionPlanRequest> items)
    {
        var views = new List<FallbackActionPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackActionPlanRequestView(item));
        }
        return views;
    }

    public static double SumFallbackActionPlanRequestView(FallbackActionPlanRequestView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedGraceDays + parsedCost;
    }
}
