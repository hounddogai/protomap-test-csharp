namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default statement of work audit calculation.</summary>
public interface IDefaultStatementOfWorkAuditAdjuster
{
    string Describe();
    double Adjust(DefaultStatementOfWorkAudit item, double valueValue);
}

public sealed record TieredDefaultStatementOfWorkAuditAdjuster(List<double> Bounds, List<double> Rates) : IDefaultStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultStatementOfWorkAudit item, double valueValue)
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

public sealed record FlaggedDefaultStatementOfWorkAuditAdjuster(Registry Flags, string Flag, double Percent) : IDefaultStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultStatementOfWorkAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultStatementOfWorkAuditAdjuster(double Cap) : IDefaultStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultStatementOfWorkAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultStatementOfWorkAuditCostOffsetAdjuster(double Weight) : IDefaultStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(DefaultStatementOfWorkAudit item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the default statement of work audit steps in order and keeps a trace of each.</summary>
public sealed class DefaultStatementOfWorkAuditEngine(List<IDefaultStatementOfWorkAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultStatementOfWorkAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultStatementOfWorkAuditEngine DefaultDefaultStatementOfWorkAuditEngine(Registry flags)
    {
        return new DefaultStatementOfWorkAuditEngine(
            new List<IDefaultStatementOfWorkAuditAdjuster>
            {
                new TieredDefaultStatementOfWorkAuditAdjuster(
                    new List<double> { 2657.0, 9368.0 },
                    new List<double> { 0.81, 0.96 }),
                new FlaggedDefaultStatementOfWorkAuditAdjuster(flags, "default-statement-of-work-audit-strict", -9.6),
                new CapDefaultStatementOfWorkAuditAdjuster(38865.0),
                new DefaultStatementOfWorkAuditCostOffsetAdjuster(1.72),
            },
            new List<string>(),
            4.7,
            21077.2);
    }

    public double Evaluate(DefaultStatementOfWorkAudit item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultStatementOfWorkAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
