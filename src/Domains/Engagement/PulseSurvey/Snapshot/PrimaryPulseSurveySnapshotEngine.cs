namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary pulse survey snapshot calculation.</summary>
public interface IPrimaryPulseSurveySnapshotModifier
{
    string Describe();
    double Adjust(PrimaryPulseSurveySnapshot item, double valueValue);
}

public sealed record NamePrimaryPulseSurveySnapshotAdjuster(Dictionary<string, double> Factors) : IPrimaryPulseSurveySnapshotModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryPulseSurveySnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPrimaryPulseSurveySnapshotAdjuster(double Minimum) : IPrimaryPulseSurveySnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryPulseSurveySnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPrimaryPulseSurveySnapshotAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryPulseSurveySnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryPulseSurveySnapshot item, double valueValue)
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

public sealed record RoundingPrimaryPulseSurveySnapshotAdjuster(double Step) : IPrimaryPulseSurveySnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryPulseSurveySnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary pulse survey snapshot steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPulseSurveySnapshotEngine(List<IPrimaryPulseSurveySnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPulseSurveySnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPulseSurveySnapshotEngine DefaultPrimaryPulseSurveySnapshotEngine()
    {
        return new PrimaryPulseSurveySnapshotEngine(
            new List<IPrimaryPulseSurveySnapshotModifier>
            {
                new NamePrimaryPulseSurveySnapshotAdjuster(new Dictionary<string, double> { ["north"] = 1.08 }),
                new MinimumPrimaryPulseSurveySnapshotAdjuster(34.5),
                new TieredPrimaryPulseSurveySnapshotAdjuster(
                    new List<double> { 3210.0, 3836.0 },
                    new List<double> { 1.19, 1.01 }),
                new RoundingPrimaryPulseSurveySnapshotAdjuster(1.0),
            },
            new List<string>(),
            4.9,
            45603.6);
    }

    public double Price(PrimaryPulseSurveySnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPulseSurveySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
