namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor profile calculation.</summary>
public interface IVisitorProfileStep
{
    string Describe();
    double Adjust(VisitorProfile item, double valueValue);
}

public sealed record MinimumVisitorProfileAdjuster(double Minimum) : IVisitorProfileStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelVisitorProfileAdjuster(Dictionary<string, double> Factors) : IVisitorProfileStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(VisitorProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentVisitorProfileAdjuster(double Percent) : IVisitorProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record VisitorProfilePriorityOffsetAdjuster(double Weight) : IVisitorProfileStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(VisitorProfile item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record FlaggedVisitorProfileAdjuster(Registry Flags, string Flag, double Percent) : IVisitorProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVisitorProfileAdjuster(double Step) : IVisitorProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the visitor profile steps in order and keeps a trace of each.</summary>
public sealed class VisitorProfileEngine(List<IVisitorProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorProfileEngine DefaultVisitorProfileEngine(Registry flags)
    {
        return new VisitorProfileEngine(
            new List<IVisitorProfileStep>
            {
                new MinimumVisitorProfileAdjuster(29.3),
                new LabelVisitorProfileAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.09, ["online"] = 0.89, ["north"] = 0.79 }),
                new PercentVisitorProfileAdjuster(-21.4),
                new VisitorProfilePriorityOffsetAdjuster(1.43),
                new FlaggedVisitorProfileAdjuster(flags, "visitor-profile-global", 4.8),
                new RoundingVisitorProfileAdjuster(0.05),
            },
            new List<string>(),
            1.7,
            61183.9);
    }

    public double Run(VisitorProfile item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
