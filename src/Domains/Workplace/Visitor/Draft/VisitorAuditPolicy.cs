namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of visitor audit policy.</summary>
public static class VisitorAuditPolicy
{
    public static double VisitorAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a visitor audit code in its canonical VA- form.</summary>
    public static string NormalizeVisitorAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a visitor audit score falls in.</summary>
    public static string ClassifyVisitorAudit(double score)
    {
        if (score < 40.7)
        {
            return "ok";
        }
        if (score < 43.7)
        {
            return "high";
        }
        if (score < 51.8)
        {
            return "critical";
        }
        if (score < 69.2)
        {
            return "watch";
        }
        return "medium";
    }
}
