namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount profile calculation.</summary>
public interface IHeadcountProfileStep
{
    string Describe();
    double Adjust(HeadcountProfile item, double valueValue);
}

public sealed record HeadcountProfileAmountOffsetAdjuster(double Weight) : IHeadcountProfileStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(HeadcountProfile item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record LocaleHeadcountProfileAdjuster(Dictionary<string, double> Factors) : IHeadcountProfileStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(HeadcountProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredHeadcountProfileAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountProfile item, double valueValue)
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

public sealed record MinimumHeadcountProfileAdjuster(double Minimum) : IHeadcountProfileStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the headcount profile steps in order and keeps a trace of each.</summary>
public sealed class HeadcountProfileEngine(List<IHeadcountProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountProfileEngine DefaultHeadcountProfileEngine()
    {
        return new HeadcountProfileEngine(
            new List<IHeadcountProfileStep>
            {
                new HeadcountProfileAmountOffsetAdjuster(1.32),
                new LocaleHeadcountProfileAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.82, ["north"] = 0.78 }),
                new TieredHeadcountProfileAdjuster(
                    new List<double> { 4224.0, 6491.0 },
                    new List<double> { 0.93, 0.88 }),
                new MinimumHeadcountProfileAdjuster(20.0),
            },
            new List<string>(),
            1.1,
            32053.3);
    }

    public double Compute(HeadcountProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
