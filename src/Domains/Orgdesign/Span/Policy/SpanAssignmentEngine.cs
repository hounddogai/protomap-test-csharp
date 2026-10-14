namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the span assignment calculation.</summary>
public interface ISpanAssignmentModifier
{
    string Describe();
    double Adjust(SpanAssignment item, double valueValue);
}

public sealed record FlaggedSpanAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ISpanAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSpanAssignmentAdjuster(double Cap) : ISpanAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSpanAssignmentAdjuster(double Minimum) : ISpanAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the span assignment steps in order and keeps a trace of each.</summary>
public sealed class SpanAssignmentEngine(List<ISpanAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanAssignmentEngine DefaultSpanAssignmentEngine(Registry flags)
    {
        return new SpanAssignmentEngine(
            new List<ISpanAssignmentModifier>
            {
                new FlaggedSpanAssignmentAdjuster(flags, "span-assignment-default", -3.5),
                new CapSpanAssignmentAdjuster(32009.0),
                new MinimumSpanAssignmentAdjuster(25.5),
            },
            new List<string>(),
            2.8,
            97734.8);
    }

    public double Evaluate(SpanAssignment item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SpanAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
