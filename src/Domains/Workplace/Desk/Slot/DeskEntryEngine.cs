namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk entry calculation.</summary>
public interface IDeskEntryModifier
{
    string Describe();
    double Adjust(DeskEntry item, double valueValue);
}

public sealed record FlaggedDeskEntryAdjuster(Registry Flags, string Flag, double Percent) : IDeskEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDeskEntryAdjuster(double Step) : IDeskEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDeskEntryAdjuster(double Percent) : IDeskEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeskEntryAdjuster(double Cap) : IDeskEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the desk entry steps in order and keeps a trace of each.</summary>
public sealed class DeskEntryEngine(List<IDeskEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskEntryEngine DefaultDeskEntryEngine(Registry flags)
    {
        return new DeskEntryEngine(
            new List<IDeskEntryModifier>
            {
                new FlaggedDeskEntryAdjuster(flags, "desk-entry-manual", -7.6),
                new RoundingDeskEntryAdjuster(0.5),
                new PercentDeskEntryAdjuster(13.0),
                new CapDeskEntryAdjuster(37182.0),
            },
            new List<string>(),
            0.9,
            74626.1);
    }

    public double Price(DeskEntry item)
    {
        var current = item.WeightedVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<DeskEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
