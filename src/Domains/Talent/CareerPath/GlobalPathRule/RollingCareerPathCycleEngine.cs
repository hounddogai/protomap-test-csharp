namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling career path cycle calculation.</summary>
public interface IRollingCareerPathCycleModifier
{
    string Describe();
    double Adjust(RollingCareerPathCycle item, double valueValue);
}

public sealed record TieredRollingCareerPathCycleAdjuster(List<double> Bounds, List<double> Rates) : IRollingCareerPathCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingCareerPathCycle item, double valueValue)
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

public sealed record NameRollingCareerPathCycleAdjuster(Dictionary<string, double> Factors) : IRollingCareerPathCycleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingCareerPathCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRollingCareerPathCycleAdjuster(double Step) : IRollingCareerPathCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingCareerPathCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRollingCareerPathCycleAdjuster(double Minimum) : IRollingCareerPathCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingCareerPathCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingCareerPathCycleAdjuster(double Cap) : IRollingCareerPathCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingCareerPathCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling career path cycle steps in order and keeps a trace of each.</summary>
public sealed class RollingCareerPathCycleEngine(List<IRollingCareerPathCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingCareerPathCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingCareerPathCycleEngine DefaultRollingCareerPathCycleEngine()
    {
        return new RollingCareerPathCycleEngine(
            new List<IRollingCareerPathCycleModifier>
            {
                new TieredRollingCareerPathCycleAdjuster(
                    new List<double> { 2451.0, 4513.0, 6963.0, 8870.0 },
                    new List<double> { 1.2, 1.06, 0.93, 1.16 }),
                new NameRollingCareerPathCycleAdjuster(new Dictionary<string, double> { ["retail"] = 1.31 }),
                new RoundingRollingCareerPathCycleAdjuster(0.25),
                new MinimumRollingCareerPathCycleAdjuster(35.0),
                new CapRollingCareerPathCycleAdjuster(47628.0),
            },
            new List<string>(),
            2.1,
            50230.9);
    }

    public double Compute(RollingCareerPathCycle item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingCareerPathCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
