namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>One step of the lenient pay band score calculation.</summary>
public interface ILenientPayBandScoreFactor
{
    string Describe();
    double Adjust(LenientPayBandScore item, double valueValue);
}

public sealed record LenientPayBandScoreLimitOffsetAdjuster(double Weight) : ILenientPayBandScoreFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(LenientPayBandScore item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredLenientPayBandScoreAdjuster(List<double> Bounds, List<double> Rates) : ILenientPayBandScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientPayBandScore item, double valueValue)
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

public sealed record PercentLenientPayBandScoreAdjuster(double Percent) : ILenientPayBandScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPayBandScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient pay band score steps in order and keeps a trace of each.</summary>
public sealed class LenientPayBandScoreEngine(List<ILenientPayBandScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPayBandScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPayBandScoreEngine DefaultLenientPayBandScoreEngine()
    {
        return new LenientPayBandScoreEngine(
            new List<ILenientPayBandScoreFactor>
            {
                new LenientPayBandScoreLimitOffsetAdjuster(0.05),
                new TieredLenientPayBandScoreAdjuster(
                    new List<double> { 1098.0, 2033.0, 9066.0 },
                    new List<double> { 0.82, 0.9, 0.98 }),
                new PercentLenientPayBandScoreAdjuster(16.4),
            },
            new List<string>(),
            4.3,
            30466.5);
    }

    public double Evaluate(LenientPayBandScore item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientPayBandScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
