namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request ledger is in its life cycle.</summary>
public enum LeaveRequestLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A leave request ledger. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestLedger(string code, string name, int attempts, string grade, string label, string team, int tenureMonths, string summary, string segment, int expiresAt, LeaveRequestLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Grade { get; set; } = grade;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Summary { get; set; } = summary;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public LeaveRequestLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a leave request ledger in status open.</summary>
    public static LeaveRequestLedger NewLeaveRequestLedger(string code, string name)
    {
        return new LeaveRequestLedger(code, name, 0, "", "", "", 0, "", "", 0, LeaveRequestLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestLedgerStatus next)
    {
        switch (Status)
        {
            case LeaveRequestLedgerStatus.Open:
                return next == LeaveRequestLedgerStatus.InReview || next == LeaveRequestLedgerStatus.Reopened;
            case LeaveRequestLedgerStatus.InReview:
                switch (next)
                {
                    case LeaveRequestLedgerStatus.Resolved:
                    case LeaveRequestLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case LeaveRequestLedgerStatus.Resolved:
                return next == LeaveRequestLedgerStatus.Reopened || next == LeaveRequestLedgerStatus.Archived;
            case LeaveRequestLedgerStatus.Reopened:
                return next == LeaveRequestLedgerStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LeaveRequestLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)ExpiresAt * (double)Attempts, 2);
    }

    public LeaveRequestLedger Copy()
    {
        return new LeaveRequestLedger(
            Code,
            Name,
            Attempts,
            Grade,
            Label,
            Team,
            TenureMonths,
            Summary,
            Segment,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
