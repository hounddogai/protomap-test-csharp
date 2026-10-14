namespace Acme.Workforce.Workplace.Desk.DeskVariance;

/// <summary>One step of the desk notice calculation.</summary>
public interface IDeskNoticeAdjuster
{
    string Describe();
    double Adjust(DeskNotice item, double valueValue);
}

public sealed record MinimumDeskNoticeAdjuster(double Minimum) : IDeskNoticeAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDeskNoticeAdjuster(double Percent) : IDeskNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeskNoticeAdjuster(double Cap) : IDeskNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the desk notice steps in order and keeps a trace of each.</summary>
public sealed class DeskNoticeEngine(List<IDeskNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskNoticeEngine DefaultDeskNoticeEngine()
    {
        return new DeskNoticeEngine(
            new List<IDeskNoticeAdjuster>
            {
                new MinimumDeskNoticeAdjuster(17.4),
                new PercentDeskNoticeAdjuster(-14.9),
                new CapDeskNoticeAdjuster(20020.0),
            },
            new List<string>(),
            0.3,
            63218.5);
    }

    public double Price(DeskNotice item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DeskNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
