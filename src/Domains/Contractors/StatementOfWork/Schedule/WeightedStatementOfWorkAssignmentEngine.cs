namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted statement of work assignment calculation.</summary>
public interface IWeightedStatementOfWorkAssignmentFactor
{
    string Describe();
    double Adjust(WeightedStatementOfWorkAssignment item, double valueValue);
}

public sealed record PercentWeightedStatementOfWorkAssignmentAdjuster(double Percent) : IWeightedStatementOfWorkAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedStatementOfWorkAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedStatementOfWorkAssignmentAdjuster(double Minimum) : IWeightedStatementOfWorkAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedStatementOfWorkAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedStatementOfWorkAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IWeightedStatementOfWorkAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedStatementOfWorkAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedStatementOfWorkAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IWeightedStatementOfWorkAssignmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedStatementOfWorkAssignment item, double valueValue)
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

/// <summary>Runs the weighted statement of work assignment steps in order and keeps a trace of each.</summary>
public sealed class WeightedStatementOfWorkAssignmentEngine(List<IWeightedStatementOfWorkAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedStatementOfWorkAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedStatementOfWorkAssignmentEngine DefaultWeightedStatementOfWorkAssignmentEngine(Registry flags)
    {
        return new WeightedStatementOfWorkAssignmentEngine(
            new List<IWeightedStatementOfWorkAssignmentFactor>
            {
                new PercentWeightedStatementOfWorkAssignmentAdjuster(-19.2),
                new MinimumWeightedStatementOfWorkAssignmentAdjuster(18.0),
                new FlaggedWeightedStatementOfWorkAssignmentAdjuster(
                    flags,
                    "weighted-statement-of-work-assignment-legacy",
                    -6.3),
                new TieredWeightedStatementOfWorkAssignmentAdjuster(
                    new List<double> { 1567.0, 2285.0, 6522.0, 7802.0 },
                    new List<double> { 0.87, 0.9, 1.14, 0.8 }),
            },
            new List<string>(),
            4.6,
            41925.0);
    }

    public double Price(WeightedStatementOfWorkAssignment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedStatementOfWorkAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
