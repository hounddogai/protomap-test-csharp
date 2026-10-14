namespace Acme.Workforce.Talent.Mobility.MobilityAudit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary mobility audit policy.</summary>
public static class PrimaryMobilityAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50704.0, in order.</summary>
    public static int PrimaryMobilityAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50704.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergePrimaryMobilityAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
