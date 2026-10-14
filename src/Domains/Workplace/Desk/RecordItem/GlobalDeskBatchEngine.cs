namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global desk batch calculation.</summary>
public interface IGlobalDeskBatchStep
{
    string Describe();
    double Adjust(GlobalDeskBatch item, double valueValue);
}

public sealed record PercentGlobalDeskBatchAdjuster(double Percent) : IGlobalDeskBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalDeskBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalDeskBatchAdjuster(double Minimum) : IGlobalDeskBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalDeskBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGlobalDeskBatchAdjuster(Registry Flags, string Flag, double Percent) : IGlobalDeskBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalDeskBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalDeskBatchBudgetOffsetAdjuster(double Weight) : IGlobalDeskBatchStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(GlobalDeskBatch item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the global desk batch steps in order and keeps a trace of each.</summary>
public sealed class GlobalDeskBatchEngine(List<IGlobalDeskBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalDeskBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalDeskBatchEngine DefaultGlobalDeskBatchEngine(Registry flags)
    {
        return new GlobalDeskBatchEngine(
            new List<IGlobalDeskBatchStep>
            {
                new PercentGlobalDeskBatchAdjuster(-3.9),
                new MinimumGlobalDeskBatchAdjuster(50.0),
                new FlaggedGlobalDeskBatchAdjuster(flags, "global-desk-batch-tiered", 4.5),
                new GlobalDeskBatchBudgetOffsetAdjuster(0.92),
            },
            new List<string>(),
            4.6,
            32340.2);
    }

    public double Price(GlobalDeskBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalDeskBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
