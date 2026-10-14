namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy record calculation.</summary>
public interface IOccupancyRecordFactor
{
    string Describe();
    double Adjust(OccupancyRecord item, double valueValue);
}

public sealed record RoundingOccupancyRecordAdjuster(double Step) : IOccupancyRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentOccupancyRecordAdjuster(double Percent) : IOccupancyRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOccupancyRecordAdjuster(double Cap) : IOccupancyRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancyRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredOccupancyRecordAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyRecord item, double valueValue)
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

/// <summary>Runs the occupancy record steps in order and keeps a trace of each.</summary>
public sealed class OccupancyRecordEngine(List<IOccupancyRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyRecordEngine DefaultOccupancyRecordEngine()
    {
        return new OccupancyRecordEngine(
            new List<IOccupancyRecordFactor>
            {
                new RoundingOccupancyRecordAdjuster(0.1),
                new PercentOccupancyRecordAdjuster(9.4),
                new CapOccupancyRecordAdjuster(15760.0),
                new TieredOccupancyRecordAdjuster(
                    new List<double> { 2109.0, 3283.0, 4829.0 },
                    new List<double> { 0.91, 0.86, 0.8 }),
            },
            new List<string>(),
            1.1,
            97534.1);
    }

    public double Price(OccupancyRecord item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
