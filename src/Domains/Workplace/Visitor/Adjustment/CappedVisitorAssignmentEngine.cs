namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped visitor assignment calculation.</summary>
public interface ICappedVisitorAssignmentModifier
{
    string Describe();
    double Adjust(CappedVisitorAssignment item, double valueValue);
}

public sealed record RoundingCappedVisitorAssignmentAdjuster(double Step) : ICappedVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedVisitorAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedVisitorAssignmentExpiresAtOffsetAdjuster(double Weight) : ICappedVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(CappedVisitorAssignment item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record MinimumCappedVisitorAssignmentAdjuster(double Minimum) : ICappedVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedVisitorAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCappedVisitorAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedVisitorAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedVisitorAssignmentAdjuster(double Percent) : ICappedVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedVisitorAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedVisitorAssignmentAdjuster(double Cap) : ICappedVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedVisitorAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped visitor assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedVisitorAssignmentEngine(List<ICappedVisitorAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedVisitorAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedVisitorAssignmentEngine DefaultCappedVisitorAssignmentEngine(Registry flags)
    {
        return new CappedVisitorAssignmentEngine(
            new List<ICappedVisitorAssignmentModifier>
            {
                new RoundingCappedVisitorAssignmentAdjuster(0.5),
                new CappedVisitorAssignmentExpiresAtOffsetAdjuster(1.09),
                new MinimumCappedVisitorAssignmentAdjuster(26.5),
                new FlaggedCappedVisitorAssignmentAdjuster(flags, "capped-visitor-assignment-adaptive", 5.3),
                new PercentCappedVisitorAssignmentAdjuster(-6.5),
                new CapCappedVisitorAssignmentAdjuster(13866.0),
            },
            new List<string>(),
            0.7,
            90933.8);
    }

    public double Compute(CappedVisitorAssignment item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CappedVisitorAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
