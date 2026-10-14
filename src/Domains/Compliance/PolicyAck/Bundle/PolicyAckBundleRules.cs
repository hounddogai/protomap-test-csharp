namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack bundle must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckBundleRule
{
    List<string> Check(PolicyAckBundle item);
}

public sealed record PolicyAckBundleDescriptionLengthRule(int MaxLength) : IPolicyAckBundleRule
{
    public List<string> Check(PolicyAckBundle item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBundleCodeLengthRule(int MaxLength) : IPolicyAckBundleRule
{
    public List<string> Check(PolicyAckBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBundleLocaleLengthRule(int MaxLength) : IPolicyAckBundleRule
{
    public List<string> Check(PolicyAckBundle item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBundleDurationMinutesRangeRule(int Min, int Max) : IPolicyAckBundleRule
{
    public List<string> Check(PolicyAckBundle item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckBundleAllowedDescriptionRule(List<string> Allowed) : IPolicyAckBundleRule
{
    public List<string> Check(PolicyAckBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBundleCodePrefixRule(string Prefix) : IPolicyAckBundleRule
{
    public List<string> Check(PolicyAckBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack bundle.</summary>
public sealed record PolicyAckBundleValidator(List<IPolicyAckBundleRule> Rules)
{
    public List<string> Validate(PolicyAckBundle item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(PolicyAckBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckBundle> items)
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

/// <summary>Functions of policy ack bundle rules.</summary>
public static class PolicyAckBundleRules
{
    public static PolicyAckBundleValidator DefaultPolicyAckBundleValidator()
    {
        return new PolicyAckBundleValidator(
            new List<IPolicyAckBundleRule>
            {
                new PolicyAckBundleDescriptionLengthRule(1000),
                new PolicyAckBundleCodeLengthRule(86400),
                new PolicyAckBundleLocaleLengthRule(64),
                new PolicyAckBundleDurationMinutesRangeRule(3600, 86400),
                new PolicyAckBundleAllowedDescriptionRule(new List<string> { "batched", "default", "batched" }),
                new PolicyAckBundleCodePrefixRule("X"),
            });
    }
}
