namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global headcount checkpoint calculation.</summary>
public interface IGlobalHeadcountCheckpointModifier
{
    string Describe();
    double Adjust(GlobalHeadcountCheckpoint item, double valueValue);
}

public sealed record TitleGlobalHeadcountCheckpointAdjuster(Dictionary<string, double> Factors) : IGlobalHeadcountCheckpointModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalHeadcountCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredGlobalHeadcountCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IGlobalHeadcountCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalHeadcountCheckpoint item, double valueValue)
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

public sealed record PercentGlobalHeadcountCheckpointAdjuster(double Percent) : IGlobalHeadcountCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalHeadcountCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGlobalHeadcountCheckpointAdjuster(double Cap) : IGlobalHeadcountCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalHeadcountCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGlobalHeadcountCheckpointAdjuster(double Step) : IGlobalHeadcountCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalHeadcountCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global headcount checkpoint steps in order and keeps a trace of each.</summary>
public sealed class GlobalHeadcountCheckpointEngine(List<IGlobalHeadcountCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalHeadcountCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalHeadcountCheckpointEngine DefaultGlobalHeadcountCheckpointEngine()
    {
        return new GlobalHeadcountCheckpointEngine(
            new List<IGlobalHeadcountCheckpointModifier>
            {
                new TitleGlobalHeadcountCheckpointAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.99, ["north"] = 0.82, ["east"] = 0.82 }),
                new TieredGlobalHeadcountCheckpointAdjuster(
                    new List<double> { 7212.0, 9462.0 },
                    new List<double> { 1.18, 1.17 }),
                new PercentGlobalHeadcountCheckpointAdjuster(22.6),
                new CapGlobalHeadcountCheckpointAdjuster(41429.0),
                new RoundingGlobalHeadcountCheckpointAdjuster(5.0),
            },
            new List<string>(),
            3.2,
            74524.6);
    }

    public double Compute(GlobalHeadcountCheckpoint item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalHeadcountCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
