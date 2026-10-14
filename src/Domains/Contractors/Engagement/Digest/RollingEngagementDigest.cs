namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling engagement digest is in its life cycle.</summary>
public enum RollingEngagementDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling engagement digest. Its code is unique within a tenant.</summary>
public sealed class RollingEngagementDigest(string code, string label, int retries, string badge, double fte, string costCenter, bool active, RollingEngagementDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public bool Active { get; set; } = active;
    public RollingEngagementDigestStatus Status { get; set; } = status;

    /// <summary>Starts a rolling engagement digest in status new.</summary>
    public static RollingEngagementDigest NewRollingEngagementDigest(string code, string label)
    {
        return new RollingEngagementDigest(code, label, 0, "", 0.0, "", false, RollingEngagementDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingEngagementDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingEngagementDigestStatus next)
    {
        switch (Status)
        {
            case RollingEngagementDigestStatus.New:
                return next == RollingEngagementDigestStatus.Running || next == RollingEngagementDigestStatus.Done;
            case RollingEngagementDigestStatus.Queued:
                return next == RollingEngagementDigestStatus.Running;
            case RollingEngagementDigestStatus.Running:
                return next == RollingEngagementDigestStatus.Done;
            case RollingEngagementDigestStatus.Done:
                return next == RollingEngagementDigestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RollingEngagementDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling engagement digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Fte * (double)Retries, 2);
    }

    public RollingEngagementDigest Copy()
    {
        return new RollingEngagementDigest(Code, Label, Retries, Badge, Fte, CostCenter, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
