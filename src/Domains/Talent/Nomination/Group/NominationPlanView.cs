namespace Acme.Workforce.Talent.Nomination.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination plan.</summary>
public sealed record NominationPlanView(string Code, string Label, string TeamText, string NameText, string Channel, string Hours, string Rate, string Region, string StatusLabel)
{
    public static NominationPlanView ToNominationPlanView(NominationPlan item)
    {
        return new NominationPlanView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Team,
            item.Name,
            item.Channel,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationPlanView> ToNominationPlanViews(List<NominationPlan> items)
    {
        var views = new List<NominationPlanView>();
        foreach (var item in items)
        {
            views.Add(ToNominationPlanView(item));
        }
        return views;
    }

    public static double SumNominationPlanView(NominationPlanView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedHours + parsedRate;
    }
}
