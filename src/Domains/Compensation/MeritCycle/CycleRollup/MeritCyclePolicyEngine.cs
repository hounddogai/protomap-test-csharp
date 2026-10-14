namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle policy calculation.</summary>
public interface IMeritCyclePolicyFactor
{
    string Describe();
    double Adjust(MeritCyclePolicy item, double valueValue);
}

public sealed record CapMeritCyclePolicyAdjuster(double Cap) : IMeritCyclePolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCyclePolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredMeritCyclePolicyAdjuster(List<double> Bounds, List<double> Rates) : IMeritCyclePolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCyclePolicy item, double valueValue)
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

public sealed record MeritCyclePolicyDurationMinutesOffsetAdjuster(double Weight) : IMeritCyclePolicyFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(MeritCyclePolicy item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record RoundingMeritCyclePolicyAdjuster(double Step) : IMeritCyclePolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCyclePolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentMeritCyclePolicyAdjuster(double Percent) : IMeritCyclePolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCyclePolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the merit cycle policy steps in order and keeps a trace of each.</summary>
public sealed class MeritCyclePolicyEngine(List<IMeritCyclePolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCyclePolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCyclePolicyEngine DefaultMeritCyclePolicyEngine()
    {
        return new MeritCyclePolicyEngine(
            new List<IMeritCyclePolicyFactor>
            {
                new CapMeritCyclePolicyAdjuster(40067.0),
                new TieredMeritCyclePolicyAdjuster(
                    new List<double> { 2041.0, 3812.0, 4982.0, 7514.0 },
                    new List<double> { 0.98, 1.16, 1.13, 0.93 }),
                new MeritCyclePolicyDurationMinutesOffsetAdjuster(0.83),
                new RoundingMeritCyclePolicyAdjuster(0.05),
                new PercentMeritCyclePolicyAdjuster(2.9),
            },
            new List<string>(),
            4.1,
            93304.5);
    }

    public double Compute(MeritCyclePolicy item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCyclePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
