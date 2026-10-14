namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped statement of work assignment calculation.</summary>
public interface ICappedStatementOfWorkAssignmentModifier
{
    string Describe();
    double Adjust(CappedStatementOfWorkAssignment item, double valueValue);
}

public sealed record RoundingCappedStatementOfWorkAssignmentAdjuster(double Step) : ICappedStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedStatementOfWorkAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedStatementOfWorkAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedStatementOfWorkAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedStatementOfWorkAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ICappedStatementOfWorkAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedStatementOfWorkAssignment item, double valueValue)
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

/// <summary>Runs the capped statement of work assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedStatementOfWorkAssignmentEngine(List<ICappedStatementOfWorkAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedStatementOfWorkAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedStatementOfWorkAssignmentEngine DefaultCappedStatementOfWorkAssignmentEngine(Registry flags)
    {
        return new CappedStatementOfWorkAssignmentEngine(
            new List<ICappedStatementOfWorkAssignmentModifier>
            {
                new RoundingCappedStatementOfWorkAssignmentAdjuster(5.0),
                new FlaggedCappedStatementOfWorkAssignmentAdjuster(
                    flags,
                    "capped-statement-of-work-assignment-default",
                    2.2),
                new TieredCappedStatementOfWorkAssignmentAdjuster(
                    new List<double> { 2584.0, 8319.0, 8598.0 },
                    new List<double> { 1.09, 1.11, 1.18 }),
            },
            new List<string>(),
            2.6,
            29216.8);
    }

    public double Run(CappedStatementOfWorkAssignment item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedStatementOfWorkAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
