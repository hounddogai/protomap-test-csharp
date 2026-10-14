namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tax form window policy.</summary>
public static class TaxFormWindowPolicy
{
    /// <summary>Puts a tax form window code in its canonical TFW- form.</summary>
    public static string NormalizeTaxFormWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTaxFormWindow()
    {
        var policy = Retry.DefaultPolicy("tax-form-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
