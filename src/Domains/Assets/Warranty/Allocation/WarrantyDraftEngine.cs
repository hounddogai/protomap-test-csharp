namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty draft calculation.</summary>
public interface IWarrantyDraftFactor
{
    string Describe();
    double Adjust(WarrantyDraft item, double valueValue);
}

public sealed record PercentWarrantyDraftAdjuster(double Percent) : IWarrantyDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWarrantyDraftAdjuster(double Minimum) : IWarrantyDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWarrantyDraftAdjuster(double Step) : IWarrantyDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedWarrantyDraftAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WarrantyDraftTenureMonthsOffsetAdjuster(double Weight) : IWarrantyDraftFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(WarrantyDraft item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the warranty draft steps in order and keeps a trace of each.</summary>
public sealed class WarrantyDraftEngine(List<IWarrantyDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyDraftEngine DefaultWarrantyDraftEngine(Registry flags)
    {
        return new WarrantyDraftEngine(
            new List<IWarrantyDraftFactor>
            {
                new PercentWarrantyDraftAdjuster(-24.3),
                new MinimumWarrantyDraftAdjuster(31.9),
                new RoundingWarrantyDraftAdjuster(0.5),
                new FlaggedWarrantyDraftAdjuster(flags, "warranty-draft-seasonal", 3.4),
                new WarrantyDraftTenureMonthsOffsetAdjuster(1.91),
            },
            new List<string>(),
            1.3,
            42275.8);
    }

    public double Price(WarrantyDraft item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
