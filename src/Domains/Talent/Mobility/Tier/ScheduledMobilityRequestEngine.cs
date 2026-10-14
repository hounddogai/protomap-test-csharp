namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled mobility request calculation.</summary>
public interface IScheduledMobilityRequestModifier
{
    string Describe();
    double Adjust(ScheduledMobilityRequest item, double valueValue);
}

public sealed record RoundingScheduledMobilityRequestAdjuster(double Step) : IScheduledMobilityRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledMobilityRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledMobilityRequestAdjuster(Registry Flags, string Flag, double Percent) : IScheduledMobilityRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledMobilityRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledMobilityRequestAdjuster(double Minimum) : IScheduledMobilityRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledMobilityRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledMobilityRequestAdjuster(List<double> Bounds, List<double> Rates) : IScheduledMobilityRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledMobilityRequest item, double valueValue)
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

/// <summary>Runs the scheduled mobility request steps in order and keeps a trace of each.</summary>
public sealed class ScheduledMobilityRequestEngine(List<IScheduledMobilityRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledMobilityRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledMobilityRequestEngine DefaultScheduledMobilityRequestEngine(Registry flags)
    {
        return new ScheduledMobilityRequestEngine(
            new List<IScheduledMobilityRequestModifier>
            {
                new RoundingScheduledMobilityRequestAdjuster(1.0),
                new FlaggedScheduledMobilityRequestAdjuster(flags, "scheduled-mobility-request-lenient", -0.4),
                new MinimumScheduledMobilityRequestAdjuster(9.4),
                new TieredScheduledMobilityRequestAdjuster(
                    new List<double> { 972.0, 1548.0 },
                    new List<double> { 1.16, 0.93 }),
            },
            new List<string>(),
            2.7,
            53982.6);
    }

    public double Compute(ScheduledMobilityRequest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledMobilityRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
