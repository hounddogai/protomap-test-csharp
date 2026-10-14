namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient laptop exception policy.</summary>
public static class LenientLaptopExceptionPolicy
{
    public static List<string> MergeLenientLaptopExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientLaptopException()
    {
        var policy = Retry.DefaultPolicy("lenient-laptop-exception");
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

    /// <summary>Names the band a lenient laptop exception score falls in.</summary>
    public static string ClassifyLenientLaptopException(double score)
    {
        if (score < 34.5)
        {
            return "watch";
        }
        if (score < 83.4)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Puts a lenient laptop exception code in its canonical LLE- form.</summary>
    public static string NormalizeLenientLaptopExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
