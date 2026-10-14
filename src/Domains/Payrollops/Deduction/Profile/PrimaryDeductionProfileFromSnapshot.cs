namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Functions of primary deduction profile from snapshot.</summary>
public static class PrimaryDeductionProfileFromSnapshot
{
    public static Dictionary<string, double> SummarizeLegacyRetroPaySnapshotsForProfile(List<LegacyRetroPaySnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<PrimaryDeductionProfile> DerivePrimaryDeductionProfilesFromSnapshot(List<LegacyRetroPaySnapshot> items)
    {
        var outValue = new List<PrimaryDeductionProfile>();
        foreach (var item in items)
        {
            var derived = PrimaryDeductionProfile.NewPrimaryDeductionProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePrimaryDeductionProfile(PrimaryDeductionProfile item)
    {
        return WeightedRetroPayNoticePolicy.WeightedRetroPayNoticeBucket(item.ProjectedLoad());
    }
}
