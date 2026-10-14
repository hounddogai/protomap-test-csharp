namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction schedule is in its life cycle.</summary>
public enum DeductionScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One deduction schedule, with what the planners need to act on it.</summary>
public sealed class DeductionSchedule(string code, string title, string ownerId, bool pinned, double discount, int windowDays, bool visible, string checksum, int leaveDays, DeductionScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public bool Pinned { get; set; } = pinned;
    public double Discount { get; set; } = discount;
    public int WindowDays { get; set; } = windowDays;
    public bool Visible { get; set; } = visible;
    public string Checksum { get; set; } = checksum;
    public int LeaveDays { get; set; } = leaveDays;
    public DeductionScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a deduction schedule in status proposed.</summary>
    public static DeductionSchedule NewDeductionSchedule(string code, string title)
    {
        return new DeductionSchedule(code, title, "", false, 0.0, 0, false, "", 0, DeductionScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionScheduleStatus next)
    {
        switch (Status)
        {
            case DeductionScheduleStatus.Proposed:
                switch (next)
                {
                    case DeductionScheduleStatus.Approved:
                        return true;
                }
                return false;
            case DeductionScheduleStatus.Approved:
                switch (next)
                {
                    case DeductionScheduleStatus.Rejected:
                        return true;
                }
                return false;
            case DeductionScheduleStatus.Rejected:
                return next == DeductionScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeductionScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Discount + (double)LeaveDays, 2);
    }

    public DeductionSchedule Copy()
    {
        return new DeductionSchedule(
            Code,
            Title,
            OwnerId,
            Pinned,
            Discount,
            WindowDays,
            Visible,
            Checksum,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
