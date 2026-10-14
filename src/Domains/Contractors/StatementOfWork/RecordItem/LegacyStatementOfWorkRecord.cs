namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy statement of work record is in its life cycle.</summary>
public enum LegacyStatementOfWorkRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One legacy statement of work record, with what the planners need to act on it.</summary>
public sealed class LegacyStatementOfWorkRecord(string code, string name, string externalRef, bool archived, int updatedAt, double ratio, string department, string jobCode, int leaveDays, double minimum, LegacyStatementOfWorkRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public bool Archived { get; set; } = archived;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public string Department { get; set; } = department;
    public string JobCode { get; set; } = jobCode;
    public int LeaveDays { get; set; } = leaveDays;
    public double Minimum { get; set; } = minimum;
    public LegacyStatementOfWorkRecordStatus Status { get; set; } = status;

    /// <summary>Starts a legacy statement of work record in status idle.</summary>
    public static LegacyStatementOfWorkRecord NewLegacyStatementOfWorkRecord(string code, string name)
    {
        return new LegacyStatementOfWorkRecord(
            code,
            name,
            "",
            false,
            0,
            0.0,
            "",
            "",
            0,
            0.0,
            LegacyStatementOfWorkRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyStatementOfWorkRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyStatementOfWorkRecordStatus next)
    {
        switch (Status)
        {
            case LegacyStatementOfWorkRecordStatus.Idle:
                switch (next)
                {
                    case LegacyStatementOfWorkRecordStatus.Draining:
                    case LegacyStatementOfWorkRecordStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyStatementOfWorkRecordStatus.Warming:
                return next == LegacyStatementOfWorkRecordStatus.Ready;
            case LegacyStatementOfWorkRecordStatus.Ready:
                switch (next)
                {
                    case LegacyStatementOfWorkRecordStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyStatementOfWorkRecordStatus.Draining:
                switch (next)
                {
                    case LegacyStatementOfWorkRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyStatementOfWorkRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy statement of work record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)LeaveDays * Minimum, 2);
    }

    public LegacyStatementOfWorkRecord Copy()
    {
        return new LegacyStatementOfWorkRecord(
            Code,
            Name,
            ExternalRef,
            Archived,
            UpdatedAt,
            Ratio,
            Department,
            JobCode,
            LeaveDays,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
