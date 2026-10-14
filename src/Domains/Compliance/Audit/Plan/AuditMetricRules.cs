namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit metric must pass. Check returns its problems, or none.</summary>
public interface IAuditMetricRule
{
    List<string> Check(AuditMetric item);
}

public sealed record AuditMetricAllowedCodeRule(List<string> Allowed) : IAuditMetricRule
{
    public List<string> Check(AuditMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AuditMetricTagsLimitRule(int MaxCount, List<string> Banned) : IAuditMetricRule
{
    public List<string> Check(AuditMetric item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record AuditMetricCodePrefixRule(string Prefix) : IAuditMetricRule
{
    public List<string> Check(AuditMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditMetricRequiredLabelRule(string Message) : IAuditMetricRule
{
    public List<string> Check(AuditMetric item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditMetricRequiredCodeRule(string Message) : IAuditMetricRule
{
    public List<string> Check(AuditMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditMetricRequiredNameRule(string Message) : IAuditMetricRule
{
    public List<string> Check(AuditMetric item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit metric.</summary>
public sealed record AuditMetricValidator(List<IAuditMetricRule> Rules)
{
    public List<string> Validate(AuditMetric item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(AuditMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditMetric> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of audit metric rules.</summary>
public static class AuditMetricRules
{
    public static AuditMetricValidator DefaultAuditMetricValidator()
    {
        return new AuditMetricValidator(
            new List<IAuditMetricRule>
            {
                new AuditMetricAllowedCodeRule(new List<string> { "lenient", "seasonal" }),
                new AuditMetricTagsLimitRule(16, new List<string> { "capped", "seasonal" }),
                new AuditMetricCodePrefixRule("X"),
                new AuditMetricRequiredLabelRule("audit metric needs a value"),
                new AuditMetricRequiredCodeRule("audit metric needs this field"),
                new AuditMetricRequiredNameRule("audit metric needs a value"),
            });
    }
}
