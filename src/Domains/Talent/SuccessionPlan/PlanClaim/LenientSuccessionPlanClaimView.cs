namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient succession plan claim.</summary>
public sealed record LenientSuccessionPlanClaimView(string Code, string Label, string SourceText, string FloorValueText, string CostCenterText, string ActiveText, string ContactEmail, string ManagerId, string LeaveDays, string Maximum, string StatusLabel)
{
    public static LenientSuccessionPlanClaimView ToLenientSuccessionPlanClaimView(LenientSuccessionPlanClaim item)
    {
        return new LenientSuccessionPlanClaimView(
            item.Code,
            item.Label,
            Text.Truncate(item.Source, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Active}",
            item.ContactEmail,
            item.ManagerId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSuccessionPlanClaimView> ToLenientSuccessionPlanClaimViews(List<LenientSuccessionPlanClaim> items)
    {
        var views = new List<LenientSuccessionPlanClaimView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSuccessionPlanClaimView(item));
        }
        return views;
    }

    public static double SumLenientSuccessionPlanClaimView(LenientSuccessionPlanClaimView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedLeaveDays + parsedMaximum;
    }
}
