namespace Acme.Workforce.Absence.Balance.Quota;

/// <summary>One step of the global balance policy calculation.</summary>
public interface IGlobalBalancePolicyFactor
{
    string Describe();
    double Adjust(GlobalBalancePolicy item, double valueValue);
}

public sealed record CapGlobalBalancePolicyAdjuster(double Cap) : IGlobalBalancePolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalBalancePolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelGlobalBalancePolicyAdjuster(Dictionary<string, double> Factors) : IGlobalBalancePolicyFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GlobalBalancePolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalBalancePolicyAdjuster(double Minimum) : IGlobalBalancePolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalBalancePolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalBalancePolicyAdjuster(List<double> Bounds, List<double> Rates) : IGlobalBalancePolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalBalancePolicy item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record PercentGlobalBalancePolicyAdjuster(double Percent) : IGlobalBalancePolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalBalancePolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global balance policy steps in order and keeps a trace of each.</summary>
public sealed class GlobalBalancePolicyEngine(List<IGlobalBalancePolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalBalancePolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalBalancePolicyEngine DefaultGlobalBalancePolicyEngine()
    {
        return new GlobalBalancePolicyEngine(
            new List<IGlobalBalancePolicyFactor>
            {
                new CapGlobalBalancePolicyAdjuster(46495.0),
                new LabelGlobalBalancePolicyAdjuster(new Dictionary<string, double> { ["east"] = 1.28 }),
                new MinimumGlobalBalancePolicyAdjuster(3.3),
                new TieredGlobalBalancePolicyAdjuster(
                    new List<double> { 1523.0, 9095.0 },
                    new List<double> { 1.04, 0.98 }),
                new PercentGlobalBalancePolicyAdjuster(-21.1),
            },
            new List<string>(),
            2.2,
            95925.4);
    }

    public double Price(GlobalBalancePolicy item)
    {
        var current = item.AdjustedVolume();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<GlobalBalancePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
