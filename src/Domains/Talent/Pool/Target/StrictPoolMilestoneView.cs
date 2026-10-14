namespace Acme.Workforce.Talent.Pool.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pool milestone.</summary>
public sealed record StrictPoolMilestoneView(string Code, string Label, string Badge, string JobCode, string ChannelText, string TitleText, string Amount, string StatusLabel)
{
    public static StrictPoolMilestoneView ToStrictPoolMilestoneView(StrictPoolMilestone item)
    {
        return new StrictPoolMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Badge,
            Text.Truncate(item.JobCode, 64),
            Text.Truncate(item.Channel, 120),
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPoolMilestoneView> ToStrictPoolMilestoneViews(List<StrictPoolMilestone> items)
    {
        var views = new List<StrictPoolMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPoolMilestoneView(item));
        }
        return views;
    }

    public static double SumStrictPoolMilestoneView(StrictPoolMilestoneView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
