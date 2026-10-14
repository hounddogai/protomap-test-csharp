namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive adjustment notice is in its life cycle.</summary>
public enum AdaptiveAdjustmentNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive adjustment notice as the adjustment team keeps it.</summary>
public sealed class AdaptiveAdjustmentNotice(string code, string name, bool visible, string managerId, string channel, string checksum, int startsAt, AdaptiveAdjustmentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public string Checksum { get; set; } = checksum;
    public int StartsAt { get; set; } = startsAt;
    public AdaptiveAdjustmentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive adjustment notice in status open.</summary>
    public static AdaptiveAdjustmentNotice NewAdaptiveAdjustmentNotice(string code, string name)
    {
        return new AdaptiveAdjustmentNotice(code, name, false, "", "", "", 0, AdaptiveAdjustmentNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAdjustmentNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAdjustmentNoticeStatus next)
    {
        switch (Status)
        {
            case AdaptiveAdjustmentNoticeStatus.Open:
                switch (next)
                {
                    case AdaptiveAdjustmentNoticeStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveAdjustmentNoticeStatus.InReview:
                return next == AdaptiveAdjustmentNoticeStatus.Resolved || next == AdaptiveAdjustmentNoticeStatus.Archived;
            case AdaptiveAdjustmentNoticeStatus.Resolved:
                switch (next)
                {
                    case AdaptiveAdjustmentNoticeStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveAdjustmentNoticeStatus.Reopened:
                return next == AdaptiveAdjustmentNoticeStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveAdjustmentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive adjustment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public AdaptiveAdjustmentNotice Copy()
    {
        return new AdaptiveAdjustmentNotice(Code, Name, Visible, ManagerId, Channel, Checksum, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
