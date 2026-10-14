namespace Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>A rule a policy checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPolicyCheckpointRule
{
    List<string> Check(PolicyCheckpoint item);
}

public sealed record PolicyCheckpointRequiredTitleRule(string Message) : IPolicyCheckpointRule
{
    public List<string> Check(PolicyCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyCheckpointRequiredCodeRule(string Message) : IPolicyCheckpointRule
{
    public List<string> Check(PolicyCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyCheckpointVisibleGuardRule(int Limit) : IPolicyCheckpointRule
{
    public List<string> Check(PolicyCheckpoint item)
    {
        if (item.Visible && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PolicyCheckpointCodeLengthRule(int MaxLength) : IPolicyCheckpointRule
{
    public List<string> Check(PolicyCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy checkpoint.</summary>
public sealed record PolicyCheckpointValidator(List<IPolicyCheckpointRule> Rules)
{
    public List<string> Validate(PolicyCheckpoint item)
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

    public void RequireValid(PolicyCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyCheckpoint> items)
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

/// <summary>Functions of policy checkpoint rules.</summary>
public static class PolicyCheckpointRules
{
    public static PolicyCheckpointValidator DefaultPolicyCheckpointValidator()
    {
        return new PolicyCheckpointValidator(
            new List<IPolicyCheckpointRule>
            {
                new PolicyCheckpointRequiredTitleRule("policy checkpoint needs this field"),
                new PolicyCheckpointRequiredCodeRule("policy checkpoint needs an entry"),
                new PolicyCheckpointVisibleGuardRule(16),
                new PolicyCheckpointCodeLengthRule(64),
            });
    }
}
