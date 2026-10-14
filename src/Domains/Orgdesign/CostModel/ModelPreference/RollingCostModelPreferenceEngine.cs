namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling cost model preference calculation.</summary>
public interface IRollingCostModelPreferenceStep
{
    string Describe();
    double Adjust(RollingCostModelPreference item, double valueValue);
}

public sealed record MinimumRollingCostModelPreferenceAdjuster(double Minimum) : IRollingCostModelPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingCostModelPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingCostModelPreferenceAdjuster(double Cap) : IRollingCostModelPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingCostModelPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRollingCostModelPreferenceAdjuster(double Step) : IRollingCostModelPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingCostModelPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionRollingCostModelPreferenceAdjuster(Dictionary<string, double> Factors) : IRollingCostModelPreferenceStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(RollingCostModelPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingCostModelPreferenceExpiresAtOffsetAdjuster(double Weight) : IRollingCostModelPreferenceStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(RollingCostModelPreference item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentRollingCostModelPreferenceAdjuster(double Percent) : IRollingCostModelPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingCostModelPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling cost model preference steps in order and keeps a trace of each.</summary>
public sealed class RollingCostModelPreferenceEngine(List<IRollingCostModelPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingCostModelPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingCostModelPreferenceEngine DefaultRollingCostModelPreferenceEngine()
    {
        return new RollingCostModelPreferenceEngine(
            new List<IRollingCostModelPreferenceStep>
            {
                new MinimumRollingCostModelPreferenceAdjuster(42.3),
                new CapRollingCostModelPreferenceAdjuster(1267.0),
                new RoundingRollingCostModelPreferenceAdjuster(0.5),
                new RegionRollingCostModelPreferenceAdjuster(new Dictionary<string, double> { ["central"] = 0.73 }),
                new RollingCostModelPreferenceExpiresAtOffsetAdjuster(0.3),
                new PercentRollingCostModelPreferenceAdjuster(-17.0),
            },
            new List<string>(),
            3.1,
            13171.5);
    }

    public double Evaluate(RollingCostModelPreference item)
    {
        var current = item.WeightedVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RollingCostModelPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
