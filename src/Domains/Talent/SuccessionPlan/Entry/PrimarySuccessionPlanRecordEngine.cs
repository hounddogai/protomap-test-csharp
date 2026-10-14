namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary succession plan record calculation.</summary>
public interface IPrimarySuccessionPlanRecordAdjuster
{
    string Describe();
    double Adjust(PrimarySuccessionPlanRecord item, double valueValue);
}

public sealed record TieredPrimarySuccessionPlanRecordAdjuster(List<double> Bounds, List<double> Rates) : IPrimarySuccessionPlanRecordAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimarySuccessionPlanRecord item, double valueValue)
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

public sealed record FlaggedPrimarySuccessionPlanRecordAdjuster(Registry Flags, string Flag, double Percent) : IPrimarySuccessionPlanRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimarySuccessionPlanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimarySuccessionPlanRecordAdjuster(double Cap) : IPrimarySuccessionPlanRecordAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimarySuccessionPlanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PrimarySuccessionPlanRecordDurationMinutesOffsetAdjuster(double Weight) : IPrimarySuccessionPlanRecordAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PrimarySuccessionPlanRecord item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record PercentPrimarySuccessionPlanRecordAdjuster(double Percent) : IPrimarySuccessionPlanRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimarySuccessionPlanRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary succession plan record steps in order and keeps a trace of each.</summary>
public sealed class PrimarySuccessionPlanRecordEngine(List<IPrimarySuccessionPlanRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySuccessionPlanRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySuccessionPlanRecordEngine DefaultPrimarySuccessionPlanRecordEngine(Registry flags)
    {
        return new PrimarySuccessionPlanRecordEngine(
            new List<IPrimarySuccessionPlanRecordAdjuster>
            {
                new TieredPrimarySuccessionPlanRecordAdjuster(
                    new List<double> { 926.0, 2924.0, 5161.0 },
                    new List<double> { 1.19, 0.86, 1.03 }),
                new FlaggedPrimarySuccessionPlanRecordAdjuster(flags, "primary-succession-plan-record-batched", 5.7),
                new CapPrimarySuccessionPlanRecordAdjuster(25561.0),
                new PrimarySuccessionPlanRecordDurationMinutesOffsetAdjuster(1.77),
                new PercentPrimarySuccessionPlanRecordAdjuster(-20.7),
            },
            new List<string>(),
            4.1,
            46303.3);
    }

    public double Price(PrimarySuccessionPlanRecord item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySuccessionPlanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
