namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling policy override must pass. Check returns its problems, or none.</summary>
public interface IRollingPolicyOverrideRule
{
    List<string> Check(RollingPolicyOverride item);
}

public sealed record RollingPolicyOverrideCountryLengthRule(int MaxLength) : IRollingPolicyOverrideRule
{
    public List<string> Check(RollingPolicyOverride item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyOverrideAllowedExternalRefRule(List<string> Allowed) : IRollingPolicyOverrideRule
{
    public List<string> Check(RollingPolicyOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyOverrideWindowRule(int MaxSpan) : IRollingPolicyOverrideRule
{
    public List<string> Check(RollingPolicyOverride item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyOverrideNameLengthRule(int MaxLength) : IRollingPolicyOverrideRule
{
    public List<string> Check(RollingPolicyOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyOverrideStartsAtRangeRule(int Min, int Max) : IRollingPolicyOverrideRule
{
    public List<string> Check(RollingPolicyOverride item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling policy override.</summary>
public sealed record RollingPolicyOverrideValidator(List<IRollingPolicyOverrideRule> Rules)
{
    public List<string> Validate(RollingPolicyOverride item)
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

    public void RequireValid(RollingPolicyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPolicyOverride> items)
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

/// <summary>Functions of rolling policy override rules.</summary>
public static class RollingPolicyOverrideRules
{
    public static RollingPolicyOverrideValidator DefaultRollingPolicyOverrideValidator()
    {
        return new RollingPolicyOverrideValidator(
            new List<IRollingPolicyOverrideRule>
            {
                new RollingPolicyOverrideCountryLengthRule(86400),
                new RollingPolicyOverrideAllowedExternalRefRule(new List<string> { "manual" }),
                new RollingPolicyOverrideWindowRule(64),
                new RollingPolicyOverrideNameLengthRule(120),
                new RollingPolicyOverrideStartsAtRangeRule(32, 32),
            });
    }
}
