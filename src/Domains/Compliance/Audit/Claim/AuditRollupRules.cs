namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit rollup must pass. Check returns its problems, or none.</summary>
public interface IAuditRollupRule
{
    List<string> Check(AuditRollup item);
}

public sealed record AuditRollupCodeLengthRule(int MaxLength) : IAuditRollupRule
{
    public List<string> Check(AuditRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditRollupCategoryLengthRule(int MaxLength) : IAuditRollupRule
{
    public List<string> Check(AuditRollup item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditRollupMinimumRangeRule(double Min, double Max) : IAuditRollupRule
{
    public List<string> Check(AuditRollup item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditRollupAllowedTitleRule(List<string> Allowed) : IAuditRollupRule
{
    public List<string> Check(AuditRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit rollup.</summary>
public sealed record AuditRollupValidator(List<IAuditRollupRule> Rules)
{
    public List<string> Validate(AuditRollup item)
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

    public void RequireValid(AuditRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditRollup> items)
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

/// <summary>Functions of audit rollup rules.</summary>
public static class AuditRollupRules
{
    public static AuditRollupValidator DefaultAuditRollupValidator()
    {
        return new AuditRollupValidator(
            new List<IAuditRollupRule>
            {
                new AuditRollupCodeLengthRule(86400),
                new AuditRollupCategoryLengthRule(64),
                new AuditRollupMinimumRangeRule(0.3, 2987.8),
                new AuditRollupAllowedTitleRule(new List<string>()),
            });
    }
}
