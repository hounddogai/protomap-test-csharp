namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered certification checkpoint calculation.</summary>
public interface ITieredCertificationCheckpointFactor
{
    string Describe();
    double Adjust(TieredCertificationCheckpoint item, double valueValue);
}

public sealed record RoundingTieredCertificationCheckpointAdjuster(double Step) : ITieredCertificationCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredCertificationCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCertificationCheckpointUpdatedAtOffsetAdjuster(double Weight) : ITieredCertificationCheckpointFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(TieredCertificationCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record TieredTieredCertificationCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ITieredCertificationCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredCertificationCheckpoint item, double valueValue)
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

public sealed record PercentTieredCertificationCheckpointAdjuster(double Percent) : ITieredCertificationCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredCertificationCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered certification checkpoint steps in order and keeps a trace of each.</summary>
public sealed class TieredCertificationCheckpointEngine(List<ITieredCertificationCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCertificationCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCertificationCheckpointEngine DefaultTieredCertificationCheckpointEngine()
    {
        return new TieredCertificationCheckpointEngine(
            new List<ITieredCertificationCheckpointFactor>
            {
                new RoundingTieredCertificationCheckpointAdjuster(5.0),
                new TieredCertificationCheckpointUpdatedAtOffsetAdjuster(1.31),
                new TieredTieredCertificationCheckpointAdjuster(
                    new List<double> { 629.0, 4121.0, 4416.0 },
                    new List<double> { 1.01, 1.09, 1.07 }),
                new PercentTieredCertificationCheckpointAdjuster(19.3),
            },
            new List<string>(),
            2.0,
            53982.1);
    }

    public double Price(TieredCertificationCheckpoint item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredCertificationCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
