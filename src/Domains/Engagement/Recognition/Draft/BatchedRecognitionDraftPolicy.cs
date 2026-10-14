namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched recognition draft policy.</summary>
public static class BatchedRecognitionDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 47535.0, in order.</summary>
    public static int BatchedRecognitionDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47535.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedRecognitionDraft()
    {
        var policy = Retry.DefaultPolicy("batched-recognition-draft");
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

    /// <summary>Puts a batched recognition draft code in its canonical BRD- form.</summary>
    public static string NormalizeBatchedRecognitionDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BRD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BRD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
