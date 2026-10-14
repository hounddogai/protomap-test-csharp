namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle profile calculation.</summary>
public interface IMeritCycleProfileModifier
{
    string Describe();
    double Adjust(MeritCycleProfile item, double valueValue);
}

public sealed record MinimumMeritCycleProfileAdjuster(double Minimum) : IMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedMeritCycleProfileAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMeritCycleProfileAdjuster(double Step) : IMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MeritCycleProfileExpiresAtOffsetAdjuster(double Weight) : IMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(MeritCycleProfile item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the merit cycle profile steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleProfileEngine(List<IMeritCycleProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleProfileEngine DefaultMeritCycleProfileEngine(Registry flags)
    {
        return new MeritCycleProfileEngine(
            new List<IMeritCycleProfileModifier>
            {
                new MinimumMeritCycleProfileAdjuster(9.4),
                new FlaggedMeritCycleProfileAdjuster(flags, "merit-cycle-profile-pending", 8.5),
                new RoundingMeritCycleProfileAdjuster(0.05),
                new MeritCycleProfileExpiresAtOffsetAdjuster(1.47),
            },
            new List<string>(),
            0.2,
            37402.6);
    }

    public double Compute(MeritCycleProfile item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
