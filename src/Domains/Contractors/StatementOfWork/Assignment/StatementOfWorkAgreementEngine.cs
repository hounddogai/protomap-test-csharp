namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work agreement calculation.</summary>
public interface IStatementOfWorkAgreementModifier
{
    string Describe();
    double Adjust(StatementOfWorkAgreement item, double valueValue);
}

public sealed record CapStatementOfWorkAgreementAdjuster(double Cap) : IStatementOfWorkAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedStatementOfWorkAgreementAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStatementOfWorkAgreementAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkAgreement item, double valueValue)
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

public sealed record StatementOfWorkAgreementGraceDaysOffsetAdjuster(double Weight) : IStatementOfWorkAgreementModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(StatementOfWorkAgreement item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record MinimumStatementOfWorkAgreementAdjuster(double Minimum) : IStatementOfWorkAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingStatementOfWorkAgreementAdjuster(double Step) : IStatementOfWorkAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the statement of work agreement steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkAgreementEngine(List<IStatementOfWorkAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkAgreementEngine DefaultStatementOfWorkAgreementEngine(Registry flags)
    {
        return new StatementOfWorkAgreementEngine(
            new List<IStatementOfWorkAgreementModifier>
            {
                new CapStatementOfWorkAgreementAdjuster(44571.0),
                new FlaggedStatementOfWorkAgreementAdjuster(flags, "statement-of-work-agreement-regional", -5.2),
                new TieredStatementOfWorkAgreementAdjuster(
                    new List<double> { 5624.0, 8407.0, 9148.0 },
                    new List<double> { 1.14, 1.13, 0.92 }),
                new StatementOfWorkAgreementGraceDaysOffsetAdjuster(1.12),
                new MinimumStatementOfWorkAgreementAdjuster(28.0),
                new RoundingStatementOfWorkAgreementAdjuster(0.05),
            },
            new List<string>(),
            4.7,
            96719.1);
    }

    public double Price(StatementOfWorkAgreement item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
