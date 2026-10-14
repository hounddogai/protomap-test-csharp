namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle plan.</summary>
public sealed record MeritCyclePlanView(string CodeText, string LabelText, string Capacity, string Reviewer, string Attempts, string Description, string Currency, string Unit, string StatusLabel)
{
    public static MeritCyclePlanView ToMeritCyclePlanView(MeritCyclePlan item)
    {
        return new MeritCyclePlanView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 40),
            Text.Truncate(item.Currency, 64),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCyclePlanView> ToMeritCyclePlanViews(List<MeritCyclePlan> items)
    {
        var views = new List<MeritCyclePlanView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCyclePlanView(item));
        }
        return views;
    }

    public static double SumMeritCyclePlanView(MeritCyclePlanView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedAttempts;
    }
}
