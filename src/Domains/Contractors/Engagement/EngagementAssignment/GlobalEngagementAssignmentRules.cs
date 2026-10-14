namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>A rule a global engagement assignment must pass. Check returns its problems, or none.</summary>
public interface IGlobalEngagementAssignmentRule
{
    List<string> Check(GlobalEngagementAssignment item);
}

public sealed record GlobalEngagementAssignmentCodePrefixRule(string Prefix) : IGlobalEngagementAssignmentRule
{
    public List<string> Check(GlobalEngagementAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEngagementAssignmentTitleLengthRule(int MaxLength) : IGlobalEngagementAssignmentRule
{
    public List<string> Check(GlobalEngagementAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalEngagementAssignmentRequiredTitleRule(string Message) : IGlobalEngagementAssignmentRule
{
    public List<string> Check(GlobalEngagementAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global engagement assignment.</summary>
public sealed record GlobalEngagementAssignmentValidator(List<IGlobalEngagementAssignmentRule> Rules)
{
    public List<string> Validate(GlobalEngagementAssignment item)
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

    public void RequireValid(GlobalEngagementAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalEngagementAssignment> items)
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

/// <summary>Functions of global engagement assignment rules.</summary>
public static class GlobalEngagementAssignmentRules
{
    public static GlobalEngagementAssignmentValidator DefaultGlobalEngagementAssignmentValidator()
    {
        return new GlobalEngagementAssignmentValidator(
            new List<IGlobalEngagementAssignmentRule>
            {
                new GlobalEngagementAssignmentCodePrefixRule("Q"),
                new GlobalEngagementAssignmentTitleLengthRule(16),
                new GlobalEngagementAssignmentRequiredTitleRule("global engagement assignment needs a value"),
            });
    }
}
