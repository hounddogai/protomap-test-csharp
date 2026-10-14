namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped access badge draft calculation.</summary>
public interface ICappedAccessBadgeDraftStep
{
    string Describe();
    double Adjust(CappedAccessBadgeDraft item, double valueValue);
}

public sealed record CapCappedAccessBadgeDraftAdjuster(double Cap) : ICappedAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAccessBadgeDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedAccessBadgeDraftFteOffsetAdjuster(double Weight) : ICappedAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(CappedAccessBadgeDraft item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record PercentCappedAccessBadgeDraftAdjuster(double Percent) : ICappedAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedAccessBadgeDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedAccessBadgeDraftAdjuster(double Step) : ICappedAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAccessBadgeDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped access badge draft steps in order and keeps a trace of each.</summary>
public sealed class CappedAccessBadgeDraftEngine(List<ICappedAccessBadgeDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAccessBadgeDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAccessBadgeDraftEngine DefaultCappedAccessBadgeDraftEngine()
    {
        return new CappedAccessBadgeDraftEngine(
            new List<ICappedAccessBadgeDraftStep>
            {
                new CapCappedAccessBadgeDraftAdjuster(7711.0),
                new CappedAccessBadgeDraftFteOffsetAdjuster(0.78),
                new PercentCappedAccessBadgeDraftAdjuster(9.5),
                new RoundingCappedAccessBadgeDraftAdjuster(0.5),
            },
            new List<string>(),
            2.4,
            78881.7);
    }

    public double Evaluate(CappedAccessBadgeDraft item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAccessBadgeDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
