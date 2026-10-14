namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pay run cycle is in its life cycle.</summary>
public enum RollingPayRunCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rolling pay run cycle and its current status.</summary>
public sealed class RollingPayRunCycle(string code, string label, double ratio, string externalRef, bool archived, int createdAt, string ownerId, RollingPayRunCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string ExternalRef { get; set; } = externalRef;
    public bool Archived { get; set; } = archived;
    public int CreatedAt { get; set; } = createdAt;
    public string OwnerId { get; set; } = ownerId;
    public RollingPayRunCycleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pay run cycle in status new.</summary>
    public static RollingPayRunCycle NewRollingPayRunCycle(string code, string label)
    {
        return new RollingPayRunCycle(code, label, 0.0, "", false, 0, "", RollingPayRunCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPayRunCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPayRunCycleStatus next)
    {
        switch (Status)
        {
            case RollingPayRunCycleStatus.New:
                switch (next)
                {
                    case RollingPayRunCycleStatus.Failed:
                        return true;
                }
                return false;
            case RollingPayRunCycleStatus.Queued:
                return next == RollingPayRunCycleStatus.Running;
            case RollingPayRunCycleStatus.Running:
                switch (next)
                {
                    case RollingPayRunCycleStatus.Failed:
                        return true;
                }
                return false;
            case RollingPayRunCycleStatus.Done:
                switch (next)
                {
                    case RollingPayRunCycleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPayRunCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pay run cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"rolling pay run cycle"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)CreatedAt * Ratio, 2);
    }

    public RollingPayRunCycle Copy()
    {
        return new RollingPayRunCycle(Code, Label, Ratio, ExternalRef, Archived, CreatedAt, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
