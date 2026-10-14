namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pool agreement calculation.</summary>
public interface ICappedPoolAgreementAdjuster
{
    string Describe();
    double Adjust(CappedPoolAgreement item, double valueValue);
}

public sealed record TieredCappedPoolAgreementAdjuster(List<double> Bounds, List<double> Rates) : ICappedPoolAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPoolAgreement item, double valueValue)
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

public sealed record PercentCappedPoolAgreementAdjuster(double Percent) : ICappedPoolAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPoolAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedPoolAgreementAdjuster(double Step) : ICappedPoolAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedPoolAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped pool agreement steps in order and keeps a trace of each.</summary>
public sealed class CappedPoolAgreementEngine(List<ICappedPoolAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPoolAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPoolAgreementEngine DefaultCappedPoolAgreementEngine()
    {
        return new CappedPoolAgreementEngine(
            new List<ICappedPoolAgreementAdjuster>
            {
                new TieredCappedPoolAgreementAdjuster(
                    new List<double> { 4261.0, 7513.0, 8459.0 },
                    new List<double> { 1.19, 0.92, 0.88 }),
                new PercentCappedPoolAgreementAdjuster(-7.0),
                new RoundingCappedPoolAgreementAdjuster(1.0),
            },
            new List<string>(),
            4.4,
            47929.8);
    }

    public double Compute(CappedPoolAgreement item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPoolAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
