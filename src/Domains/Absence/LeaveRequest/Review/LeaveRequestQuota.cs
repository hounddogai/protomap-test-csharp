namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request quota is in its life cycle.</summary>
public enum LeaveRequestQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One leave request quota, with what the planners need to act on it.</summary>
public sealed class LeaveRequestQuota(string code, string name, string summary, int revision, bool pinned, string grade, double floorValue, LeaveRequestQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public string Grade { get; set; } = grade;
    public double FloorValue { get; set; } = floorValue;
    public LeaveRequestQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a leave request quota in status created.</summary>
    public static LeaveRequestQuota NewLeaveRequestQuota(string code, string name)
    {
        return new LeaveRequestQuota(code, name, "", 0, false, "", 0.0, LeaveRequestQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestQuotaStatus next)
    {
        switch (Status)
        {
            case LeaveRequestQuotaStatus.Created:
                return next == LeaveRequestQuotaStatus.Retired;
            case LeaveRequestQuotaStatus.Validated:
                return next == LeaveRequestQuotaStatus.Published || next == LeaveRequestQuotaStatus.Retired;
            case LeaveRequestQuotaStatus.Published:
                return next == LeaveRequestQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LeaveRequestQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(FloorValue + (double)Revision, 2);
    }

    public LeaveRequestQuota Copy()
    {
        return new LeaveRequestQuota(Code, Name, Summary, Revision, Pinned, Grade, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
