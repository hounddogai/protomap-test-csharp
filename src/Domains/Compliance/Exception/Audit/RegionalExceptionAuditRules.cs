namespace Acme.Workforce.Compliance.Exception.Audit;

/// <summary>A rule a regional exception audit must pass. Check returns its problems, or none.</summary>
public interface IRegionalExceptionAuditRule
{
    List<string> Check(RegionalExceptionAudit item);
}

public sealed record RegionalExceptionAuditLabelLengthRule(int MaxLength) : IRegionalExceptionAuditRule
{
    public List<string> Check(RegionalExceptionAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionAuditCodePrefixRule(string Prefix) : IRegionalExceptionAuditRule
{
    public List<string> Check(RegionalExceptionAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalExceptionAuditExpiresAtRangeRule(int Min, int Max) : IRegionalExceptionAuditRule
{
    public List<string> Check(RegionalExceptionAudit item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional exception audit.</summary>
public sealed record RegionalExceptionAuditValidator(List<IRegionalExceptionAuditRule> Rules)
{
    public List<string> Validate(RegionalExceptionAudit item)
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

    public void RequireValid(RegionalExceptionAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalExceptionAudit> items)
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

/// <summary>Functions of regional exception audit rules.</summary>
public static class RegionalExceptionAuditRules
{
    public static RegionalExceptionAuditValidator DefaultRegionalExceptionAuditValidator()
    {
        return new RegionalExceptionAuditValidator(
            new List<IRegionalExceptionAuditRule>
            {
                new RegionalExceptionAuditLabelLengthRule(86400),
                new RegionalExceptionAuditCodePrefixRule("Q"),
                new RegionalExceptionAuditExpiresAtRangeRule(80, 120),
            });
    }
}
