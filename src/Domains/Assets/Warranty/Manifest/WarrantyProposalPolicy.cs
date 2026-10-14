namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty proposal policy.</summary>
public static class WarrantyProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 80805.0, in order.</summary>
    public static int WarrantyProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80805.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double WarrantyProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
