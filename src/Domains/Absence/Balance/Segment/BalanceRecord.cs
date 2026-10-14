namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance record is in its life cycle.</summary>
public enum BalanceRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A balance record. Its code is unique within a tenant.</summary>
public sealed class BalanceRecord(string code, string name, string employeeId, int startsAt, bool archived, string channel, int durationMinutes, double score, BalanceRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public bool Archived { get; set; } = archived;
    public string Channel { get; set; } = channel;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Score { get; set; } = score;
    public BalanceRecordStatus Status { get; set; } = status;

    /// <summary>Starts a balance record in status idle.</summary>
    public static BalanceRecord NewBalanceRecord(string code, string name)
    {
        return new BalanceRecord(code, name, "", 0, false, "", 0, 0.0, BalanceRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceRecordStatus next)
    {
        switch (Status)
        {
            case BalanceRecordStatus.Idle:
                switch (next)
                {
                    case BalanceRecordStatus.Warming:
                    case BalanceRecordStatus.Ready:
                        return true;
                }
                return false;
            case BalanceRecordStatus.Warming:
                switch (next)
                {
                    case BalanceRecordStatus.Ready:
                        return true;
                }
                return false;
            case BalanceRecordStatus.Ready:
                switch (next)
                {
                    case BalanceRecordStatus.Draining:
                    case BalanceRecordStatus.Stopped:
                        return true;
                }
                return false;
            case BalanceRecordStatus.Draining:
                return next == BalanceRecordStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BalanceRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"balance record"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)DurationMinutes * (double)StartsAt, 2);
    }

    public BalanceRecord Copy()
    {
        return new BalanceRecord(
            Code,
            Name,
            EmployeeId,
            StartsAt,
            Archived,
            Channel,
            DurationMinutes,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
