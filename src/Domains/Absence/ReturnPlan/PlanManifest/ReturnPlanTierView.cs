namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using System.Globalization;

/// <summary>How screens and exports show a return plan tier.</summary>
public sealed record ReturnPlanTierView(string Code, string Label, string RevisionText, string Attempts, string Checksum, string Amount, string StatusLabel)
{
    public static ReturnPlanTierView ToReturnPlanTierView(ReturnPlanTier item)
    {
        return new ReturnPlanTierView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ReturnPlanTierView> ToReturnPlanTierViews(List<ReturnPlanTier> items)
    {
        var views = new List<ReturnPlanTierView>();
        foreach (var item in items)
        {
            views.Add(ToReturnPlanTierView(item));
        }
        return views;
    }

    public static double SumReturnPlanTierView(ReturnPlanTierView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedAttempts + parsedAmount;
    }
}
