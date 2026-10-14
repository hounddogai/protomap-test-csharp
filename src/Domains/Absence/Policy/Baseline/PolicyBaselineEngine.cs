namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy baseline calculation.</summary>
public interface IPolicyBaselineFactor
{
    string Describe();
    double Adjust(PolicyBaseline item, double valueValue);
}

public sealed record MinimumPolicyBaselineAdjuster(double Minimum) : IPolicyBaselineFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PolicyBaselineDurationMinutesOffsetAdjuster(double Weight) : IPolicyBaselineFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PolicyBaseline item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record PercentPolicyBaselineAdjuster(double Percent) : IPolicyBaselineFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPolicyBaselineAdjuster(double Cap) : IPolicyBaselineFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPolicyBaselineAdjuster(double Step) : IPolicyBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the policy baseline steps in order and keeps a trace of each.</summary>
public sealed class PolicyBaselineEngine(List<IPolicyBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyBaselineEngine DefaultPolicyBaselineEngine()
    {
        return new PolicyBaselineEngine(
            new List<IPolicyBaselineFactor>
            {
                new MinimumPolicyBaselineAdjuster(41.9),
                new PolicyBaselineDurationMinutesOffsetAdjuster(0.48),
                new PercentPolicyBaselineAdjuster(-10.1),
                new CapPolicyBaselineAdjuster(18605.0),
                new RoundingPolicyBaselineAdjuster(0.1),
            },
            new List<string>(),
            0.0,
            18938.5);
    }

    public double Price(PolicyBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
