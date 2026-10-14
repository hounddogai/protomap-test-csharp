namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary bonus pool notice is in its life cycle.</summary>
public enum PrimaryBonusPoolNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A primary bonus pool notice. Its code is unique within a tenant.</summary>
public sealed class PrimaryBonusPoolNotice(string code, string label, int capacity, string title, bool active, PrimaryBonusPoolNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public PrimaryBonusPoolNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a primary bonus pool notice in status proposed.</summary>
    public static PrimaryBonusPoolNotice NewPrimaryBonusPoolNotice(string code, string label)
    {
        return new PrimaryBonusPoolNotice(code, label, 0, "", false, PrimaryBonusPoolNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBonusPoolNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBonusPoolNoticeStatus next)
    {
        switch (Status)
        {
            case PrimaryBonusPoolNoticeStatus.Proposed:
                switch (next)
                {
                    case PrimaryBonusPoolNoticeStatus.Rejected:
                    case PrimaryBonusPoolNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case PrimaryBonusPoolNoticeStatus.Approved:
                return next == PrimaryBonusPoolNoticeStatus.Withdrawn;
            case PrimaryBonusPoolNoticeStatus.Rejected:
                switch (next)
                {
                    case PrimaryBonusPoolNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryBonusPoolNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary bonus pool notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public PrimaryBonusPoolNotice Copy()
    {
        return new PrimaryBonusPoolNotice(Code, Label, Capacity, Title, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
