namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work quota calculation.</summary>
public interface IStatementOfWorkQuotaAdjuster
{
    string Describe();
    double Adjust(StatementOfWorkQuota item, double valueValue);
}

public sealed record FlaggedStatementOfWorkQuotaAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStatementOfWorkQuotaAdjuster(double Step) : IStatementOfWorkQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStatementOfWorkQuotaAdjuster(double Minimum) : IStatementOfWorkQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStatementOfWorkQuotaAdjuster(double Cap) : IStatementOfWorkQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStatementOfWorkQuotaAdjuster(double Percent) : IStatementOfWorkQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StatementOfWorkQuotaScoreOffsetAdjuster(double Weight) : IStatementOfWorkQuotaAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(StatementOfWorkQuota item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the statement of work quota steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkQuotaEngine(List<IStatementOfWorkQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkQuotaEngine DefaultStatementOfWorkQuotaEngine(Registry flags)
    {
        return new StatementOfWorkQuotaEngine(
            new List<IStatementOfWorkQuotaAdjuster>
            {
                new FlaggedStatementOfWorkQuotaAdjuster(flags, "statement-of-work-quota-tiered", -3.8),
                new RoundingStatementOfWorkQuotaAdjuster(5.0),
                new MinimumStatementOfWorkQuotaAdjuster(16.2),
                new CapStatementOfWorkQuotaAdjuster(9282.0),
                new PercentStatementOfWorkQuotaAdjuster(0.1),
                new StatementOfWorkQuotaScoreOffsetAdjuster(1.56),
            },
            new List<string>(),
            0.1,
            28372.1);
    }

    public double Price(StatementOfWorkQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
