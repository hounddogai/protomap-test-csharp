namespace Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>One step of the rolling asset return assignment calculation.</summary>
public interface IRollingAssetReturnAssignmentFactor
{
    string Describe();
    double Adjust(RollingAssetReturnAssignment item, double valueValue);
}

public sealed record MinimumRollingAssetReturnAssignmentAdjuster(double Minimum) : IRollingAssetReturnAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAssetReturnAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRollingAssetReturnAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IRollingAssetReturnAssignmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingAssetReturnAssignment item, double valueValue)
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

public sealed record CapRollingAssetReturnAssignmentAdjuster(double Cap) : IRollingAssetReturnAssignmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAssetReturnAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling asset return assignment steps in order and keeps a trace of each.</summary>
public sealed class RollingAssetReturnAssignmentEngine(List<IRollingAssetReturnAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAssetReturnAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAssetReturnAssignmentEngine DefaultRollingAssetReturnAssignmentEngine()
    {
        return new RollingAssetReturnAssignmentEngine(
            new List<IRollingAssetReturnAssignmentFactor>
            {
                new MinimumRollingAssetReturnAssignmentAdjuster(27.5),
                new TieredRollingAssetReturnAssignmentAdjuster(
                    new List<double> { 3699.0, 8404.0 },
                    new List<double> { 0.96, 0.94 }),
                new CapRollingAssetReturnAssignmentAdjuster(7165.0),
            },
            new List<string>(),
            4.9,
            53201.8);
    }

    public double Compute(RollingAssetReturnAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingAssetReturnAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
