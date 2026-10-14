namespace Acme.Workforce.Absence.Policy.Score;

/// <summary>A rule a policy score must pass. Check returns its problems, or none.</summary>
public interface IPolicyScoreRule
{
    List<string> Check(PolicyScore item);
}

public sealed record PolicyScoreCodePrefixRule(string Prefix) : IPolicyScoreRule
{
    public List<string> Check(PolicyScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyScoreRequiredDescriptionRule(string Message) : IPolicyScoreRule
{
    public List<string> Check(PolicyScore item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyScoreDescriptionLengthRule(int MaxLength) : IPolicyScoreRule
{
    public List<string> Check(PolicyScore item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyScoreChannelLengthRule(int MaxLength) : IPolicyScoreRule
{
    public List<string> Check(PolicyScore item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy score.</summary>
public sealed record PolicyScoreValidator(List<IPolicyScoreRule> Rules)
{
    public List<string> Validate(PolicyScore item)
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

    public void RequireValid(PolicyScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyScore> items)
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

/// <summary>Functions of policy score rules.</summary>
public static class PolicyScoreRules
{
    public static PolicyScoreValidator DefaultPolicyScoreValidator()
    {
        return new PolicyScoreValidator(
            new List<IPolicyScoreRule>
            {
                new PolicyScoreCodePrefixRule("ACM"),
                new PolicyScoreRequiredDescriptionRule("policy score needs a value"),
                new PolicyScoreDescriptionLengthRule(3600),
                new PolicyScoreChannelLengthRule(32),
            });
    }
}
