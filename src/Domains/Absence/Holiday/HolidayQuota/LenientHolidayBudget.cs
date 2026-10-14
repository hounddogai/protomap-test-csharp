namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient holiday budget is in its life cycle.</summary>
public enum LenientHolidayBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient holiday budget as the holiday team keeps it.</summary>
public sealed class LenientHolidayBudget(string code, string label, double ceiling, int leaveDays, int batchSize, string unit, string timezone, double cost, string region, double budget, LenientHolidayBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public string Unit { get; set; } = unit;
    public string Timezone { get; set; } = timezone;
    public double Cost { get; set; } = cost;
    public string Region { get; set; } = region;
    public double Budget { get; set; } = budget;
    public LenientHolidayBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a lenient holiday budget in status created.</summary>
    public static LenientHolidayBudget NewLenientHolidayBudget(string code, string label)
    {
        return new LenientHolidayBudget(
            code,
            label,
            0.0,
            0,
            0,
            "",
            "",
            0.0,
            "",
            0.0,
            LenientHolidayBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientHolidayBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientHolidayBudgetStatus next)
    {
        switch (Status)
        {
            case LenientHolidayBudgetStatus.Created:
                return next == LenientHolidayBudgetStatus.Published || next == LenientHolidayBudgetStatus.Retired;
            case LenientHolidayBudgetStatus.Validated:
                return next == LenientHolidayBudgetStatus.Published || next == LenientHolidayBudgetStatus.Retired;
            case LenientHolidayBudgetStatus.Published:
                return next == LenientHolidayBudgetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientHolidayBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient holiday budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient holiday budget"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)LeaveDays + Cost, 2);
    }

    public LenientHolidayBudget Copy()
    {
        return new LenientHolidayBudget(
            Code,
            Label,
            Ceiling,
            LeaveDays,
            BatchSize,
            Unit,
            Timezone,
            Cost,
            Region,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
