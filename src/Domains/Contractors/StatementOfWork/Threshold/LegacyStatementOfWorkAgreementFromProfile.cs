namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Functions of legacy statement of work agreement from profile.</summary>
public static class LegacyStatementOfWorkAgreementFromProfile
{
    public static Dictionary<string, double> SummarizePrimaryEngagementProfilesForAgreement(List<PrimaryEngagementProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<LegacyStatementOfWorkAgreement> DeriveLegacyStatementOfWorkAgreementsFromProfile(List<PrimaryEngagementProfile> items)
    {
        var outValue = new List<LegacyStatementOfWorkAgreement>();
        foreach (var item in items)
        {
            var derived = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateLegacyStatementOfWorkAgreement(LegacyStatementOfWorkAgreement item)
    {
        return PrimaryEngagementProfilePolicy.ClassifyPrimaryEngagementProfile(item.EffectiveTotal());
    }
}
