namespace Acme.Workforce.Absence.Accrual.Notice;

/// <summary>One step of the primary accrual notice calculation.</summary>
public interface IPrimaryAccrualNoticeFactor
{
    string Describe();
    double Adjust(PrimaryAccrualNotice item, double valueValue);
}

public sealed record MinimumPrimaryAccrualNoticeAdjuster(double Minimum) : IPrimaryAccrualNoticeFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryAccrualNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PrimaryAccrualNoticeStartsAtOffsetAdjuster(double Weight) : IPrimaryAccrualNoticeFactor
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(PrimaryAccrualNotice item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record PercentPrimaryAccrualNoticeAdjuster(double Percent) : IPrimaryAccrualNoticeFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAccrualNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary accrual notice steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAccrualNoticeEngine(List<IPrimaryAccrualNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAccrualNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAccrualNoticeEngine DefaultPrimaryAccrualNoticeEngine()
    {
        return new PrimaryAccrualNoticeEngine(
            new List<IPrimaryAccrualNoticeFactor>
            {
                new MinimumPrimaryAccrualNoticeAdjuster(45.6),
                new PrimaryAccrualNoticeStartsAtOffsetAdjuster(0.59),
                new PercentPrimaryAccrualNoticeAdjuster(11.4),
            },
            new List<string>(),
            0.8,
            20553.4);
    }

    public double Compute(PrimaryAccrualNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAccrualNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
