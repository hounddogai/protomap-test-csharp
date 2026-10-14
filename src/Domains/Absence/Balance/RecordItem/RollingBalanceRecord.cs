namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling balance record is in its life cycle.</summary>
public enum RollingBalanceRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a rolling balance record and its current status.</summary>
public sealed class RollingBalanceRecord(string code, string label, string source, int updatedAt, string summary, int sequence, string category, string locale, RollingBalanceRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Summary { get; set; } = summary;
    public int Sequence { get; set; } = sequence;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public RollingBalanceRecordStatus Status { get; set; } = status;

    /// <summary>Starts a rolling balance record in status idle.</summary>
    public static RollingBalanceRecord NewRollingBalanceRecord(string code, string label)
    {
        return new RollingBalanceRecord(code, label, "", 0, "", 0, "", "", RollingBalanceRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBalanceRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBalanceRecordStatus next)
    {
        switch (Status)
        {
            case RollingBalanceRecordStatus.Idle:
                switch (next)
                {
                    case RollingBalanceRecordStatus.Ready:
                    case RollingBalanceRecordStatus.Draining:
                        return true;
                }
                return false;
            case RollingBalanceRecordStatus.Warming:
                switch (next)
                {
                    case RollingBalanceRecordStatus.Draining:
                    case RollingBalanceRecordStatus.Stopped:
                        return true;
                }
                return false;
            case RollingBalanceRecordStatus.Ready:
                return next == RollingBalanceRecordStatus.Draining || next == RollingBalanceRecordStatus.Stopped;
            case RollingBalanceRecordStatus.Draining:
                switch (next)
                {
                    case RollingBalanceRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingBalanceRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling balance record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Sequence * (double)UpdatedAt, 2);
    }

    public RollingBalanceRecord Copy()
    {
        return new RollingBalanceRecord(Code, Label, Source, UpdatedAt, Summary, Sequence, Category, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
