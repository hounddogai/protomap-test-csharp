namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the visitor slot calculation.</summary>
public interface IVisitorSlotModifier
{
    string Describe();
    double Adjust(VisitorSlot item, double valueValue);
}

public sealed record FlaggedVisitorSlotAdjuster(Registry Flags, string Flag, double Percent) : IVisitorSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVisitorSlotAdjuster(double Cap) : IVisitorSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelVisitorSlotAdjuster(Dictionary<string, double> Factors) : IVisitorSlotModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(VisitorSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredVisitorSlotAdjuster(List<double> Bounds, List<double> Rates) : IVisitorSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorSlot item, double valueValue)
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

public sealed record PercentVisitorSlotAdjuster(double Percent) : IVisitorSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumVisitorSlotAdjuster(double Minimum) : IVisitorSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the visitor slot steps in order and keeps a trace of each.</summary>
public sealed class VisitorSlotEngine(List<IVisitorSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorSlotEngine DefaultVisitorSlotEngine(Registry flags)
    {
        return new VisitorSlotEngine(
            new List<IVisitorSlotModifier>
            {
                new FlaggedVisitorSlotAdjuster(flags, "visitor-slot-lenient", -9.4),
                new CapVisitorSlotAdjuster(45384.0),
                new LabelVisitorSlotAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["online"] = 0.98, ["retail"] = 1.37 }),
                new TieredVisitorSlotAdjuster(
                    new List<double> { 3228.0, 3764.0, 6087.0, 9558.0 },
                    new List<double> { 1.08, 0.98, 0.94, 0.84 }),
                new PercentVisitorSlotAdjuster(5.1),
                new MinimumVisitorSlotAdjuster(49.7),
            },
            new List<string>(),
            4.2,
            73552.6);
    }

    public double Evaluate(VisitorSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<VisitorSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
