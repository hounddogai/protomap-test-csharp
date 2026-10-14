namespace Acme.Workforce.Contractors.StatementOfWork.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient statement of work slot policy.</summary>
public static class LenientStatementOfWorkSlotPolicy
{
    /// <summary>Names the band a lenient statement of work slot score falls in.</summary>
    public static string ClassifyLenientStatementOfWorkSlot(double score)
    {
        if (score < 10.9)
        {
            return "elevated";
        }
        if (score < 21.0)
        {
            return "medium";
        }
        return "ok";
    }

    public static List<string> MergeLenientStatementOfWorkSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string LenientStatementOfWorkSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 12434.0, in order.</summary>
    public static int LenientStatementOfWorkSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12434.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
