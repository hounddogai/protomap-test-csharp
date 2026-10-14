namespace Acme.Workforce.Talent.CareerPath.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path snapshot calculation.</summary>
public interface ICareerPathSnapshotModifier
{
    string Describe();
    double Adjust(CareerPathSnapshot item, double valueValue);
}

public sealed record CapCareerPathSnapshotAdjuster(double Cap) : ICareerPathSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCareerPathSnapshotAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCareerPathSnapshotAdjuster(double Step) : ICareerPathSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the career path snapshot steps in order and keeps a trace of each.</summary>
public sealed class CareerPathSnapshotEngine(List<ICareerPathSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathSnapshotEngine DefaultCareerPathSnapshotEngine(Registry flags)
    {
        return new CareerPathSnapshotEngine(
            new List<ICareerPathSnapshotModifier>
            {
                new CapCareerPathSnapshotAdjuster(3100.0),
                new FlaggedCareerPathSnapshotAdjuster(flags, "career-path-snapshot-primary", 6.0),
                new RoundingCareerPathSnapshotAdjuster(0.1),
            },
            new List<string>(),
            2.0,
            80391.9);
    }

    public double Compute(CareerPathSnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
