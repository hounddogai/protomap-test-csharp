namespace Acme.Workforce.Compliance.PolicyAck.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary policy ack plan.</summary>
public sealed record PrimaryPolicyAckPlanView(string Code, string LabelText, string Badge, string Minimum, string Team, string RatioText, string Approved, string TitleText, string StatusLabel)
{
    public static PrimaryPolicyAckPlanView ToPrimaryPolicyAckPlanView(PrimaryPolicyAckPlan item)
    {
        return new PrimaryPolicyAckPlanView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Badge,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Title,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryPolicyAckPlanView> ToPrimaryPolicyAckPlanViews(List<PrimaryPolicyAckPlan> items)
    {
        var views = new List<PrimaryPolicyAckPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryPolicyAckPlanView(item));
        }
        return views;
    }

    public static double SumPrimaryPolicyAckPlanView(PrimaryPolicyAckPlanView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedRatio;
    }
}
