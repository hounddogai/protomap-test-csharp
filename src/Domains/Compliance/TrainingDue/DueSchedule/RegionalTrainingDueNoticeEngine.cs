namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional training due notice calculation.</summary>
public interface IRegionalTrainingDueNoticeModifier
{
    string Describe();
    double Adjust(RegionalTrainingDueNotice item, double valueValue);
}

public sealed record RoundingRegionalTrainingDueNoticeAdjuster(double Step) : IRegionalTrainingDueNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalTrainingDueNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalTrainingDueNoticeAdjuster(double Minimum) : IRegionalTrainingDueNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalTrainingDueNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalTrainingDueNoticeAdjuster(double Percent) : IRegionalTrainingDueNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalTrainingDueNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional training due notice steps in order and keeps a trace of each.</summary>
public sealed class RegionalTrainingDueNoticeEngine(List<IRegionalTrainingDueNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTrainingDueNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTrainingDueNoticeEngine DefaultRegionalTrainingDueNoticeEngine()
    {
        return new RegionalTrainingDueNoticeEngine(
            new List<IRegionalTrainingDueNoticeModifier>
            {
                new RoundingRegionalTrainingDueNoticeAdjuster(0.25),
                new MinimumRegionalTrainingDueNoticeAdjuster(29.7),
                new PercentRegionalTrainingDueNoticeAdjuster(6.9),
            },
            new List<string>(),
            0.8,
            77160.2);
    }

    public double Evaluate(RegionalTrainingDueNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTrainingDueNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
