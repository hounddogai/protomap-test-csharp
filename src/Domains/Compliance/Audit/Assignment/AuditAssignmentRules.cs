namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit assignment must pass. Check returns its problems, or none.</summary>
public interface IAuditAssignmentRule
{
    List<string> Check(AuditAssignment item);
}

public sealed record AuditAssignmentAllowedCodeRule(List<string> Allowed) : IAuditAssignmentRule
{
    public List<string> Check(AuditAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AuditAssignmentPriorityRangeRule(int Min, int Max) : IAuditAssignmentRule
{
    public List<string> Check(AuditAssignment item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditAssignmentCodePrefixRule(string Prefix) : IAuditAssignmentRule
{
    public List<string> Check(AuditAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditAssignmentRequiredJobCodeRule(string Message) : IAuditAssignmentRule
{
    public List<string> Check(AuditAssignment item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditAssignmentRequiredCodeRule(string Message) : IAuditAssignmentRule
{
    public List<string> Check(AuditAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditAssignmentRequiredLocationRule(string Message) : IAuditAssignmentRule
{
    public List<string> Check(AuditAssignment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit assignment.</summary>
public sealed record AuditAssignmentValidator(List<IAuditAssignmentRule> Rules)
{
    public List<string> Validate(AuditAssignment item)
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

    public void RequireValid(AuditAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditAssignment> items)
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

/// <summary>Functions of audit assignment rules.</summary>
public static class AuditAssignmentRules
{
    public static AuditAssignmentValidator DefaultAuditAssignmentValidator()
    {
        return new AuditAssignmentValidator(
            new List<IAuditAssignmentRule>
            {
                new AuditAssignmentAllowedCodeRule(new List<string> { "fallback", "global", "scheduled" }),
                new AuditAssignmentPriorityRangeRule(80, 120),
                new AuditAssignmentCodePrefixRule("ACM"),
                new AuditAssignmentRequiredJobCodeRule("audit assignment needs this field"),
                new AuditAssignmentRequiredCodeRule("audit assignment needs this field"),
                new AuditAssignmentRequiredLocationRule("audit assignment needs this field"),
            });
    }
}
