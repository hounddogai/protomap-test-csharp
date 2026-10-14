namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement snapshot is in its life cycle.</summary>
public enum EngagementSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A engagement snapshot. Its code is unique within a tenant.</summary>
public sealed class EngagementSnapshot(string code, string title, string contactEmail, string unit, string salaryBand, double maximum, string grade, int batchSize, EngagementSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Unit { get; set; } = unit;
    public string SalaryBand { get; set; } = salaryBand;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public int BatchSize { get; set; } = batchSize;
    public EngagementSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a engagement snapshot in status idle.</summary>
    public static EngagementSnapshot NewEngagementSnapshot(string code, string title)
    {
        return new EngagementSnapshot(code, title, "", "", "", 0.0, "", 0, EngagementSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementSnapshotStatus next)
    {
        switch (Status)
        {
            case EngagementSnapshotStatus.Idle:
                return next == EngagementSnapshotStatus.Ready || next == EngagementSnapshotStatus.Stopped;
            case EngagementSnapshotStatus.Warming:
                switch (next)
                {
                    case EngagementSnapshotStatus.Ready:
                    case EngagementSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case EngagementSnapshotStatus.Ready:
                switch (next)
                {
                    case EngagementSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case EngagementSnapshotStatus.Draining:
                switch (next)
                {
                    case EngagementSnapshotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"engagement snapshot"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Maximum + (double)BatchSize, 2);
    }

    public EngagementSnapshot Copy()
    {
        return new EngagementSnapshot(
            Code,
            Title,
            ContactEmail,
            Unit,
            SalaryBand,
            Maximum,
            Grade,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
