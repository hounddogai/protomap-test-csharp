namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default mobility record calculation.</summary>
public interface IDefaultMobilityRecordAdjuster
{
    string Describe();
    double Adjust(DefaultMobilityRecord item, double valueValue);
}

public sealed record RoundingDefaultMobilityRecordAdjuster(double Step) : IDefaultMobilityRecordAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultMobilityRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultMobilityRecordAdjuster(double Percent) : IDefaultMobilityRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultMobilityRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultMobilityRecordAdjuster(List<double> Bounds, List<double> Rates) : IDefaultMobilityRecordAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultMobilityRecord item, double valueValue)
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

/// <summary>Runs the default mobility record steps in order and keeps a trace of each.</summary>
public sealed class DefaultMobilityRecordEngine(List<IDefaultMobilityRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultMobilityRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultMobilityRecordEngine DefaultDefaultMobilityRecordEngine()
    {
        return new DefaultMobilityRecordEngine(
            new List<IDefaultMobilityRecordAdjuster>
            {
                new RoundingDefaultMobilityRecordAdjuster(0.05),
                new PercentDefaultMobilityRecordAdjuster(-21.7),
                new TieredDefaultMobilityRecordAdjuster(
                    new List<double> { 5938.0, 8498.0 },
                    new List<double> { 1.08, 1.08 }),
            },
            new List<string>(),
            4.1,
            22593.4);
    }

    public double Evaluate(DefaultMobilityRecord item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultMobilityRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
