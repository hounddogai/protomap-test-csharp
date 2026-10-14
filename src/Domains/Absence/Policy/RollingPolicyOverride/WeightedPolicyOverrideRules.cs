namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted policy override must pass. Check returns its problems, or none.</summary>
public interface IWeightedPolicyOverrideRule
{
    List<string> Check(WeightedPolicyOverride item);
}

public sealed record WeightedPolicyOverrideAllowedTimezoneRule(List<string> Allowed) : IWeightedPolicyOverrideRule
{
    public List<string> Check(WeightedPolicyOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyOverrideNameLengthRule(int MaxLength) : IWeightedPolicyOverrideRule
{
    public List<string> Check(WeightedPolicyOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyOverrideVersionRangeRule(int Min, int Max) : IWeightedPolicyOverrideRule
{
    public List<string> Check(WeightedPolicyOverride item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyOverrideRequiredExternalRefRule(string Message) : IWeightedPolicyOverrideRule
{
    public List<string> Check(WeightedPolicyOverride item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyOverrideCodePrefixRule(string Prefix) : IWeightedPolicyOverrideRule
{
    public List<string> Check(WeightedPolicyOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted policy override.</summary>
public sealed record WeightedPolicyOverrideValidator(List<IWeightedPolicyOverrideRule> Rules)
{
    public List<string> Validate(WeightedPolicyOverride item)
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

    public void RequireValid(WeightedPolicyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPolicyOverride> items)
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

/// <summary>Functions of weighted policy override rules.</summary>
public static class WeightedPolicyOverrideRules
{
    public static WeightedPolicyOverrideValidator DefaultWeightedPolicyOverrideValidator()
    {
        return new WeightedPolicyOverrideValidator(
            new List<IWeightedPolicyOverrideRule>
            {
                new WeightedPolicyOverrideAllowedTimezoneRule(new List<string> { "manual", "batched" }),
                new WeightedPolicyOverrideNameLengthRule(255),
                new WeightedPolicyOverrideVersionRangeRule(64, 86400),
                new WeightedPolicyOverrideRequiredExternalRefRule("weighted policy override needs a value"),
                new WeightedPolicyOverrideCodePrefixRule("X"),
            });
    }
}
