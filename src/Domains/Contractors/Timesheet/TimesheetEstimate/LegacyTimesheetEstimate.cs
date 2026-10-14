namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy timesheet estimate is in its life cycle.</summary>
public enum LegacyTimesheetEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One legacy timesheet estimate, with what the planners need to act on it.</summary>
public sealed class LegacyTimesheetEstimate(string code, string title, double hours, string notes, string country, double cost, int sequence, double discount, int startsAt, bool archived, LegacyTimesheetEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Notes { get; set; } = notes;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public double Discount { get; set; } = discount;
    public int StartsAt { get; set; } = startsAt;
    public bool Archived { get; set; } = archived;
    public LegacyTimesheetEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a legacy timesheet estimate in status planned.</summary>
    public static LegacyTimesheetEstimate NewLegacyTimesheetEstimate(string code, string title)
    {
        return new LegacyTimesheetEstimate(
            code,
            title,
            0.0,
            "",
            "",
            0.0,
            0,
            0.0,
            0,
            false,
            LegacyTimesheetEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTimesheetEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTimesheetEstimateStatus next)
    {
        switch (Status)
        {
            case LegacyTimesheetEstimateStatus.Planned:
                switch (next)
                {
                    case LegacyTimesheetEstimateStatus.InProgress:
                    case LegacyTimesheetEstimateStatus.Completed:
                        return true;
                }
                return false;
            case LegacyTimesheetEstimateStatus.Scheduled:
                switch (next)
                {
                    case LegacyTimesheetEstimateStatus.InProgress:
                        return true;
                }
                return false;
            case LegacyTimesheetEstimateStatus.InProgress:
                switch (next)
                {
                    case LegacyTimesheetEstimateStatus.Completed:
                    case LegacyTimesheetEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyTimesheetEstimateStatus.Completed:
                switch (next)
                {
                    case LegacyTimesheetEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTimesheetEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy timesheet estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy timesheet estimate"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt * Cost, 2);
    }

    public LegacyTimesheetEstimate Copy()
    {
        return new LegacyTimesheetEstimate(
            Code,
            Title,
            Hours,
            Notes,
            Country,
            Cost,
            Sequence,
            Discount,
            StartsAt,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
