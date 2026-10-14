namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped recognition template policy.</summary>
public static class CappedRecognitionTemplatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedRecognitionTemplate()
    {
        var policy = Retry.DefaultPolicy("capped-recognition-template");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int CappedRecognitionTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a capped recognition template code in its canonical CRT- form.</summary>
    public static string NormalizeCappedRecognitionTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CRT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CRT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 71371.0, in order.</summary>
    public static int CappedRecognitionTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71371.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
