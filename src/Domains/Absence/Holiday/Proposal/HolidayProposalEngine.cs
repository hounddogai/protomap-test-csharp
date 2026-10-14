namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday proposal calculation.</summary>
public interface IHolidayProposalStep
{
    string Describe();
    double Adjust(HolidayProposal item, double valueValue);
}

public sealed record HolidayProposalWindowDaysOffsetAdjuster(double Weight) : IHolidayProposalStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(HolidayProposal item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record RoundingHolidayProposalAdjuster(double Step) : IHolidayProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredHolidayProposalAdjuster(List<double> Bounds, List<double> Rates) : IHolidayProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayProposal item, double valueValue)
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

public sealed record MinimumHolidayProposalAdjuster(double Minimum) : IHolidayProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleHolidayProposalAdjuster(Dictionary<string, double> Factors) : IHolidayProposalStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(HolidayProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentHolidayProposalAdjuster(double Percent) : IHolidayProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the holiday proposal steps in order and keeps a trace of each.</summary>
public sealed class HolidayProposalEngine(List<IHolidayProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayProposalEngine DefaultHolidayProposalEngine()
    {
        return new HolidayProposalEngine(
            new List<IHolidayProposalStep>
            {
                new HolidayProposalWindowDaysOffsetAdjuster(1.13),
                new RoundingHolidayProposalAdjuster(1.0),
                new TieredHolidayProposalAdjuster(
                    new List<double> { 414.0, 1149.0 },
                    new List<double> { 0.95, 0.86 }),
                new MinimumHolidayProposalAdjuster(35.8),
                new TitleHolidayProposalAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.75, ["retail"] = 0.95 }),
                new PercentHolidayProposalAdjuster(3.6),
            },
            new List<string>(),
            2.9,
            66053.9);
    }

    public double Compute(HolidayProposal item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
