namespace Acme.Workforce.Assets.Refresh.Preference;

/// <summary>One step of the refresh assignment calculation.</summary>
public interface IRefreshAssignmentAdjuster
{
    string Describe();
    double Adjust(RefreshAssignment item, double valueValue);
}

public sealed record CapRefreshAssignmentAdjuster(double Cap) : IRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RefreshAssignmentFteOffsetAdjuster(double Weight) : IRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(RefreshAssignment item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TieredRefreshAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshAssignment item, double valueValue)
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

public sealed record MinimumRefreshAssignmentAdjuster(double Minimum) : IRefreshAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh assignment steps in order and keeps a trace of each.</summary>
public sealed class RefreshAssignmentEngine(List<IRefreshAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshAssignmentEngine DefaultRefreshAssignmentEngine()
    {
        return new RefreshAssignmentEngine(
            new List<IRefreshAssignmentAdjuster>
            {
                new CapRefreshAssignmentAdjuster(27975.0),
                new RefreshAssignmentFteOffsetAdjuster(1.44),
                new TieredRefreshAssignmentAdjuster(
                    new List<double> { 1527.0, 3334.0, 4460.0 },
                    new List<double> { 0.87, 1.18, 0.93 }),
                new MinimumRefreshAssignmentAdjuster(12.0),
            },
            new List<string>(),
            2.7,
            42125.2);
    }

    public double Evaluate(RefreshAssignment item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
