namespace Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>A rule a offboarding threshold must pass. Check returns its problems, or none.</summary>
public interface IOffboardingThresholdRule
{
    List<string> Check(OffboardingThreshold item);
}

public sealed record OffboardingThresholdQuantityRangeRule(int Min, int Max) : IOffboardingThresholdRule
{
    public List<string> Check(OffboardingThreshold item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingThresholdDurationMinutesRangeRule(int Min, int Max) : IOffboardingThresholdRule
{
    public List<string> Check(OffboardingThreshold item)
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

public sealed record OffboardingThresholdCodePrefixRule(string Prefix) : IOffboardingThresholdRule
{
    public List<string> Check(OffboardingThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingThresholdCodeLengthRule(int MaxLength) : IOffboardingThresholdRule
{
    public List<string> Check(OffboardingThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding threshold.</summary>
public sealed record OffboardingThresholdValidator(List<IOffboardingThresholdRule> Rules)
{
    public List<string> Validate(OffboardingThreshold item)
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

    public void RequireValid(OffboardingThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingThreshold> items)
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

/// <summary>Functions of offboarding threshold rules.</summary>
public static class OffboardingThresholdRules
{
    public static OffboardingThresholdValidator DefaultOffboardingThresholdValidator()
    {
        return new OffboardingThresholdValidator(
            new List<IOffboardingThresholdRule>
            {
                new OffboardingThresholdQuantityRangeRule(32, 80),
                new OffboardingThresholdDurationMinutesRangeRule(64, 1000),
                new OffboardingThresholdCodePrefixRule("OPS"),
                new OffboardingThresholdCodeLengthRule(80),
            });
    }
}
