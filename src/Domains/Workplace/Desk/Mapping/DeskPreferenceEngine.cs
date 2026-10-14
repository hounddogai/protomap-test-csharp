namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk preference calculation.</summary>
public interface IDeskPreferenceStep
{
    string Describe();
    double Adjust(DeskPreference item, double valueValue);
}

public sealed record FlaggedDeskPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IDeskPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeskPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IDeskPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskPreference item, double valueValue)
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

public sealed record MinimumDeskPreferenceAdjuster(double Minimum) : IDeskPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DeskPreferenceCapacityOffsetAdjuster(double Weight) : IDeskPreferenceStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(DeskPreference item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record CapDeskPreferenceAdjuster(double Cap) : IDeskPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the desk preference steps in order and keeps a trace of each.</summary>
public sealed class DeskPreferenceEngine(List<IDeskPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskPreferenceEngine DefaultDeskPreferenceEngine(Registry flags)
    {
        return new DeskPreferenceEngine(
            new List<IDeskPreferenceStep>
            {
                new FlaggedDeskPreferenceAdjuster(flags, "desk-preference-batched", 5.2),
                new TieredDeskPreferenceAdjuster(
                    new List<double> { 3483.0, 7790.0 },
                    new List<double> { 1.04, 1.09 }),
                new MinimumDeskPreferenceAdjuster(3.5),
                new DeskPreferenceCapacityOffsetAdjuster(0.29),
                new CapDeskPreferenceAdjuster(43642.0),
            },
            new List<string>(),
            3.0,
            89742.0);
    }

    public double Compute(DeskPreference item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DeskPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
