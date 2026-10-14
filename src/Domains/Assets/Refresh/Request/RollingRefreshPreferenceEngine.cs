namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling refresh preference calculation.</summary>
public interface IRollingRefreshPreferenceFactor
{
    string Describe();
    double Adjust(RollingRefreshPreference item, double valueValue);
}

public sealed record CapRollingRefreshPreferenceAdjuster(double Cap) : IRollingRefreshPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingRefreshPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RollingRefreshPreferenceDurationMinutesOffsetAdjuster(double Weight) : IRollingRefreshPreferenceFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(RollingRefreshPreference item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record RoundingRollingRefreshPreferenceAdjuster(double Step) : IRollingRefreshPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingRefreshPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling refresh preference steps in order and keeps a trace of each.</summary>
public sealed class RollingRefreshPreferenceEngine(List<IRollingRefreshPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingRefreshPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingRefreshPreferenceEngine DefaultRollingRefreshPreferenceEngine()
    {
        return new RollingRefreshPreferenceEngine(
            new List<IRollingRefreshPreferenceFactor>
            {
                new CapRollingRefreshPreferenceAdjuster(7309.0),
                new RollingRefreshPreferenceDurationMinutesOffsetAdjuster(1.31),
                new RoundingRollingRefreshPreferenceAdjuster(0.25),
            },
            new List<string>(),
            5.0,
            84083.3);
    }

    public double Evaluate(RollingRefreshPreference item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingRefreshPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
