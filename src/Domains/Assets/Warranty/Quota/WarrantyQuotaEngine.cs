namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty quota calculation.</summary>
public interface IWarrantyQuotaAdjuster
{
    string Describe();
    double Adjust(WarrantyQuota item, double valueValue);
}

public sealed record RoundingWarrantyQuotaAdjuster(double Step) : IWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentWarrantyQuotaAdjuster(double Percent) : IWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedWarrantyQuotaAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the warranty quota steps in order and keeps a trace of each.</summary>
public sealed class WarrantyQuotaEngine(List<IWarrantyQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyQuotaEngine DefaultWarrantyQuotaEngine(Registry flags)
    {
        return new WarrantyQuotaEngine(
            new List<IWarrantyQuotaAdjuster>
            {
                new RoundingWarrantyQuotaAdjuster(1.0),
                new PercentWarrantyQuotaAdjuster(13.6),
                new FlaggedWarrantyQuotaAdjuster(flags, "warranty-quota-tiered", 9.9),
            },
            new List<string>(),
            3.4,
            97324.1);
    }

    public double Price(WarrantyQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
