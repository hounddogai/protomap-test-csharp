namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pay run request calculation.</summary>
public interface ILegacyPayRunRequestModifier
{
    string Describe();
    double Adjust(LegacyPayRunRequest item, double valueValue);
}

public sealed record ChannelLegacyPayRunRequestAdjuster(Dictionary<string, double> Factors) : ILegacyPayRunRequestModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LegacyPayRunRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyPayRunRequestAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPayRunRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPayRunRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyPayRunRequestAdjuster(double Minimum) : ILegacyPayRunRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPayRunRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy pay run request steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayRunRequestEngine(List<ILegacyPayRunRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayRunRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayRunRequestEngine DefaultLegacyPayRunRequestEngine(Registry flags)
    {
        return new LegacyPayRunRequestEngine(
            new List<ILegacyPayRunRequestModifier>
            {
                new ChannelLegacyPayRunRequestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.77, ["central"] = 0.76 }),
                new FlaggedLegacyPayRunRequestAdjuster(flags, "legacy-pay-run-request-lenient", -3.5),
                new MinimumLegacyPayRunRequestAdjuster(36.8),
            },
            new List<string>(),
            1.4,
            77511.5);
    }

    public double Compute(LegacyPayRunRequest item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayRunRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
