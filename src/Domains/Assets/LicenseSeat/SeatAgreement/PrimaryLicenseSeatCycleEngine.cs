namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary license seat cycle calculation.</summary>
public interface IPrimaryLicenseSeatCycleStep
{
    string Describe();
    double Adjust(PrimaryLicenseSeatCycle item, double valueValue);
}

public sealed record FlaggedPrimaryLicenseSeatCycleAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryLicenseSeatCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryLicenseSeatCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryLicenseSeatCycleAdjuster(double Step) : IPrimaryLicenseSeatCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryLicenseSeatCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentPrimaryLicenseSeatCycleAdjuster(Dictionary<string, double> Factors) : IPrimaryLicenseSeatCycleStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PrimaryLicenseSeatCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryLicenseSeatCycleAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryLicenseSeatCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryLicenseSeatCycle item, double valueValue)
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

public sealed record PrimaryLicenseSeatCycleStartsAtOffsetAdjuster(double Weight) : IPrimaryLicenseSeatCycleStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(PrimaryLicenseSeatCycle item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

/// <summary>Runs the primary license seat cycle steps in order and keeps a trace of each.</summary>
public sealed class PrimaryLicenseSeatCycleEngine(List<IPrimaryLicenseSeatCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryLicenseSeatCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryLicenseSeatCycleEngine DefaultPrimaryLicenseSeatCycleEngine(Registry flags)
    {
        return new PrimaryLicenseSeatCycleEngine(
            new List<IPrimaryLicenseSeatCycleStep>
            {
                new FlaggedPrimaryLicenseSeatCycleAdjuster(flags, "primary-license-seat-cycle-fallback", 8.3),
                new RoundingPrimaryLicenseSeatCycleAdjuster(0.05),
                new DepartmentPrimaryLicenseSeatCycleAdjuster(new Dictionary<string, double> { ["south"] = 0.98 }),
                new TieredPrimaryLicenseSeatCycleAdjuster(
                    new List<double> { 7147.0, 7166.0, 9338.0 },
                    new List<double> { 1.02, 1.15, 1.0 }),
                new PrimaryLicenseSeatCycleStartsAtOffsetAdjuster(0.89),
            },
            new List<string>(),
            0.0,
            87711.3);
    }

    public double Price(PrimaryLicenseSeatCycle item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryLicenseSeatCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
