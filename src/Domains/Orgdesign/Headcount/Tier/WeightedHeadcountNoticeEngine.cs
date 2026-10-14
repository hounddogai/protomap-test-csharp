namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted headcount notice calculation.</summary>
public interface IWeightedHeadcountNoticeFactor
{
    string Describe();
    double Adjust(WeightedHeadcountNotice item, double valueValue);
}

public sealed record WeightedHeadcountNoticeQuantityOffsetAdjuster(double Weight) : IWeightedHeadcountNoticeFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(WeightedHeadcountNotice item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record MinimumWeightedHeadcountNoticeAdjuster(double Minimum) : IWeightedHeadcountNoticeFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedHeadcountNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWeightedHeadcountNoticeAdjuster(double Percent) : IWeightedHeadcountNoticeFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedHeadcountNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedHeadcountNoticeAdjuster(double Step) : IWeightedHeadcountNoticeFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedHeadcountNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredWeightedHeadcountNoticeAdjuster(List<double> Bounds, List<double> Rates) : IWeightedHeadcountNoticeFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedHeadcountNotice item, double valueValue)
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

/// <summary>Runs the weighted headcount notice steps in order and keeps a trace of each.</summary>
public sealed class WeightedHeadcountNoticeEngine(List<IWeightedHeadcountNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedHeadcountNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedHeadcountNoticeEngine DefaultWeightedHeadcountNoticeEngine()
    {
        return new WeightedHeadcountNoticeEngine(
            new List<IWeightedHeadcountNoticeFactor>
            {
                new WeightedHeadcountNoticeQuantityOffsetAdjuster(0.87),
                new MinimumWeightedHeadcountNoticeAdjuster(25.3),
                new PercentWeightedHeadcountNoticeAdjuster(2.3),
                new RoundingWeightedHeadcountNoticeAdjuster(0.1),
                new TieredWeightedHeadcountNoticeAdjuster(
                    new List<double> { 4896.0, 7274.0 },
                    new List<double> { 0.96, 1.12 }),
            },
            new List<string>(),
            0.4,
            17346.6);
    }

    public double Price(WeightedHeadcountNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedHeadcountNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
