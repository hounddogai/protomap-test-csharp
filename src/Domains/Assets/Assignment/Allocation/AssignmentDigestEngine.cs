namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment digest calculation.</summary>
public interface IAssignmentDigestAdjuster
{
    string Describe();
    double Adjust(AssignmentDigest item, double valueValue);
}

public sealed record FlaggedAssignmentDigestAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssignmentDigestAdjuster(double Minimum) : IAssignmentDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAssignmentDigestAdjuster(double Cap) : IAssignmentDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the assignment digest steps in order and keeps a trace of each.</summary>
public sealed class AssignmentDigestEngine(List<IAssignmentDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentDigestEngine DefaultAssignmentDigestEngine(Registry flags)
    {
        return new AssignmentDigestEngine(
            new List<IAssignmentDigestAdjuster>
            {
                new FlaggedAssignmentDigestAdjuster(flags, "assignment-digest-tiered", -4.2),
                new MinimumAssignmentDigestAdjuster(28.1),
                new CapAssignmentDigestAdjuster(44037.0),
            },
            new List<string>(),
            2.6,
            61478.9);
    }

    public double Price(AssignmentDigest item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
