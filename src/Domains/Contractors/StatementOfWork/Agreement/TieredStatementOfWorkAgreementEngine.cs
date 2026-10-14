namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered statement of work agreement calculation.</summary>
public interface ITieredStatementOfWorkAgreementStep
{
    string Describe();
    double Adjust(TieredStatementOfWorkAgreement item, double valueValue);
}

public sealed record FlaggedTieredStatementOfWorkAgreementAdjuster(Registry Flags, string Flag, double Percent) : ITieredStatementOfWorkAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredStatementOfWorkAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredStatementOfWorkAgreementAdjuster(double Cap) : ITieredStatementOfWorkAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredStatementOfWorkAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTieredStatementOfWorkAgreementAdjuster(double Step) : ITieredStatementOfWorkAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredStatementOfWorkAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTieredStatementOfWorkAgreementAdjuster(List<double> Bounds, List<double> Rates) : ITieredStatementOfWorkAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredStatementOfWorkAgreement item, double valueValue)
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

public sealed record PercentTieredStatementOfWorkAgreementAdjuster(double Percent) : ITieredStatementOfWorkAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredStatementOfWorkAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameTieredStatementOfWorkAgreementAdjuster(Dictionary<string, double> Factors) : ITieredStatementOfWorkAgreementStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredStatementOfWorkAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered statement of work agreement steps in order and keeps a trace of each.</summary>
public sealed class TieredStatementOfWorkAgreementEngine(List<ITieredStatementOfWorkAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredStatementOfWorkAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredStatementOfWorkAgreementEngine DefaultTieredStatementOfWorkAgreementEngine(Registry flags)
    {
        return new TieredStatementOfWorkAgreementEngine(
            new List<ITieredStatementOfWorkAgreementStep>
            {
                new FlaggedTieredStatementOfWorkAgreementAdjuster(
                    flags,
                    "tiered-statement-of-work-agreement-legacy",
                    6.7),
                new CapTieredStatementOfWorkAgreementAdjuster(11553.0),
                new RoundingTieredStatementOfWorkAgreementAdjuster(0.05),
                new TieredTieredStatementOfWorkAgreementAdjuster(
                    new List<double> { 975.0, 6045.0, 9502.0 },
                    new List<double> { 1.1, 1.07, 1.02 }),
                new PercentTieredStatementOfWorkAgreementAdjuster(19.1),
                new NameTieredStatementOfWorkAgreementAdjuster(new Dictionary<string, double> { ["central"] = 0.72 }),
            },
            new List<string>(),
            3.1,
            77543.5);
    }

    public double Price(TieredStatementOfWorkAgreement item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredStatementOfWorkAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
