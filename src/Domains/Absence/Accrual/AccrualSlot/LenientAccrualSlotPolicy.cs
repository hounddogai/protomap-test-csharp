namespace Acme.Workforce.Absence.Accrual.AccrualSlot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient accrual slot policy.</summary>
public static class LenientAccrualSlotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 94770.0, in order.</summary>
    public static int LenientAccrualSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 94770.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientAccrualSlot()
    {
        var policy = Retry.DefaultPolicy("lenient-accrual-slot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeLenientAccrualSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a lenient accrual slot code in its canonical LAS- form.</summary>
    public static string NormalizeLenientAccrualSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
