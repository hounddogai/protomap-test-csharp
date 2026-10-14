namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal garnishment allocation is in its life cycle.</summary>
public enum SeasonalGarnishmentAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One seasonal garnishment allocation, with what the planners need to act on it.</summary>
public sealed class SeasonalGarnishmentAllocation(string code, string title, string unit, string grade, string department, string currency, double threshold, bool active, string costCenter, int attempts, int graceDays, SeasonalGarnishmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public string Grade { get; set; } = grade;
    public string Department { get; set; } = department;
    public string Currency { get; set; } = currency;
    public double Threshold { get; set; } = threshold;
    public bool Active { get; set; } = active;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public SeasonalGarnishmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal garnishment allocation in status planned.</summary>
    public static SeasonalGarnishmentAllocation NewSeasonalGarnishmentAllocation(string code, string title)
    {
        return new SeasonalGarnishmentAllocation(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            false,
            "",
            0,
            0,
            SeasonalGarnishmentAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalGarnishmentAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalGarnishmentAllocationStatus next)
    {
        switch (Status)
        {
            case SeasonalGarnishmentAllocationStatus.Planned:
                return next == SeasonalGarnishmentAllocationStatus.Scheduled || next == SeasonalGarnishmentAllocationStatus.Completed;
            case SeasonalGarnishmentAllocationStatus.Scheduled:
                switch (next)
                {
                    case SeasonalGarnishmentAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalGarnishmentAllocationStatus.InProgress:
                switch (next)
                {
                    case SeasonalGarnishmentAllocationStatus.Completed:
                        return true;
                }
                return false;
            case SeasonalGarnishmentAllocationStatus.Completed:
                switch (next)
                {
                    case SeasonalGarnishmentAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalGarnishmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal garnishment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round((double)GraceDays - (double)Attempts, 2);
    }

    public SeasonalGarnishmentAllocation Copy()
    {
        return new SeasonalGarnishmentAllocation(
            Code,
            Title,
            Unit,
            Grade,
            Department,
            Currency,
            Threshold,
            Active,
            CostCenter,
            Attempts,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
