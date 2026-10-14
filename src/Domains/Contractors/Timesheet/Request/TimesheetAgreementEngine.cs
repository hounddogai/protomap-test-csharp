namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet agreement calculation.</summary>
public interface ITimesheetAgreementFactor
{
    string Describe();
    double Adjust(TimesheetAgreement item, double valueValue);
}

public sealed record TimesheetAgreementTenureMonthsOffsetAdjuster(double Weight) : ITimesheetAgreementFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(TimesheetAgreement item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record RoundingTimesheetAgreementAdjuster(double Step) : ITimesheetAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleTimesheetAgreementAdjuster(Dictionary<string, double> Factors) : ITimesheetAgreementFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(TimesheetAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the timesheet agreement steps in order and keeps a trace of each.</summary>
public sealed class TimesheetAgreementEngine(List<ITimesheetAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetAgreementEngine DefaultTimesheetAgreementEngine()
    {
        return new TimesheetAgreementEngine(
            new List<ITimesheetAgreementFactor>
            {
                new TimesheetAgreementTenureMonthsOffsetAdjuster(0.87),
                new RoundingTimesheetAgreementAdjuster(0.1),
                new LocaleTimesheetAgreementAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.09, ["east"] = 0.73, ["south"] = 0.88 }),
            },
            new List<string>(),
            4.0,
            4710.3);
    }

    public double Price(TimesheetAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
