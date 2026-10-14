namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment notice is in its life cycle.</summary>
public enum AdjustmentNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adjustment notice and its current status.</summary>
public sealed class AdjustmentNotice(string code, string label, string skill, string currency, int batchSize, string externalRef, int version, AdjustmentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string Currency { get; set; } = currency;
    public int BatchSize { get; set; } = batchSize;
    public string ExternalRef { get; set; } = externalRef;
    public int Version { get; set; } = version;
    public AdjustmentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment notice in status new.</summary>
    public static AdjustmentNotice NewAdjustmentNotice(string code, string label)
    {
        return new AdjustmentNotice(code, label, "", "", 0, "", 0, AdjustmentNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentNoticeStatus next)
    {
        switch (Status)
        {
            case AdjustmentNoticeStatus.New:
                switch (next)
                {
                    case AdjustmentNoticeStatus.Done:
                        return true;
                }
                return false;
            case AdjustmentNoticeStatus.Queued:
                switch (next)
                {
                    case AdjustmentNoticeStatus.Done:
                    case AdjustmentNoticeStatus.Failed:
                        return true;
                }
                return false;
            case AdjustmentNoticeStatus.Running:
                switch (next)
                {
                    case AdjustmentNoticeStatus.Done:
                    case AdjustmentNoticeStatus.Failed:
                        return true;
                }
                return false;
            case AdjustmentNoticeStatus.Done:
                switch (next)
                {
                    case AdjustmentNoticeStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize * (double)Version, 2);
    }

    public AdjustmentNotice Copy()
    {
        return new AdjustmentNotice(Code, Label, Skill, Currency, BatchSize, ExternalRef, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
