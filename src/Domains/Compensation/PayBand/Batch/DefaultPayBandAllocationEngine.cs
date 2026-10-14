namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pay band allocation calculation.</summary>
public interface IDefaultPayBandAllocationStep
{
    string Describe();
    double Adjust(DefaultPayBandAllocation item, double valueValue);
}

public sealed record CapDefaultPayBandAllocationAdjuster(double Cap) : IDefaultPayBandAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPayBandAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultPayBandAllocationMaximumOffsetAdjuster(double Weight) : IDefaultPayBandAllocationStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(DefaultPayBandAllocation item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record RoundingDefaultPayBandAllocationAdjuster(double Step) : IDefaultPayBandAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPayBandAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultPayBandAllocationAdjuster(double Percent) : IDefaultPayBandAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayBandAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultPayBandAllocationAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPayBandAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPayBandAllocation item, double valueValue)
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

public sealed record CategoryDefaultPayBandAllocationAdjuster(Dictionary<string, double> Factors) : IDefaultPayBandAllocationStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(DefaultPayBandAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default pay band allocation steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayBandAllocationEngine(List<IDefaultPayBandAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayBandAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayBandAllocationEngine DefaultDefaultPayBandAllocationEngine()
    {
        return new DefaultPayBandAllocationEngine(
            new List<IDefaultPayBandAllocationStep>
            {
                new CapDefaultPayBandAllocationAdjuster(44721.0),
                new DefaultPayBandAllocationMaximumOffsetAdjuster(0.69),
                new RoundingDefaultPayBandAllocationAdjuster(0.1),
                new PercentDefaultPayBandAllocationAdjuster(-16.4),
                new TieredDefaultPayBandAllocationAdjuster(
                    new List<double> { 3240.0, 4493.0, 4506.0 },
                    new List<double> { 0.81, 1.01, 0.88 }),
                new CategoryDefaultPayBandAllocationAdjuster(new Dictionary<string, double> { ["north"] = 1.12 }),
            },
            new List<string>(),
            2.6,
            21917.9);
    }

    public double Price(DefaultPayBandAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayBandAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
