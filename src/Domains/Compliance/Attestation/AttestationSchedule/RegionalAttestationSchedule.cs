namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional attestation schedule is in its life cycle.</summary>
public enum RegionalAttestationScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One regional attestation schedule, with what the planners need to act on it.</summary>
public sealed class RegionalAttestationSchedule(string code, string name, double maximum, string managerId, string title, bool active, string department, RegionalAttestationScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string ManagerId { get; set; } = managerId;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string Department { get; set; } = department;
    public RegionalAttestationScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a regional attestation schedule in status requested.</summary>
    public static RegionalAttestationSchedule NewRegionalAttestationSchedule(string code, string name)
    {
        return new RegionalAttestationSchedule(
            code,
            name,
            0.0,
            "",
            "",
            false,
            "",
            RegionalAttestationScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAttestationScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAttestationScheduleStatus next)
    {
        switch (Status)
        {
            case RegionalAttestationScheduleStatus.Requested:
                return next == RegionalAttestationScheduleStatus.Granted;
            case RegionalAttestationScheduleStatus.Granted:
                switch (next)
                {
                    case RegionalAttestationScheduleStatus.Expired:
                        return true;
                }
                return false;
            case RegionalAttestationScheduleStatus.Revoked:
                return next == RegionalAttestationScheduleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalAttestationScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional attestation schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public RegionalAttestationSchedule Copy()
    {
        return new RegionalAttestationSchedule(Code, Name, Maximum, ManagerId, Title, Active, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
