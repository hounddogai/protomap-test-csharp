namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment ledger calculation.</summary>
public interface IAssignmentLedgerModifier
{
    string Describe();
    double Adjust(AssignmentLedger item, double valueValue);
}

public sealed record CapAssignmentLedgerAdjuster(double Cap) : IAssignmentLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAssignmentLedgerAdjuster(double Minimum) : IAssignmentLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAssignmentLedgerAdjuster(double Percent) : IAssignmentLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAssignmentLedgerAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentLedgerModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAssignmentLedgerAdjuster(double Step) : IAssignmentLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the assignment ledger steps in order and keeps a trace of each.</summary>
public sealed class AssignmentLedgerEngine(List<IAssignmentLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentLedgerEngine DefaultAssignmentLedgerEngine(Registry flags)
    {
        return new AssignmentLedgerEngine(
            new List<IAssignmentLedgerModifier>
            {
                new CapAssignmentLedgerAdjuster(34850.0),
                new MinimumAssignmentLedgerAdjuster(30.4),
                new PercentAssignmentLedgerAdjuster(-24.2),
                new FlaggedAssignmentLedgerAdjuster(flags, "assignment-ledger-manual", -9.2),
                new RoundingAssignmentLedgerAdjuster(0.5),
            },
            new List<string>(),
            1.0,
            77103.7);
    }

    public double Evaluate(AssignmentLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
