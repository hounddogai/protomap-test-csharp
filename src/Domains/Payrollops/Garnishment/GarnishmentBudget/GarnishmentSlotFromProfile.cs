namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Functions of garnishment slot from profile.</summary>
public static class GarnishmentSlotFromProfile
{
    public static Dictionary<string, double> SummarizePrimaryDeductionProfilesForSlot(List<PrimaryDeductionProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<GarnishmentSlot> DeriveGarnishmentSlotsFromProfile(List<PrimaryDeductionProfile> items)
    {
        var outValue = new List<GarnishmentSlot>();
        foreach (var item in items)
        {
            var derived = GarnishmentSlot.NewGarnishmentSlot(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchGarnishmentSlot(GarnishmentSlot item)
    {
        return LegacyDeductionScorePolicy.LegacyDeductionScoreBucket(item.WeightedVolume());
    }
}
