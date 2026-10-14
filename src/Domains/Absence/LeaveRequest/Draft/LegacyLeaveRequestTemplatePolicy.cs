namespace Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Functions of legacy leave request template policy.</summary>
public static class LegacyLeaveRequestTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 25479.0, in order.</summary>
    public static int LegacyLeaveRequestTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25479.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a legacy leave request template code in its canonical LLRT- form.</summary>
    public static string NormalizeLegacyLeaveRequestTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLRT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLRT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
