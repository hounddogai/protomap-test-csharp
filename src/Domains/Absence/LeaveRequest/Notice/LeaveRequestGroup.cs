namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request group is in its life cycle.</summary>
public enum LeaveRequestGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A leave request group. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestGroup(string code, string name, int sequence, double ratio, bool pinned, int updatedAt, int limit, LeaveRequestGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public double Ratio { get; set; } = ratio;
    public bool Pinned { get; set; } = pinned;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Limit { get; set; } = limit;
    public LeaveRequestGroupStatus Status { get; set; } = status;

    /// <summary>Starts a leave request group in status created.</summary>
    public static LeaveRequestGroup NewLeaveRequestGroup(string code, string name)
    {
        return new LeaveRequestGroup(code, name, 0, 0.0, false, 0, 0, LeaveRequestGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestGroupStatus next)
    {
        switch (Status)
        {
            case LeaveRequestGroupStatus.Created:
                return next == LeaveRequestGroupStatus.Published;
            case LeaveRequestGroupStatus.Validated:
                return next == LeaveRequestGroupStatus.Published || next == LeaveRequestGroupStatus.Retired;
            case LeaveRequestGroupStatus.Published:
                return next == LeaveRequestGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LeaveRequestGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"leave request group"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Limit * (double)Sequence, 2);
    }

    public LeaveRequestGroup Copy()
    {
        return new LeaveRequestGroup(Code, Name, Sequence, Ratio, Pinned, UpdatedAt, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
