namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>A rule a adaptive retro pay checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveRetroPayCheckpointRule
{
    List<string> Check(AdaptiveRetroPayCheckpoint item);
}

public sealed record AdaptiveRetroPayCheckpointThresholdRangeRule(double Min, double Max) : IAdaptiveRetroPayCheckpointRule
{
    public List<string> Check(AdaptiveRetroPayCheckpoint item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayCheckpointRequiredManagerIdRule(string Message) : IAdaptiveRetroPayCheckpointRule
{
    public List<string> Check(AdaptiveRetroPayCheckpoint item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayCheckpointRequiredCodeRule(string Message) : IAdaptiveRetroPayCheckpointRule
{
    public List<string> Check(AdaptiveRetroPayCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayCheckpointCodePrefixRule(string Prefix) : IAdaptiveRetroPayCheckpointRule
{
    public List<string> Check(AdaptiveRetroPayCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayCheckpointNameLengthRule(int MaxLength) : IAdaptiveRetroPayCheckpointRule
{
    public List<string> Check(AdaptiveRetroPayCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveRetroPayCheckpointCostRangeRule(double Min, double Max) : IAdaptiveRetroPayCheckpointRule
{
    public List<string> Check(AdaptiveRetroPayCheckpoint item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive retro pay checkpoint.</summary>
public sealed record AdaptiveRetroPayCheckpointValidator(List<IAdaptiveRetroPayCheckpointRule> Rules)
{
    public List<string> Validate(AdaptiveRetroPayCheckpoint item)
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

    public void RequireValid(AdaptiveRetroPayCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveRetroPayCheckpoint> items)
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

/// <summary>Functions of adaptive retro pay checkpoint rules.</summary>
public static class AdaptiveRetroPayCheckpointRules
{
    public static AdaptiveRetroPayCheckpointValidator DefaultAdaptiveRetroPayCheckpointValidator()
    {
        return new AdaptiveRetroPayCheckpointValidator(
            new List<IAdaptiveRetroPayCheckpointRule>
            {
                new AdaptiveRetroPayCheckpointThresholdRangeRule(1.4, 1207.1),
                new AdaptiveRetroPayCheckpointRequiredManagerIdRule("adaptive retro pay checkpoint needs a value"),
                new AdaptiveRetroPayCheckpointRequiredCodeRule("adaptive retro pay checkpoint needs a value"),
                new AdaptiveRetroPayCheckpointCodePrefixRule("REF"),
                new AdaptiveRetroPayCheckpointNameLengthRule(86400),
                new AdaptiveRetroPayCheckpointCostRangeRule(9.1, 3464.9),
            });
    }
}
