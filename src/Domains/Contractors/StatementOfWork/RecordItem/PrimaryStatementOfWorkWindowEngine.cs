namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary statement of work window calculation.</summary>
public interface IPrimaryStatementOfWorkWindowModifier
{
    string Describe();
    double Adjust(PrimaryStatementOfWorkWindow item, double valueValue);
}

public sealed record CapPrimaryStatementOfWorkWindowAdjuster(double Cap) : IPrimaryStatementOfWorkWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryStatementOfWorkWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryStatementOfWorkWindowAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryStatementOfWorkWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryStatementOfWorkWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourcePrimaryStatementOfWorkWindowAdjuster(Dictionary<string, double> Factors) : IPrimaryStatementOfWorkWindowModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PrimaryStatementOfWorkWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the primary statement of work window steps in order and keeps a trace of each.</summary>
public sealed class PrimaryStatementOfWorkWindowEngine(List<IPrimaryStatementOfWorkWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryStatementOfWorkWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryStatementOfWorkWindowEngine DefaultPrimaryStatementOfWorkWindowEngine(Registry flags)
    {
        return new PrimaryStatementOfWorkWindowEngine(
            new List<IPrimaryStatementOfWorkWindowModifier>
            {
                new CapPrimaryStatementOfWorkWindowAdjuster(41336.0),
                new FlaggedPrimaryStatementOfWorkWindowAdjuster(
                    flags,
                    "primary-statement-of-work-window-manual",
                    -6.0),
                new SourcePrimaryStatementOfWorkWindowAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.37, ["central"] = 0.78 }),
            },
            new List<string>(),
            0.4,
            98197.0);
    }

    public double Evaluate(PrimaryStatementOfWorkWindow item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryStatementOfWorkWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
