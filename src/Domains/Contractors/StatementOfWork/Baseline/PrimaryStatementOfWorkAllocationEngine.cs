namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary statement of work allocation calculation.</summary>
public interface IPrimaryStatementOfWorkAllocationAdjuster
{
    string Describe();
    double Adjust(PrimaryStatementOfWorkAllocation item, double valueValue);
}

public sealed record FlaggedPrimaryStatementOfWorkAllocationAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryStatementOfWorkAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryStatementOfWorkAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryStatementOfWorkAllocationAdjuster(double Minimum) : IPrimaryStatementOfWorkAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryStatementOfWorkAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPrimaryStatementOfWorkAllocationAdjuster(double Cap) : IPrimaryStatementOfWorkAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryStatementOfWorkAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPrimaryStatementOfWorkAllocationAdjuster(double Step) : IPrimaryStatementOfWorkAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryStatementOfWorkAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryStatementOfWorkAllocationAdjuster(double Percent) : IPrimaryStatementOfWorkAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryStatementOfWorkAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary statement of work allocation steps in order and keeps a trace of each.</summary>
public sealed class PrimaryStatementOfWorkAllocationEngine(List<IPrimaryStatementOfWorkAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryStatementOfWorkAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryStatementOfWorkAllocationEngine DefaultPrimaryStatementOfWorkAllocationEngine(Registry flags)
    {
        return new PrimaryStatementOfWorkAllocationEngine(
            new List<IPrimaryStatementOfWorkAllocationAdjuster>
            {
                new FlaggedPrimaryStatementOfWorkAllocationAdjuster(
                    flags,
                    "primary-statement-of-work-allocation-tiered",
                    -1.5),
                new MinimumPrimaryStatementOfWorkAllocationAdjuster(47.8),
                new CapPrimaryStatementOfWorkAllocationAdjuster(41875.0),
                new RoundingPrimaryStatementOfWorkAllocationAdjuster(5.0),
                new PercentPrimaryStatementOfWorkAllocationAdjuster(-3.9),
            },
            new List<string>(),
            2.6,
            35615.8);
    }

    public double Evaluate(PrimaryStatementOfWorkAllocation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryStatementOfWorkAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
