namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk profile calculation.</summary>
public interface IDeskProfileStep
{
    string Describe();
    double Adjust(DeskProfile item, double valueValue);
}

public sealed record MinimumDeskProfileAdjuster(double Minimum) : IDeskProfileStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDeskProfileAdjuster(double Percent) : IDeskProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DeskProfilePriorityOffsetAdjuster(double Weight) : IDeskProfileStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(DeskProfile item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record FlaggedDeskProfileAdjuster(Registry Flags, string Flag, double Percent) : IDeskProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the desk profile steps in order and keeps a trace of each.</summary>
public sealed class DeskProfileEngine(List<IDeskProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskProfileEngine DefaultDeskProfileEngine(Registry flags)
    {
        return new DeskProfileEngine(
            new List<IDeskProfileStep>
            {
                new MinimumDeskProfileAdjuster(36.3),
                new PercentDeskProfileAdjuster(3.6),
                new DeskProfilePriorityOffsetAdjuster(0.81),
                new FlaggedDeskProfileAdjuster(flags, "desk-profile-pending", -1.3),
            },
            new List<string>(),
            1.0,
            80732.0);
    }

    public double Run(DeskProfile item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DeskProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
