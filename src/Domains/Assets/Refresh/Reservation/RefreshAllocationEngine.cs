namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh allocation calculation.</summary>
public interface IRefreshAllocationModifier
{
    string Describe();
    double Adjust(RefreshAllocation item, double valueValue);
}

public sealed record RefreshAllocationStartsAtOffsetAdjuster(double Weight) : IRefreshAllocationModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(RefreshAllocation item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record FlaggedRefreshAllocationAdjuster(Registry Flags, string Flag, double Percent) : IRefreshAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRefreshAllocationAdjuster(Dictionary<string, double> Factors) : IRefreshAllocationModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RefreshAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRefreshAllocationAdjuster(List<double> Bounds, List<double> Rates) : IRefreshAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshAllocation item, double valueValue)
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

public sealed record RoundingRefreshAllocationAdjuster(double Step) : IRefreshAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the refresh allocation steps in order and keeps a trace of each.</summary>
public sealed class RefreshAllocationEngine(List<IRefreshAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshAllocationEngine DefaultRefreshAllocationEngine(Registry flags)
    {
        return new RefreshAllocationEngine(
            new List<IRefreshAllocationModifier>
            {
                new RefreshAllocationStartsAtOffsetAdjuster(0.98),
                new FlaggedRefreshAllocationAdjuster(flags, "refresh-allocation-seasonal", 5.7),
                new NameRefreshAllocationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.3, ["online"] = 1.32 }),
                new TieredRefreshAllocationAdjuster(
                    new List<double> { 322.0, 1978.0, 4988.0, 8007.0 },
                    new List<double> { 0.82, 1.01, 0.91, 0.84 }),
                new RoundingRefreshAllocationAdjuster(1.0),
            },
            new List<string>(),
            2.3,
            84102.3);
    }

    public double Compute(RefreshAllocation item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
