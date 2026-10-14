namespace Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>One step of the headcount entry calculation.</summary>
public interface IHeadcountEntryAdjuster
{
    string Describe();
    double Adjust(HeadcountEntry item, double valueValue);
}

public sealed record CapHeadcountEntryAdjuster(double Cap) : IHeadcountEntryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record HeadcountEntryDurationMinutesOffsetAdjuster(double Weight) : IHeadcountEntryAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(HeadcountEntry item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record PercentHeadcountEntryAdjuster(double Percent) : IHeadcountEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelHeadcountEntryAdjuster(Dictionary<string, double> Factors) : IHeadcountEntryAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(HeadcountEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the headcount entry steps in order and keeps a trace of each.</summary>
public sealed class HeadcountEntryEngine(List<IHeadcountEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountEntryEngine DefaultHeadcountEntryEngine()
    {
        return new HeadcountEntryEngine(
            new List<IHeadcountEntryAdjuster>
            {
                new CapHeadcountEntryAdjuster(23723.0),
                new HeadcountEntryDurationMinutesOffsetAdjuster(0.26),
                new PercentHeadcountEntryAdjuster(22.9),
                new ChannelHeadcountEntryAdjuster(new Dictionary<string, double> { ["central"] = 1.03 }),
            },
            new List<string>(),
            0.9,
            14806.7);
    }

    public double Price(HeadcountEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
