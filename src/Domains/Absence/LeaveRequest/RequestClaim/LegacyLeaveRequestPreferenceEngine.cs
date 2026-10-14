namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy leave request preference calculation.</summary>
public interface ILegacyLeaveRequestPreferenceFactor
{
    string Describe();
    double Adjust(LegacyLeaveRequestPreference item, double valueValue);
}

public sealed record FlaggedLegacyLeaveRequestPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ILegacyLeaveRequestPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyLeaveRequestPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLegacyLeaveRequestPreferenceAdjuster(double Percent) : ILegacyLeaveRequestPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyLeaveRequestPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyLeaveRequestPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ILegacyLeaveRequestPreferenceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyLeaveRequestPreference item, double valueValue)
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

public sealed record LegacyLeaveRequestPreferenceWindowDaysOffsetAdjuster(double Weight) : ILegacyLeaveRequestPreferenceFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(LegacyLeaveRequestPreference item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumLegacyLeaveRequestPreferenceAdjuster(double Minimum) : ILegacyLeaveRequestPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyLeaveRequestPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy leave request preference steps in order and keeps a trace of each.</summary>
public sealed class LegacyLeaveRequestPreferenceEngine(List<ILegacyLeaveRequestPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLeaveRequestPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLeaveRequestPreferenceEngine DefaultLegacyLeaveRequestPreferenceEngine(Registry flags)
    {
        return new LegacyLeaveRequestPreferenceEngine(
            new List<ILegacyLeaveRequestPreferenceFactor>
            {
                new FlaggedLegacyLeaveRequestPreferenceAdjuster(
                    flags,
                    "legacy-leave-request-preference-scheduled",
                    8.4),
                new PercentLegacyLeaveRequestPreferenceAdjuster(0.0),
                new TieredLegacyLeaveRequestPreferenceAdjuster(
                    new List<double> { 1686.0, 9762.0 },
                    new List<double> { 0.92, 1.16 }),
                new LegacyLeaveRequestPreferenceWindowDaysOffsetAdjuster(1.37),
                new MinimumLegacyLeaveRequestPreferenceAdjuster(2.2),
            },
            new List<string>(),
            1.2,
            99964.6);
    }

    public double Price(LegacyLeaveRequestPreference item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLeaveRequestPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
