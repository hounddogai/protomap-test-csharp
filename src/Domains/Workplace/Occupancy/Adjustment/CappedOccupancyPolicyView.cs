namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped occupancy policy.</summary>
public sealed record CappedOccupancyPolicyView(string Code, string Name, string AttemptsText, string Summary, string Channel, string Reviewer, string StatusLabel)
{
    public static CappedOccupancyPolicyView ToCappedOccupancyPolicyView(CappedOccupancyPolicy item)
    {
        return new CappedOccupancyPolicyView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.Channel,
            Text.Truncate(item.Reviewer, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedOccupancyPolicyView> ToCappedOccupancyPolicyViews(List<CappedOccupancyPolicy> items)
    {
        var views = new List<CappedOccupancyPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToCappedOccupancyPolicyView(item));
        }
        return views;
    }

    public static double SumCappedOccupancyPolicyView(CappedOccupancyPolicyView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
