namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request mapping is in its life cycle.</summary>
public enum LeaveRequestMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A leave request mapping as the leave request team keeps it.</summary>
public sealed class LeaveRequestMapping(string code, string title, int priority, double discount, string label, int expiresAt, double ratio, LeaveRequestMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public double Discount { get; set; } = discount;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ratio { get; set; } = ratio;
    public LeaveRequestMappingStatus Status { get; set; } = status;

    /// <summary>Starts a leave request mapping in status requested.</summary>
    public static LeaveRequestMapping NewLeaveRequestMapping(string code, string title)
    {
        return new LeaveRequestMapping(code, title, 0, 0.0, "", 0, 0.0, LeaveRequestMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestMappingStatus next)
    {
        switch (Status)
        {
            case LeaveRequestMappingStatus.Requested:
                return next == LeaveRequestMappingStatus.Revoked || next == LeaveRequestMappingStatus.Expired;
            case LeaveRequestMappingStatus.Granted:
                return next == LeaveRequestMappingStatus.Revoked;
            case LeaveRequestMappingStatus.Revoked:
                switch (next)
                {
                    case LeaveRequestMappingStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ratio + Discount, 2);
    }

    public LeaveRequestMapping Copy()
    {
        return new LeaveRequestMapping(Code, Title, Priority, Discount, Label, ExpiresAt, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
