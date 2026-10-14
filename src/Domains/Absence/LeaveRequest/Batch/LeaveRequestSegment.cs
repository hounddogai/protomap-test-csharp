namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request segment is in its life cycle.</summary>
public enum LeaveRequestSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One leave request segment, with what the planners need to act on it.</summary>
public sealed class LeaveRequestSegment(string code, string title, double ratio, string skill, int capacity, double cost, string salaryBand, string team, string category, double score, string checksum, LeaveRequestSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Skill { get; set; } = skill;
    public int Capacity { get; set; } = capacity;
    public double Cost { get; set; } = cost;
    public string SalaryBand { get; set; } = salaryBand;
    public string Team { get; set; } = team;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public string Checksum { get; set; } = checksum;
    public LeaveRequestSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a leave request segment in status requested.</summary>
    public static LeaveRequestSegment NewLeaveRequestSegment(string code, string title)
    {
        return new LeaveRequestSegment(
            code,
            title,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            LeaveRequestSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestSegmentStatus next)
    {
        switch (Status)
        {
            case LeaveRequestSegmentStatus.Requested:
                return next == LeaveRequestSegmentStatus.Expired;
            case LeaveRequestSegmentStatus.Granted:
                return next == LeaveRequestSegmentStatus.Revoked;
            case LeaveRequestSegmentStatus.Revoked:
                return next == LeaveRequestSegmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LeaveRequestSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)Capacity - Cost, 2);
    }

    public LeaveRequestSegment Copy()
    {
        return new LeaveRequestSegment(
            Code,
            Title,
            Ratio,
            Skill,
            Capacity,
            Cost,
            SalaryBand,
            Team,
            Category,
            Score,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
