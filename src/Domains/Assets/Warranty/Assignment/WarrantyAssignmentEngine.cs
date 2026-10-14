namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty assignment calculation.</summary>
public interface IWarrantyAssignmentFactor
{
    string Describe();
    double Adjust(WarrantyAssignment item, double valueValue);
}

public sealed record WarrantyAssignmentRatioOffsetAdjuster(double Weight) : IWarrantyAssignmentFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(WarrantyAssignment item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record PercentWarrantyAssignmentAdjuster(double Percent) : IWarrantyAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWarrantyAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyAssignmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyAssignment item, double valueValue)
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

public sealed record RoundingWarrantyAssignmentAdjuster(double Step) : IWarrantyAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWarrantyAssignmentAdjuster(double Minimum) : IWarrantyAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentWarrantyAssignmentAdjuster(Dictionary<string, double> Factors) : IWarrantyAssignmentFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(WarrantyAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the warranty assignment steps in order and keeps a trace of each.</summary>
public sealed class WarrantyAssignmentEngine(List<IWarrantyAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyAssignmentEngine DefaultWarrantyAssignmentEngine()
    {
        return new WarrantyAssignmentEngine(
            new List<IWarrantyAssignmentFactor>
            {
                new WarrantyAssignmentRatioOffsetAdjuster(0.22),
                new PercentWarrantyAssignmentAdjuster(-0.3),
                new TieredWarrantyAssignmentAdjuster(
                    new List<double> { 6242.0, 6926.0, 8505.0, 8582.0 },
                    new List<double> { 0.81, 1.05, 1.17, 0.83 }),
                new RoundingWarrantyAssignmentAdjuster(0.5),
                new MinimumWarrantyAssignmentAdjuster(14.3),
                new DepartmentWarrantyAssignmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.96, ["retail"] = 0.97, ["north"] = 1.31 }),
            },
            new List<string>(),
            0.2,
            55906.6);
    }

    public double Run(WarrantyAssignment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
