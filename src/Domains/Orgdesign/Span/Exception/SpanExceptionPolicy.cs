namespace Acme.Workforce.Orgdesign.Span.Exception;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of span exception policy.</summary>
public static class SpanExceptionPolicy
{
    /// <summary>Puts a span exception code in its canonical SE- form.</summary>
    public static string NormalizeSpanExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SpanExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSpanException()
    {
        var policy = Retry.DefaultPolicy("span-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 50556.0, in order.</summary>
    public static int SpanExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50556.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
