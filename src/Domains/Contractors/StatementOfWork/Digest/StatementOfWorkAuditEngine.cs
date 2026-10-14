namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work audit calculation.</summary>
public interface IStatementOfWorkAuditModifier
{
    string Describe();
    double Adjust(StatementOfWorkAudit item, double valueValue);
}

public sealed record MinimumStatementOfWorkAuditAdjuster(double Minimum) : IStatementOfWorkAuditModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStatementOfWorkAuditAdjuster(double Cap) : IStatementOfWorkAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStatementOfWorkAuditAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkAudit item, double valueValue)
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

public sealed record FlaggedStatementOfWorkAuditAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work audit steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkAuditEngine(List<IStatementOfWorkAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkAuditEngine DefaultStatementOfWorkAuditEngine(Registry flags)
    {
        return new StatementOfWorkAuditEngine(
            new List<IStatementOfWorkAuditModifier>
            {
                new MinimumStatementOfWorkAuditAdjuster(0.1),
                new CapStatementOfWorkAuditAdjuster(17573.0),
                new TieredStatementOfWorkAuditAdjuster(
                    new List<double> { 1665.0, 8213.0 },
                    new List<double> { 1.05, 1.07 }),
                new FlaggedStatementOfWorkAuditAdjuster(flags, "statement-of-work-audit-regional", -7.3),
            },
            new List<string>(),
            0.1,
            84713.3);
    }

    public double Compute(StatementOfWorkAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
