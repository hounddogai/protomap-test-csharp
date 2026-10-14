namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the span batch calculation.</summary>
public interface ISpanBatchAdjuster
{
    string Describe();
    double Adjust(SpanBatch item, double valueValue);
}

public sealed record FlaggedSpanBatchAdjuster(Registry Flags, string Flag, double Percent) : ISpanBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSpanBatchAdjuster(double Minimum) : ISpanBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentSpanBatchAdjuster(Dictionary<string, double> Factors) : ISpanBatchAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SpanBatchBudgetOffsetAdjuster(double Weight) : ISpanBatchAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(SpanBatch item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the span batch steps in order and keeps a trace of each.</summary>
public sealed class SpanBatchEngine(List<ISpanBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanBatchEngine DefaultSpanBatchEngine(Registry flags)
    {
        return new SpanBatchEngine(
            new List<ISpanBatchAdjuster>
            {
                new FlaggedSpanBatchAdjuster(flags, "span-batch-adaptive", -1.4),
                new MinimumSpanBatchAdjuster(16.4),
                new DepartmentSpanBatchAdjuster(new Dictionary<string, double> { ["north"] = 0.75 }),
                new SpanBatchBudgetOffsetAdjuster(1.72),
            },
            new List<string>(),
            3.6,
            96226.7);
    }

    public double Evaluate(SpanBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<SpanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
