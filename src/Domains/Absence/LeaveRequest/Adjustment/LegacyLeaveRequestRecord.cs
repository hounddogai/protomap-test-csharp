namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy leave request record is in its life cycle.</summary>
public enum LegacyLeaveRequestRecordStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy leave request record and its current status.</summary>
public sealed class LegacyLeaveRequestRecord(string code, string name, string segment, int version, double ratio, int expiresAt, double score, LegacyLeaveRequestRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public double Ratio { get; set; } = ratio;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Score { get; set; } = score;
    public LegacyLeaveRequestRecordStatus Status { get; set; } = status;

    /// <summary>Starts a legacy leave request record in status new.</summary>
    public static LegacyLeaveRequestRecord NewLegacyLeaveRequestRecord(string code, string name)
    {
        return new LegacyLeaveRequestRecord(code, name, "", 0, 0.0, 0, 0.0, LegacyLeaveRequestRecordStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLeaveRequestRecordStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLeaveRequestRecordStatus next)
    {
        switch (Status)
        {
            case LegacyLeaveRequestRecordStatus.New:
                return next == LegacyLeaveRequestRecordStatus.Failed;
            case LegacyLeaveRequestRecordStatus.Queued:
                return next == LegacyLeaveRequestRecordStatus.Running || next == LegacyLeaveRequestRecordStatus.Failed;
            case LegacyLeaveRequestRecordStatus.Running:
                switch (next)
                {
                    case LegacyLeaveRequestRecordStatus.Done:
                        return true;
                }
                return false;
            case LegacyLeaveRequestRecordStatus.Done:
                switch (next)
                {
                    case LegacyLeaveRequestRecordStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyLeaveRequestRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy leave request record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"legacy leave request record"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Score - Ratio, 2);
    }

    public LegacyLeaveRequestRecord Copy()
    {
        return new LegacyLeaveRequestRecord(Code, Name, Segment, Version, Ratio, ExpiresAt, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
