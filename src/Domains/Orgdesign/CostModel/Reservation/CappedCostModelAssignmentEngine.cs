namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped cost model assignment calculation.</summary>
public interface ICappedCostModelAssignmentStep
{
    string Describe();
    double Adjust(CappedCostModelAssignment item, double valueValue);
}

public sealed record ChannelCappedCostModelAssignmentAdjuster(Dictionary<string, double> Factors) : ICappedCostModelAssignmentStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CappedCostModelAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCappedCostModelAssignmentAdjuster(double Minimum) : ICappedCostModelAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedCostModelAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedCostModelAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ICappedCostModelAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedCostModelAssignment item, double valueValue)
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

public sealed record CapCappedCostModelAssignmentAdjuster(double Cap) : ICappedCostModelAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedCostModelAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped cost model assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedCostModelAssignmentEngine(List<ICappedCostModelAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedCostModelAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedCostModelAssignmentEngine DefaultCappedCostModelAssignmentEngine()
    {
        return new CappedCostModelAssignmentEngine(
            new List<ICappedCostModelAssignmentStep>
            {
                new ChannelCappedCostModelAssignmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.35, ["south"] = 1.28, ["north"] = 0.95 }),
                new MinimumCappedCostModelAssignmentAdjuster(30.3),
                new TieredCappedCostModelAssignmentAdjuster(
                    new List<double> { 1169.0, 1647.0, 3742.0 },
                    new List<double> { 1.11, 1.06, 1.05 }),
                new CapCappedCostModelAssignmentAdjuster(25113.0),
            },
            new List<string>(),
            0.2,
            7744.4);
    }

    public double Price(CappedCostModelAssignment item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedCostModelAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
