namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>A rule a succession plan audit must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanAuditRule
{
    List<string> Check(SuccessionPlanAudit item);
}

public sealed record SuccessionPlanAuditSegmentLengthRule(int MaxLength) : ISuccessionPlanAuditRule
{
    public List<string> Check(SuccessionPlanAudit item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAuditCodePrefixRule(string Prefix) : ISuccessionPlanAuditRule
{
    public List<string> Check(SuccessionPlanAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAuditGradeLengthRule(int MaxLength) : ISuccessionPlanAuditRule
{
    public List<string> Check(SuccessionPlanAudit item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan audit.</summary>
public sealed record SuccessionPlanAuditValidator(List<ISuccessionPlanAuditRule> Rules)
{
    public List<string> Validate(SuccessionPlanAudit item)
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

    public void RequireValid(SuccessionPlanAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanAudit> items)
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

/// <summary>Functions of succession plan audit rules.</summary>
public static class SuccessionPlanAuditRules
{
    public static SuccessionPlanAuditValidator DefaultSuccessionPlanAuditValidator()
    {
        return new SuccessionPlanAuditValidator(
            new List<ISuccessionPlanAuditRule>
            {
                new SuccessionPlanAuditSegmentLengthRule(255),
                new SuccessionPlanAuditCodePrefixRule("ACM"),
                new SuccessionPlanAuditGradeLengthRule(64),
            });
    }
}
