namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment mapping is in its life cycle.</summary>
public enum AdjustmentMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adjustment mapping as the adjustment team keeps it.</summary>
public sealed class AdjustmentMapping(string code, string title, double minimum, string department, int tenureMonths, double ratio, string region, string employeeId, double hours, string grade, string contactEmail, AdjustmentMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ratio { get; set; } = ratio;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public double Hours { get; set; } = hours;
    public string Grade { get; set; } = grade;
    public string ContactEmail { get; set; } = contactEmail;
    public AdjustmentMappingStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment mapping in status planned.</summary>
    public static AdjustmentMapping NewAdjustmentMapping(string code, string title)
    {
        return new AdjustmentMapping(
            code,
            title,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            AdjustmentMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentMappingStatus next)
    {
        switch (Status)
        {
            case AdjustmentMappingStatus.Planned:
                return next == AdjustmentMappingStatus.Scheduled || next == AdjustmentMappingStatus.Completed;
            case AdjustmentMappingStatus.Scheduled:
                switch (next)
                {
                    case AdjustmentMappingStatus.Completed:
                        return true;
                }
                return false;
            case AdjustmentMappingStatus.InProgress:
                return next == AdjustmentMappingStatus.Completed || next == AdjustmentMappingStatus.Cancelled;
            case AdjustmentMappingStatus.Completed:
                switch (next)
                {
                    case AdjustmentMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Hours - Minimum, 2);
    }

    public AdjustmentMapping Copy()
    {
        return new AdjustmentMapping(
            Code,
            Title,
            Minimum,
            Department,
            TenureMonths,
            Ratio,
            Region,
            EmployeeId,
            Hours,
            Grade,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
