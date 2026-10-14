namespace Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>One step of the headcount notice calculation.</summary>
public interface IHeadcountNoticeAdjuster
{
    string Describe();
    double Adjust(HeadcountNotice item, double valueValue);
}

public sealed record CategoryHeadcountNoticeAdjuster(Dictionary<string, double> Factors) : IHeadcountNoticeAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(HeadcountNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record HeadcountNoticeRatioOffsetAdjuster(double Weight) : IHeadcountNoticeAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(HeadcountNotice item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record PercentHeadcountNoticeAdjuster(double Percent) : IHeadcountNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredHeadcountNoticeAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountNoticeAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountNotice item, double valueValue)
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

public sealed record MinimumHeadcountNoticeAdjuster(double Minimum) : IHeadcountNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the headcount notice steps in order and keeps a trace of each.</summary>
public sealed class HeadcountNoticeEngine(List<IHeadcountNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountNoticeEngine DefaultHeadcountNoticeEngine()
    {
        return new HeadcountNoticeEngine(
            new List<IHeadcountNoticeAdjuster>
            {
                new CategoryHeadcountNoticeAdjuster(new Dictionary<string, double> { ["central"] = 0.72 }),
                new HeadcountNoticeRatioOffsetAdjuster(1.99),
                new PercentHeadcountNoticeAdjuster(17.2),
                new TieredHeadcountNoticeAdjuster(
                    new List<double> { 7782.0, 9081.0, 9701.0 },
                    new List<double> { 0.88, 0.83, 0.96 }),
                new MinimumHeadcountNoticeAdjuster(22.0),
            },
            new List<string>(),
            3.3,
            12601.1);
    }

    public double Compute(HeadcountNotice item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
