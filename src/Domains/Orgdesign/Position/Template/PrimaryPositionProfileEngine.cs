namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary position profile calculation.</summary>
public interface IPrimaryPositionProfileStep
{
    string Describe();
    double Adjust(PrimaryPositionProfile item, double valueValue);
}

public sealed record RoundingPrimaryPositionProfileAdjuster(double Step) : IPrimaryPositionProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryPositionProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPrimaryPositionProfileAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPositionProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPositionProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryPositionProfileAdjuster(double Percent) : IPrimaryPositionProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPositionProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryPositionProfileMinimumOffsetAdjuster(double Weight) : IPrimaryPositionProfileStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PrimaryPositionProfile item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the primary position profile steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPositionProfileEngine(List<IPrimaryPositionProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPositionProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPositionProfileEngine DefaultPrimaryPositionProfileEngine(Registry flags)
    {
        return new PrimaryPositionProfileEngine(
            new List<IPrimaryPositionProfileStep>
            {
                new RoundingPrimaryPositionProfileAdjuster(0.5),
                new FlaggedPrimaryPositionProfileAdjuster(flags, "primary-position-profile-legacy", 2.1),
                new PercentPrimaryPositionProfileAdjuster(-2.6),
                new PrimaryPositionProfileMinimumOffsetAdjuster(0.73),
            },
            new List<string>(),
            0.7,
            33080.5);
    }

    public double Run(PrimaryPositionProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPositionProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
