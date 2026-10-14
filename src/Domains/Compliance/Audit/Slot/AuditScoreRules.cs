namespace Acme.Workforce.Compliance.Audit.Slot;

/// <summary>A rule a audit score must pass. Check returns its problems, or none.</summary>
public interface IAuditScoreRule
{
    List<string> Check(AuditScore item);
}

public sealed record AuditScoreRequiredExternalRefRule(string Message) : IAuditScoreRule
{
    public List<string> Check(AuditScore item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditScoreCodePrefixRule(string Prefix) : IAuditScoreRule
{
    public List<string> Check(AuditScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditScoreCostRangeRule(double Min, double Max) : IAuditScoreRule
{
    public List<string> Check(AuditScore item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AuditScoreRequiredNameRule(string Message) : IAuditScoreRule
{
    public List<string> Check(AuditScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditScoreTagsLimitRule(int MaxCount, List<string> Banned) : IAuditScoreRule
{
    public List<string> Check(AuditScore item)
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

public sealed record AuditScoreNameLengthRule(int MaxLength) : IAuditScoreRule
{
    public List<string> Check(AuditScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit score.</summary>
public sealed record AuditScoreValidator(List<IAuditScoreRule> Rules)
{
    public List<string> Validate(AuditScore item)
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

    public void RequireValid(AuditScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditScore> items)
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

/// <summary>Functions of audit score rules.</summary>
public static class AuditScoreRules
{
    public static AuditScoreValidator DefaultAuditScoreValidator()
    {
        return new AuditScoreValidator(
            new List<IAuditScoreRule>
            {
                new AuditScoreRequiredExternalRefRule("audit score needs a value"),
                new AuditScoreCodePrefixRule("X"),
                new AuditScoreCostRangeRule(4.3, 4073.3),
                new AuditScoreRequiredNameRule("audit score needs a value"),
                new AuditScoreTagsLimitRule(32, new List<string> { "global", "tiered", "global" }),
                new AuditScoreNameLengthRule(120),
            });
    }
}
