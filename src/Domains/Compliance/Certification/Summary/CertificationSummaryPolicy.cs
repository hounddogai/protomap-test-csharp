namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification summary policy.</summary>
public static class CertificationSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72751.0, in order.</summary>
    public static int CertificationSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72751.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string CertificationSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
