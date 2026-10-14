namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request batch is in its life cycle.</summary>
public enum LeaveRequestBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A leave request batch. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestBatch(string code, string name, int startsAt, string source, string grade, int capacity, string unit, int expiresAt, double rate, string category, LeaveRequestBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string Source { get; set; } = source;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Rate { get; set; } = rate;
    public string Category { get; set; } = category;
    public LeaveRequestBatchStatus Status { get; set; } = status;

    /// <summary>Starts a leave request batch in status draft.</summary>
    public static LeaveRequestBatch NewLeaveRequestBatch(string code, string name)
    {
        return new LeaveRequestBatch(code, name, 0, "", "", 0, "", 0, 0.0, "", LeaveRequestBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestBatchStatus next)
    {
        switch (Status)
        {
            case LeaveRequestBatchStatus.Draft:
                return next == LeaveRequestBatchStatus.Pending || next == LeaveRequestBatchStatus.Closed;
            case LeaveRequestBatchStatus.Pending:
                switch (next)
                {
                    case LeaveRequestBatchStatus.Suspended:
                    case LeaveRequestBatchStatus.Closed:
                        return true;
                }
                return false;
            case LeaveRequestBatchStatus.Active:
                return next == LeaveRequestBatchStatus.Suspended || next == LeaveRequestBatchStatus.Closed;
            case LeaveRequestBatchStatus.Suspended:
                return next == LeaveRequestBatchStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LeaveRequestBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Capacity - Rate, 2);
    }

    public LeaveRequestBatch Copy()
    {
        return new LeaveRequestBatch(
            Code,
            Name,
            StartsAt,
            Source,
            Grade,
            Capacity,
            Unit,
            ExpiresAt,
            Rate,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
