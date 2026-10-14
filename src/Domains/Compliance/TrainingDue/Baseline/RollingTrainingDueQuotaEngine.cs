namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling training due quota calculation.</summary>
public interface IRollingTrainingDueQuotaFactor
{
    string Describe();
    double Adjust(RollingTrainingDueQuota item, double valueValue);
}

public sealed record PercentRollingTrainingDueQuotaAdjuster(double Percent) : IRollingTrainingDueQuotaFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingTrainingDueQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRollingTrainingDueQuotaAdjuster(List<double> Bounds, List<double> Rates) : IRollingTrainingDueQuotaFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingTrainingDueQuota item, double valueValue)
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

public sealed record MinimumRollingTrainingDueQuotaAdjuster(double Minimum) : IRollingTrainingDueQuotaFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingTrainingDueQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RollingTrainingDueQuotaFloorValueOffsetAdjuster(double Weight) : IRollingTrainingDueQuotaFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(RollingTrainingDueQuota item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapRollingTrainingDueQuotaAdjuster(double Cap) : IRollingTrainingDueQuotaFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingTrainingDueQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingTrainingDueQuotaAdjuster(Registry Flags, string Flag, double Percent) : IRollingTrainingDueQuotaFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingTrainingDueQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling training due quota steps in order and keeps a trace of each.</summary>
public sealed class RollingTrainingDueQuotaEngine(List<IRollingTrainingDueQuotaFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingTrainingDueQuotaFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingTrainingDueQuotaEngine DefaultRollingTrainingDueQuotaEngine(Registry flags)
    {
        return new RollingTrainingDueQuotaEngine(
            new List<IRollingTrainingDueQuotaFactor>
            {
                new PercentRollingTrainingDueQuotaAdjuster(-16.3),
                new TieredRollingTrainingDueQuotaAdjuster(
                    new List<double> { 5013.0, 7739.0, 8090.0 },
                    new List<double> { 1.03, 1.07, 1.08 }),
                new MinimumRollingTrainingDueQuotaAdjuster(1.1),
                new RollingTrainingDueQuotaFloorValueOffsetAdjuster(0.29),
                new CapRollingTrainingDueQuotaAdjuster(3832.0),
                new FlaggedRollingTrainingDueQuotaAdjuster(flags, "rolling-training-due-quota-legacy", 5.2),
            },
            new List<string>(),
            0.4,
            21802.4);
    }

    public double Price(RollingTrainingDueQuota item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingTrainingDueQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
