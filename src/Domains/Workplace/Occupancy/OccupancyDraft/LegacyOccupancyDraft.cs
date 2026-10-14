namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy occupancy draft is in its life cycle.</summary>
public enum LegacyOccupancyDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy occupancy draft. Its code is unique within a tenant.</summary>
public sealed class LegacyOccupancyDraft(string code, string title, string department, int tenureMonths, string summary, int sequence, string unit, string location, double weight, string employeeId, LegacyOccupancyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Summary { get; set; } = summary;
    public int Sequence { get; set; } = sequence;
    public string Unit { get; set; } = unit;
    public string Location { get; set; } = location;
    public double Weight { get; set; } = weight;
    public string EmployeeId { get; set; } = employeeId;
    public LegacyOccupancyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a legacy occupancy draft in status planned.</summary>
    public static LegacyOccupancyDraft NewLegacyOccupancyDraft(string code, string title)
    {
        return new LegacyOccupancyDraft(
            code,
            title,
            "",
            0,
            "",
            0,
            "",
            "",
            0.0,
            "",
            LegacyOccupancyDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOccupancyDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOccupancyDraftStatus next)
    {
        switch (Status)
        {
            case LegacyOccupancyDraftStatus.Planned:
                switch (next)
                {
                    case LegacyOccupancyDraftStatus.Scheduled:
                    case LegacyOccupancyDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyOccupancyDraftStatus.Scheduled:
                return next == LegacyOccupancyDraftStatus.Completed || next == LegacyOccupancyDraftStatus.Cancelled;
            case LegacyOccupancyDraftStatus.InProgress:
                return next == LegacyOccupancyDraftStatus.Completed || next == LegacyOccupancyDraftStatus.Cancelled;
            case LegacyOccupancyDraftStatus.Completed:
                switch (next)
                {
                    case LegacyOccupancyDraftStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyOccupancyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy occupancy draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Sequence + (double)TenureMonths, 2);
    }

    public LegacyOccupancyDraft Copy()
    {
        return new LegacyOccupancyDraft(
            Code,
            Title,
            Department,
            TenureMonths,
            Summary,
            Sequence,
            Unit,
            Location,
            Weight,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
