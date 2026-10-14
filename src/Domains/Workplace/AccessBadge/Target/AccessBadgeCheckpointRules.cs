namespace Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>A rule a access badge checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeCheckpointRule
{
    List<string> Check(AccessBadgeCheckpoint item);
}

public sealed record AccessBadgeCheckpointCodePrefixRule(string Prefix) : IAccessBadgeCheckpointRule
{
    public List<string> Check(AccessBadgeCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeCheckpointNameLengthRule(int MaxLength) : IAccessBadgeCheckpointRule
{
    public List<string> Check(AccessBadgeCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeCheckpointRequiredChannelRule(string Message) : IAccessBadgeCheckpointRule
{
    public List<string> Check(AccessBadgeCheckpoint item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge checkpoint.</summary>
public sealed record AccessBadgeCheckpointValidator(List<IAccessBadgeCheckpointRule> Rules)
{
    public List<string> Validate(AccessBadgeCheckpoint item)
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

    public void RequireValid(AccessBadgeCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeCheckpoint> items)
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

/// <summary>Functions of access badge checkpoint rules.</summary>
public static class AccessBadgeCheckpointRules
{
    public static AccessBadgeCheckpointValidator DefaultAccessBadgeCheckpointValidator()
    {
        return new AccessBadgeCheckpointValidator(
            new List<IAccessBadgeCheckpointRule>
            {
                new AccessBadgeCheckpointCodePrefixRule("X"),
                new AccessBadgeCheckpointNameLengthRule(32),
                new AccessBadgeCheckpointRequiredChannelRule("access badge checkpoint needs a value"),
            });
    }
}
