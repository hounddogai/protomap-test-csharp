namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>One step of the primary reorg baseline calculation.</summary>
public interface IPrimaryReorgBaselineStep
{
    string Describe();
    double Adjust(PrimaryReorgBaseline item, double valueValue);
}

public sealed record TieredPrimaryReorgBaselineAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryReorgBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryReorgBaseline item, double valueValue)
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

public sealed record PrimaryReorgBaselineRetriesOffsetAdjuster(double Weight) : IPrimaryReorgBaselineStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PrimaryReorgBaseline item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumPrimaryReorgBaselineAdjuster(double Minimum) : IPrimaryReorgBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryReorgBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPrimaryReorgBaselineAdjuster(double Cap) : IPrimaryReorgBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryReorgBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary reorg baseline steps in order and keeps a trace of each.</summary>
public sealed class PrimaryReorgBaselineEngine(List<IPrimaryReorgBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryReorgBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryReorgBaselineEngine DefaultPrimaryReorgBaselineEngine()
    {
        return new PrimaryReorgBaselineEngine(
            new List<IPrimaryReorgBaselineStep>
            {
                new TieredPrimaryReorgBaselineAdjuster(
                    new List<double> { 821.0, 4502.0 },
                    new List<double> { 0.98, 0.87 }),
                new PrimaryReorgBaselineRetriesOffsetAdjuster(0.21),
                new MinimumPrimaryReorgBaselineAdjuster(11.7),
                new CapPrimaryReorgBaselineAdjuster(3023.0),
            },
            new List<string>(),
            4.1,
            35710.5);
    }

    public double Evaluate(PrimaryReorgBaseline item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryReorgBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
