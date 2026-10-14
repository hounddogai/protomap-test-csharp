namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay run record is in its life cycle.</summary>
public enum LegacyPayRunRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy pay run record as the pay run team keeps it.</summary>
public sealed class LegacyPayRunRecord(string code, string name, int endsAt, int expiresAt, string jobCode, LegacyPayRunRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public string JobCode { get; set; } = jobCode;
    public LegacyPayRunRecordStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay run record in status open.</summary>
    public static LegacyPayRunRecord NewLegacyPayRunRecord(string code, string name)
    {
        return new LegacyPayRunRecord(code, name, 0, 0, "", LegacyPayRunRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayRunRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayRunRecordStatus next)
    {
        switch (Status)
        {
            case LegacyPayRunRecordStatus.Open:
                return next == LegacyPayRunRecordStatus.Reopened || next == LegacyPayRunRecordStatus.Archived;
            case LegacyPayRunRecordStatus.InReview:
                switch (next)
                {
                    case LegacyPayRunRecordStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyPayRunRecordStatus.Resolved:
                switch (next)
                {
                    case LegacyPayRunRecordStatus.Reopened:
                    case LegacyPayRunRecordStatus.Archived:
                        return true;
                }
                return false;
            case LegacyPayRunRecordStatus.Reopened:
                switch (next)
                {
                    case LegacyPayRunRecordStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayRunRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay run record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)EndsAt * (double)ExpiresAt, 2);
    }

    public LegacyPayRunRecord Copy()
    {
        return new LegacyPayRunRecord(Code, Name, EndsAt, ExpiresAt, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
