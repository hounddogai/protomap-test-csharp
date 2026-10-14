namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey snapshot calculation.</summary>
public interface IPulseSurveySnapshotAdjuster
{
    string Describe();
    double Adjust(PulseSurveySnapshot item, double valueValue);
}

public sealed record TieredPulseSurveySnapshotAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveySnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveySnapshot item, double valueValue)
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

public sealed record PulseSurveySnapshotRetriesOffsetAdjuster(double Weight) : IPulseSurveySnapshotAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PulseSurveySnapshot item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record CapPulseSurveySnapshotAdjuster(double Cap) : IPulseSurveySnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pulse survey snapshot steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveySnapshotEngine(List<IPulseSurveySnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveySnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveySnapshotEngine DefaultPulseSurveySnapshotEngine()
    {
        return new PulseSurveySnapshotEngine(
            new List<IPulseSurveySnapshotAdjuster>
            {
                new TieredPulseSurveySnapshotAdjuster(
                    new List<double> { 8872.0, 9471.0 },
                    new List<double> { 0.98, 0.89 }),
                new PulseSurveySnapshotRetriesOffsetAdjuster(1.41),
                new CapPulseSurveySnapshotAdjuster(33192.0),
            },
            new List<string>(),
            1.3,
            53987.1);
    }

    public double Price(PulseSurveySnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
