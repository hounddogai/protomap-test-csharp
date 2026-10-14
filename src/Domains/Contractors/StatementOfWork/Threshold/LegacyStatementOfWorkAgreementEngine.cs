namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy statement of work agreement calculation.</summary>
public interface ILegacyStatementOfWorkAgreementAdjuster
{
    string Describe();
    double Adjust(LegacyStatementOfWorkAgreement item, double valueValue);
}

public sealed record LegacyStatementOfWorkAgreementBudgetOffsetAdjuster(double Weight) : ILegacyStatementOfWorkAgreementAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(LegacyStatementOfWorkAgreement item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingLegacyStatementOfWorkAgreementAdjuster(double Step) : ILegacyStatementOfWorkAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyStatementOfWorkAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryLegacyStatementOfWorkAgreementAdjuster(Dictionary<string, double> Factors) : ILegacyStatementOfWorkAgreementAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LegacyStatementOfWorkAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyStatementOfWorkAgreementAdjuster(List<double> Bounds, List<double> Rates) : ILegacyStatementOfWorkAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyStatementOfWorkAgreement item, double valueValue)
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

public sealed record CapLegacyStatementOfWorkAgreementAdjuster(double Cap) : ILegacyStatementOfWorkAgreementAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyStatementOfWorkAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy statement of work agreement steps in order and keeps a trace of each.</summary>
public sealed class LegacyStatementOfWorkAgreementEngine(List<ILegacyStatementOfWorkAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyStatementOfWorkAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyStatementOfWorkAgreementEngine DefaultLegacyStatementOfWorkAgreementEngine()
    {
        return new LegacyStatementOfWorkAgreementEngine(
            new List<ILegacyStatementOfWorkAgreementAdjuster>
            {
                new LegacyStatementOfWorkAgreementBudgetOffsetAdjuster(1.96),
                new RoundingLegacyStatementOfWorkAgreementAdjuster(5.0),
                new CountryLegacyStatementOfWorkAgreementAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.74, ["retail"] = 0.93, ["online"] = 0.95 }),
                new TieredLegacyStatementOfWorkAgreementAdjuster(
                    new List<double> { 3702.0, 4602.0, 4928.0, 9339.0 },
                    new List<double> { 1.15, 1.19, 0.82, 1.17 }),
                new CapLegacyStatementOfWorkAgreementAdjuster(17902.0),
            },
            new List<string>(),
            3.3,
            84607.2);
    }

    public double Evaluate(LegacyStatementOfWorkAgreement item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyStatementOfWorkAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
