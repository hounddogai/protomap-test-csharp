namespace Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Functions of lenient pay run request policy.</summary>
public static class LenientPayRunRequestPolicy
{
    /// <summary>Puts a lenient pay run request code in its canonical LPRR- form.</summary>
    public static string NormalizeLenientPayRunRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 32939.0, in order.</summary>
    public static int LenientPayRunRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32939.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
