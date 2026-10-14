namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>A rule a retro pay threshold must pass. Check returns its problems, or none.</summary>
public interface IRetroPayThresholdRule
{
    List<string> Check(RetroPayThreshold item);
}

public sealed record RetroPayThresholdCostRangeRule(double Min, double Max) : IRetroPayThresholdRule
{
    public List<string> Check(RetroPayThreshold item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayThresholdCodePrefixRule(string Prefix) : IRetroPayThresholdRule
{
    public List<string> Check(RetroPayThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayThresholdCodeLengthRule(int MaxLength) : IRetroPayThresholdRule
{
    public List<string> Check(RetroPayThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay threshold.</summary>
public sealed record RetroPayThresholdValidator(List<IRetroPayThresholdRule> Rules)
{
    public List<string> Validate(RetroPayThreshold item)
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

    public void RequireValid(RetroPayThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayThreshold> items)
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

/// <summary>Functions of retro pay threshold rules.</summary>
public static class RetroPayThresholdRules
{
    public static RetroPayThresholdValidator DefaultRetroPayThresholdValidator()
    {
        return new RetroPayThresholdValidator(
            new List<IRetroPayThresholdRule>
            {
                new RetroPayThresholdCostRangeRule(8.0, 4272.7),
                new RetroPayThresholdCodePrefixRule("OPS"),
                new RetroPayThresholdCodeLengthRule(64),
            });
    }
}
