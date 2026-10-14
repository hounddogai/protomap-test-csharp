namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy exception agreement calculation.</summary>
public interface ILegacyExceptionAgreementAdjuster
{
    string Describe();
    double Adjust(LegacyExceptionAgreement item, double valueValue);
}

public sealed record PercentLegacyExceptionAgreementAdjuster(double Percent) : ILegacyExceptionAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyExceptionAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleLegacyExceptionAgreementAdjuster(Dictionary<string, double> Factors) : ILegacyExceptionAgreementAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(LegacyExceptionAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyExceptionAgreementAdjuster(List<double> Bounds, List<double> Rates) : ILegacyExceptionAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyExceptionAgreement item, double valueValue)
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

public sealed record MinimumLegacyExceptionAgreementAdjuster(double Minimum) : ILegacyExceptionAgreementAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyExceptionAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyExceptionAgreementAdjuster(double Step) : ILegacyExceptionAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyExceptionAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy exception agreement steps in order and keeps a trace of each.</summary>
public sealed class LegacyExceptionAgreementEngine(List<ILegacyExceptionAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyExceptionAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyExceptionAgreementEngine DefaultLegacyExceptionAgreementEngine()
    {
        return new LegacyExceptionAgreementEngine(
            new List<ILegacyExceptionAgreementAdjuster>
            {
                new PercentLegacyExceptionAgreementAdjuster(-9.8),
                new LocaleLegacyExceptionAgreementAdjuster(new Dictionary<string, double> { ["south"] = 0.96 }),
                new TieredLegacyExceptionAgreementAdjuster(
                    new List<double> { 469.0, 7651.0 },
                    new List<double> { 1.19, 0.91 }),
                new MinimumLegacyExceptionAgreementAdjuster(5.0),
                new RoundingLegacyExceptionAgreementAdjuster(0.05),
            },
            new List<string>(),
            1.0,
            50564.0);
    }

    public double Evaluate(LegacyExceptionAgreement item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyExceptionAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
