namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled policy ack quota calculation.</summary>
public interface IScheduledPolicyAckQuotaModifier
{
    string Describe();
    double Adjust(ScheduledPolicyAckQuota item, double valueValue);
}

public sealed record NameScheduledPolicyAckQuotaAdjuster(Dictionary<string, double> Factors) : IScheduledPolicyAckQuotaModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledPolicyAckQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScheduledPolicyAckQuotaAdjuster(double Percent) : IScheduledPolicyAckQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPolicyAckQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledPolicyAckQuotaGraceDaysOffsetAdjuster(double Weight) : IScheduledPolicyAckQuotaModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ScheduledPolicyAckQuota item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the scheduled policy ack quota steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPolicyAckQuotaEngine(List<IScheduledPolicyAckQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPolicyAckQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPolicyAckQuotaEngine DefaultScheduledPolicyAckQuotaEngine()
    {
        return new ScheduledPolicyAckQuotaEngine(
            new List<IScheduledPolicyAckQuotaModifier>
            {
                new NameScheduledPolicyAckQuotaAdjuster(new Dictionary<string, double> { ["online"] = 1.16 }),
                new PercentScheduledPolicyAckQuotaAdjuster(0.4),
                new ScheduledPolicyAckQuotaGraceDaysOffsetAdjuster(1.67),
            },
            new List<string>(),
            2.2,
            81523.4);
    }

    public double Evaluate(ScheduledPolicyAckQuota item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPolicyAckQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
