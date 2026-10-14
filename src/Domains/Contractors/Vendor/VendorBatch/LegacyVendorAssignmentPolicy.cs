namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy vendor assignment policy.</summary>
public static class LegacyVendorAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 47374.0, in order.</summary>
    public static int LegacyVendorAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47374.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLegacyVendorAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
