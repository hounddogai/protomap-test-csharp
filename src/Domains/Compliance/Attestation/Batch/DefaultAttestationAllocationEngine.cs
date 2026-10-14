namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default attestation allocation calculation.</summary>
public interface IDefaultAttestationAllocationStep
{
    string Describe();
    double Adjust(DefaultAttestationAllocation item, double valueValue);
}

public sealed record FlaggedDefaultAttestationAllocationAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAttestationAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAttestationAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleDefaultAttestationAllocationAdjuster(Dictionary<string, double> Factors) : IDefaultAttestationAllocationStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultAttestationAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultAttestationAllocationAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAttestationAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAttestationAllocation item, double valueValue)
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

public sealed record PercentDefaultAttestationAllocationAdjuster(double Percent) : IDefaultAttestationAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAttestationAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultAttestationAllocationAdjuster(double Cap) : IDefaultAttestationAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAttestationAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultAttestationAllocationGraceDaysOffsetAdjuster(double Weight) : IDefaultAttestationAllocationStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(DefaultAttestationAllocation item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the default attestation allocation steps in order and keeps a trace of each.</summary>
public sealed class DefaultAttestationAllocationEngine(List<IDefaultAttestationAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAttestationAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAttestationAllocationEngine DefaultDefaultAttestationAllocationEngine(Registry flags)
    {
        return new DefaultAttestationAllocationEngine(
            new List<IDefaultAttestationAllocationStep>
            {
                new FlaggedDefaultAttestationAllocationAdjuster(
                    flags,
                    "default-attestation-allocation-regional",
                    9.8),
                new TitleDefaultAttestationAllocationAdjuster(new Dictionary<string, double> { ["south"] = 1.01 }),
                new TieredDefaultAttestationAllocationAdjuster(
                    new List<double> { 2334.0, 4115.0, 9471.0, 9918.0 },
                    new List<double> { 0.81, 1.04, 0.86, 0.84 }),
                new PercentDefaultAttestationAllocationAdjuster(10.2),
                new CapDefaultAttestationAllocationAdjuster(2630.0),
                new DefaultAttestationAllocationGraceDaysOffsetAdjuster(0.56),
            },
            new List<string>(),
            2.7,
            99126.9);
    }

    public double Compute(DefaultAttestationAllocation item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAttestationAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
