namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of exception template policy.</summary>
public static class ExceptionTemplatePolicy
{
    /// <summary>Puts a exception template code in its canonical ET- form.</summary>
    public static string NormalizeExceptionTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ET-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ET-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ExceptionTemplateSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeExceptionTemplate()
    {
        var policy = Retry.DefaultPolicy("exception-template");
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
}
