namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>A rule a adaptive merit cycle mapping must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveMeritCycleMappingRule
{
    List<string> Check(AdaptiveMeritCycleMapping item);
}

public sealed record AdaptiveMeritCycleMappingCodePrefixRule(string Prefix) : IAdaptiveMeritCycleMappingRule
{
    public List<string> Check(AdaptiveMeritCycleMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMeritCycleMappingUpdatedAtRangeRule(int Min, int Max) : IAdaptiveMeritCycleMappingRule
{
    public List<string> Check(AdaptiveMeritCycleMapping item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveMeritCycleMappingLocationLengthRule(int MaxLength) : IAdaptiveMeritCycleMappingRule
{
    public List<string> Check(AdaptiveMeritCycleMapping item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMeritCycleMappingThresholdRangeRule(double Min, double Max) : IAdaptiveMeritCycleMappingRule
{
    public List<string> Check(AdaptiveMeritCycleMapping item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMeritCycleMappingNameLengthRule(int MaxLength) : IAdaptiveMeritCycleMappingRule
{
    public List<string> Check(AdaptiveMeritCycleMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveMeritCycleMappingTimezoneLengthRule(int MaxLength) : IAdaptiveMeritCycleMappingRule
{
    public List<string> Check(AdaptiveMeritCycleMapping item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive merit cycle mapping.</summary>
public sealed record AdaptiveMeritCycleMappingValidator(List<IAdaptiveMeritCycleMappingRule> Rules)
{
    public List<string> Validate(AdaptiveMeritCycleMapping item)
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

    public void RequireValid(AdaptiveMeritCycleMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveMeritCycleMapping> items)
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

/// <summary>Functions of adaptive merit cycle mapping rules.</summary>
public static class AdaptiveMeritCycleMappingRules
{
    public static AdaptiveMeritCycleMappingValidator DefaultAdaptiveMeritCycleMappingValidator()
    {
        return new AdaptiveMeritCycleMappingValidator(
            new List<IAdaptiveMeritCycleMappingRule>
            {
                new AdaptiveMeritCycleMappingCodePrefixRule("X"),
                new AdaptiveMeritCycleMappingUpdatedAtRangeRule(120, 3600),
                new AdaptiveMeritCycleMappingLocationLengthRule(255),
                new AdaptiveMeritCycleMappingThresholdRangeRule(3.0, 695.3),
                new AdaptiveMeritCycleMappingNameLengthRule(32),
                new AdaptiveMeritCycleMappingTimezoneLengthRule(32),
            });
    }
}
