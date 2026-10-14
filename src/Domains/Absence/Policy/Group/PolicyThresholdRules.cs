namespace Acme.Workforce.Absence.Policy.Group;

/// <summary>A rule a policy threshold must pass. Check returns its problems, or none.</summary>
public interface IPolicyThresholdRule
{
    List<string> Check(PolicyThreshold item);
}

public sealed record PolicyThresholdCodePrefixRule(string Prefix) : IPolicyThresholdRule
{
    public List<string> Check(PolicyThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyThresholdRequiredNameRule(string Message) : IPolicyThresholdRule
{
    public List<string> Check(PolicyThreshold item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyThresholdActiveGuardRule(double Limit) : IPolicyThresholdRule
{
    public List<string> Check(PolicyThreshold item)
    {
        if (item.Active && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PolicyThresholdUnitLengthRule(int MaxLength) : IPolicyThresholdRule
{
    public List<string> Check(PolicyThreshold item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy threshold.</summary>
public sealed record PolicyThresholdValidator(List<IPolicyThresholdRule> Rules)
{
    public List<string> Validate(PolicyThreshold item)
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

    public void RequireValid(PolicyThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyThreshold> items)
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

/// <summary>Functions of policy threshold rules.</summary>
public static class PolicyThresholdRules
{
    public static PolicyThresholdValidator DefaultPolicyThresholdValidator()
    {
        return new PolicyThresholdValidator(
            new List<IPolicyThresholdRule>
            {
                new PolicyThresholdCodePrefixRule("OPS"),
                new PolicyThresholdRequiredNameRule("policy threshold needs a value"),
                new PolicyThresholdActiveGuardRule(7.6),
                new PolicyThresholdUnitLengthRule(16),
            });
    }
}
