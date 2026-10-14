namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending desk entry calculation.</summary>
public interface IPendingDeskEntryModifier
{
    string Describe();
    double Adjust(PendingDeskEntry item, double valueValue);
}

public sealed record TieredPendingDeskEntryAdjuster(List<double> Bounds, List<double> Rates) : IPendingDeskEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingDeskEntry item, double valueValue)
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

public sealed record CapPendingDeskEntryAdjuster(double Cap) : IPendingDeskEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingDeskEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPendingDeskEntryAdjuster(double Minimum) : IPendingDeskEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingDeskEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPendingDeskEntryAdjuster(double Step) : IPendingDeskEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingDeskEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PendingDeskEntryScoreOffsetAdjuster(double Weight) : IPendingDeskEntryModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PendingDeskEntry item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentPendingDeskEntryAdjuster(double Percent) : IPendingDeskEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingDeskEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending desk entry steps in order and keeps a trace of each.</summary>
public sealed class PendingDeskEntryEngine(List<IPendingDeskEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingDeskEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingDeskEntryEngine DefaultPendingDeskEntryEngine()
    {
        return new PendingDeskEntryEngine(
            new List<IPendingDeskEntryModifier>
            {
                new TieredPendingDeskEntryAdjuster(
                    new List<double> { 5557.0, 5781.0, 8451.0 },
                    new List<double> { 0.92, 1.05, 0.86 }),
                new CapPendingDeskEntryAdjuster(33244.0),
                new MinimumPendingDeskEntryAdjuster(13.5),
                new RoundingPendingDeskEntryAdjuster(0.5),
                new PendingDeskEntryScoreOffsetAdjuster(0.85),
                new PercentPendingDeskEntryAdjuster(-20.7),
            },
            new List<string>(),
            2.9,
            25209.6);
    }

    public double Compute(PendingDeskEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingDeskEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
