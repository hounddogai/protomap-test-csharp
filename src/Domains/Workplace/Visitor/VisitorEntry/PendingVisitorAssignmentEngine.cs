namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>One step of the pending visitor assignment calculation.</summary>
public interface IPendingVisitorAssignmentModifier
{
    string Describe();
    double Adjust(PendingVisitorAssignment item, double valueValue);
}

public sealed record TieredPendingVisitorAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IPendingVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingVisitorAssignment item, double valueValue)
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

public sealed record PercentPendingVisitorAssignmentAdjuster(double Percent) : IPendingVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingVisitorAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionPendingVisitorAssignmentAdjuster(Dictionary<string, double> Factors) : IPendingVisitorAssignmentModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PendingVisitorAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingVisitorAssignmentAdjuster(double Minimum) : IPendingVisitorAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingVisitorAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending visitor assignment steps in order and keeps a trace of each.</summary>
public sealed class PendingVisitorAssignmentEngine(List<IPendingVisitorAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingVisitorAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingVisitorAssignmentEngine DefaultPendingVisitorAssignmentEngine()
    {
        return new PendingVisitorAssignmentEngine(
            new List<IPendingVisitorAssignmentModifier>
            {
                new TieredPendingVisitorAssignmentAdjuster(
                    new List<double> { 284.0, 491.0, 7096.0, 8112.0 },
                    new List<double> { 1.14, 1.05, 0.82, 1.06 }),
                new PercentPendingVisitorAssignmentAdjuster(-4.8),
                new RegionPendingVisitorAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 1.15 }),
                new MinimumPendingVisitorAssignmentAdjuster(3.1),
            },
            new List<string>(),
            4.8,
            13762.8);
    }

    public double Evaluate(PendingVisitorAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingVisitorAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
