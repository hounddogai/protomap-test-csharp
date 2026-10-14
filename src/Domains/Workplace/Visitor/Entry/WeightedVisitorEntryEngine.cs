namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted visitor entry calculation.</summary>
public interface IWeightedVisitorEntryFactor
{
    string Describe();
    double Adjust(WeightedVisitorEntry item, double valueValue);
}

public sealed record RoundingWeightedVisitorEntryAdjuster(double Step) : IWeightedVisitorEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedVisitorEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedVisitorEntryWindowDaysOffsetAdjuster(double Weight) : IWeightedVisitorEntryFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(WeightedVisitorEntry item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record FlaggedWeightedVisitorEntryAdjuster(Registry Flags, string Flag, double Percent) : IWeightedVisitorEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedVisitorEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentWeightedVisitorEntryAdjuster(double Percent) : IWeightedVisitorEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedVisitorEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedVisitorEntryAdjuster(double Cap) : IWeightedVisitorEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedVisitorEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted visitor entry steps in order and keeps a trace of each.</summary>
public sealed class WeightedVisitorEntryEngine(List<IWeightedVisitorEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedVisitorEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedVisitorEntryEngine DefaultWeightedVisitorEntryEngine(Registry flags)
    {
        return new WeightedVisitorEntryEngine(
            new List<IWeightedVisitorEntryFactor>
            {
                new RoundingWeightedVisitorEntryAdjuster(5.0),
                new WeightedVisitorEntryWindowDaysOffsetAdjuster(1.45),
                new FlaggedWeightedVisitorEntryAdjuster(flags, "weighted-visitor-entry-lenient", -9.8),
                new PercentWeightedVisitorEntryAdjuster(-3.7),
                new CapWeightedVisitorEntryAdjuster(41802.0),
            },
            new List<string>(),
            2.9,
            51204.4);
    }

    public double Run(WeightedVisitorEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedVisitorEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
