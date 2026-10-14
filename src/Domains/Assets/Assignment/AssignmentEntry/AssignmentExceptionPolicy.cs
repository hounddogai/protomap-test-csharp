namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of assignment exception policy.</summary>
public static class AssignmentExceptionPolicy
{
    /// <summary>Puts a assignment exception code in its canonical AE- form.</summary>
    public static string NormalizeAssignmentExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 34762.0, in order.</summary>
    public static int AssignmentExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 34762.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AssignmentExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
