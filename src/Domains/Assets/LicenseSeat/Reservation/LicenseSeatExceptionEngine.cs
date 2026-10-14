namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat exception calculation.</summary>
public interface ILicenseSeatExceptionStep
{
    string Describe();
    double Adjust(LicenseSeatException item, double valueValue);
}

public sealed record LicenseSeatExceptionExpiresAtOffsetAdjuster(double Weight) : ILicenseSeatExceptionStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LicenseSeatException item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record SegmentLicenseSeatExceptionAdjuster(Dictionary<string, double> Factors) : ILicenseSeatExceptionStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LicenseSeatException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLicenseSeatExceptionAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLicenseSeatExceptionAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatExceptionStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatException item, double valueValue)
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

public sealed record RoundingLicenseSeatExceptionAdjuster(double Step) : ILicenseSeatExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the license seat exception steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatExceptionEngine(List<ILicenseSeatExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatExceptionEngine DefaultLicenseSeatExceptionEngine(Registry flags)
    {
        return new LicenseSeatExceptionEngine(
            new List<ILicenseSeatExceptionStep>
            {
                new LicenseSeatExceptionExpiresAtOffsetAdjuster(0.71),
                new SegmentLicenseSeatExceptionAdjuster(new Dictionary<string, double> { ["south"] = 1.05 }),
                new FlaggedLicenseSeatExceptionAdjuster(flags, "license-seat-exception-regional", -0.3),
                new TieredLicenseSeatExceptionAdjuster(
                    new List<double> { 2196.0, 4839.0 },
                    new List<double> { 1.0, 1.04 }),
                new RoundingLicenseSeatExceptionAdjuster(0.05),
            },
            new List<string>(),
            2.0,
            86930.7);
    }

    public double Run(LicenseSeatException item)
    {
        var current = item.ProjectedExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
