namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat schedule is in its life cycle.</summary>
public enum LicenseSeatScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A license seat schedule as the license seat team keeps it.</summary>
public sealed class LicenseSeatSchedule(string code, string name, string salaryBand, int leaveDays, string ownerId, string description, string employeeId, double ratio, LicenseSeatScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public string Description { get; set; } = description;
    public string EmployeeId { get; set; } = employeeId;
    public double Ratio { get; set; } = ratio;
    public LicenseSeatScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a license seat schedule in status proposed.</summary>
    public static LicenseSeatSchedule NewLicenseSeatSchedule(string code, string name)
    {
        return new LicenseSeatSchedule(code, name, "", 0, "", "", "", 0.0, LicenseSeatScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatScheduleStatus next)
    {
        switch (Status)
        {
            case LicenseSeatScheduleStatus.Proposed:
                switch (next)
                {
                    case LicenseSeatScheduleStatus.Approved:
                    case LicenseSeatScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case LicenseSeatScheduleStatus.Approved:
                return next == LicenseSeatScheduleStatus.Rejected;
            case LicenseSeatScheduleStatus.Rejected:
                switch (next)
                {
                    case LicenseSeatScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"license seat schedule"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ratio * (double)LeaveDays, 2);
    }

    public LicenseSeatSchedule Copy()
    {
        return new LicenseSeatSchedule(
            Code,
            Name,
            SalaryBand,
            LeaveDays,
            OwnerId,
            Description,
            EmployeeId,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
