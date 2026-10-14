namespace Acme.Workforce.Engagement.TownHall.Target;

/// <summary>One step of the pending town hall baseline calculation.</summary>
public interface IPendingTownHallBaselineModifier
{
    string Describe();
    double Adjust(PendingTownHallBaseline item, double valueValue);
}

public sealed record TieredPendingTownHallBaselineAdjuster(List<double> Bounds, List<double> Rates) : IPendingTownHallBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingTownHallBaseline item, double valueValue)
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

public sealed record MinimumPendingTownHallBaselineAdjuster(double Minimum) : IPendingTownHallBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingTownHallBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPendingTownHallBaselineAdjuster(double Cap) : IPendingTownHallBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingTownHallBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingTownHallBaselineMarginOffsetAdjuster(double Weight) : IPendingTownHallBaselineModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(PendingTownHallBaseline item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record PercentPendingTownHallBaselineAdjuster(double Percent) : IPendingTownHallBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingTownHallBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending town hall baseline steps in order and keeps a trace of each.</summary>
public sealed class PendingTownHallBaselineEngine(List<IPendingTownHallBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingTownHallBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingTownHallBaselineEngine DefaultPendingTownHallBaselineEngine()
    {
        return new PendingTownHallBaselineEngine(
            new List<IPendingTownHallBaselineModifier>
            {
                new TieredPendingTownHallBaselineAdjuster(
                    new List<double> { 5651.0, 6156.0, 6500.0, 9136.0 },
                    new List<double> { 0.99, 0.96, 0.92, 1.11 }),
                new MinimumPendingTownHallBaselineAdjuster(3.3),
                new CapPendingTownHallBaselineAdjuster(23004.0),
                new PendingTownHallBaselineMarginOffsetAdjuster(1.98),
                new PercentPendingTownHallBaselineAdjuster(-15.3),
            },
            new List<string>(),
            2.9,
            29885.8);
    }

    public double Compute(PendingTownHallBaseline item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingTownHallBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
