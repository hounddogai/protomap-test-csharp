namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work reservation calculation.</summary>
public interface IStatementOfWorkReservationModifier
{
    string Describe();
    double Adjust(StatementOfWorkReservation item, double valueValue);
}

public sealed record StatementOfWorkReservationUpdatedAtOffsetAdjuster(double Weight) : IStatementOfWorkReservationModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(StatementOfWorkReservation item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentStatementOfWorkReservationAdjuster(double Percent) : IStatementOfWorkReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStatementOfWorkReservationAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkReservation item, double valueValue)
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

public sealed record FlaggedStatementOfWorkReservationAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the statement of work reservation steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkReservationEngine(List<IStatementOfWorkReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkReservationEngine DefaultStatementOfWorkReservationEngine(Registry flags)
    {
        return new StatementOfWorkReservationEngine(
            new List<IStatementOfWorkReservationModifier>
            {
                new StatementOfWorkReservationUpdatedAtOffsetAdjuster(1.85),
                new PercentStatementOfWorkReservationAdjuster(2.5),
                new TieredStatementOfWorkReservationAdjuster(
                    new List<double> { 1193.0, 9969.0 },
                    new List<double> { 1.1, 1.09 }),
                new FlaggedStatementOfWorkReservationAdjuster(flags, "statement-of-work-reservation-weighted", 7.1),
            },
            new List<string>(),
            2.6,
            53679.9);
    }

    public double Price(StatementOfWorkReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
