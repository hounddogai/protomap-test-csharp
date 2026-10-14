namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling adjustment notice is in its life cycle.</summary>
public enum RollingAdjustmentNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling adjustment notice. Its code is unique within a tenant.</summary>
public sealed class RollingAdjustmentNotice(string code, string title, double margin, string managerId, int priority, RollingAdjustmentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public string ManagerId { get; set; } = managerId;
    public int Priority { get; set; } = priority;
    public RollingAdjustmentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a rolling adjustment notice in status idle.</summary>
    public static RollingAdjustmentNotice NewRollingAdjustmentNotice(string code, string title)
    {
        return new RollingAdjustmentNotice(code, title, 0.0, "", 0, RollingAdjustmentNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAdjustmentNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAdjustmentNoticeStatus next)
    {
        switch (Status)
        {
            case RollingAdjustmentNoticeStatus.Idle:
                return next == RollingAdjustmentNoticeStatus.Ready || next == RollingAdjustmentNoticeStatus.Draining;
            case RollingAdjustmentNoticeStatus.Warming:
                switch (next)
                {
                    case RollingAdjustmentNoticeStatus.Draining:
                    case RollingAdjustmentNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case RollingAdjustmentNoticeStatus.Ready:
                switch (next)
                {
                    case RollingAdjustmentNoticeStatus.Draining:
                        return true;
                }
                return false;
            case RollingAdjustmentNoticeStatus.Draining:
                return next == RollingAdjustmentNoticeStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingAdjustmentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling adjustment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Margin + (double)Priority, 2);
    }

    public RollingAdjustmentNotice Copy()
    {
        return new RollingAdjustmentNotice(Code, Title, Margin, ManagerId, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
