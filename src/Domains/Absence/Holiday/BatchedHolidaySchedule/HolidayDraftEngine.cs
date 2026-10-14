namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday draft calculation.</summary>
public interface IHolidayDraftModifier
{
    string Describe();
    double Adjust(HolidayDraft item, double valueValue);
}

public sealed record PercentHolidayDraftAdjuster(double Percent) : IHolidayDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumHolidayDraftAdjuster(double Minimum) : IHolidayDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedHolidayDraftAdjuster(Registry Flags, string Flag, double Percent) : IHolidayDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record HolidayDraftUpdatedAtOffsetAdjuster(double Weight) : IHolidayDraftModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(HolidayDraft item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record RoundingHolidayDraftAdjuster(double Step) : IHolidayDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the holiday draft steps in order and keeps a trace of each.</summary>
public sealed class HolidayDraftEngine(List<IHolidayDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayDraftEngine DefaultHolidayDraftEngine(Registry flags)
    {
        return new HolidayDraftEngine(
            new List<IHolidayDraftModifier>
            {
                new PercentHolidayDraftAdjuster(19.5),
                new MinimumHolidayDraftAdjuster(31.6),
                new FlaggedHolidayDraftAdjuster(flags, "holiday-draft-global", 4.1),
                new HolidayDraftUpdatedAtOffsetAdjuster(1.09),
                new RoundingHolidayDraftAdjuster(0.05),
            },
            new List<string>(),
            4.5,
            69838.7);
    }

    public double Evaluate(HolidayDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<HolidayDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
