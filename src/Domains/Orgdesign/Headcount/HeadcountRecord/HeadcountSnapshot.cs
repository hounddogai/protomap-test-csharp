namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount snapshot is in its life cycle.</summary>
public enum HeadcountSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A headcount snapshot as the headcount team keeps it.</summary>
public sealed class HeadcountSnapshot(string code, string label, int limit, string unit, int version, double score, string costCenter, HeadcountSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Unit { get; set; } = unit;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public string CostCenter { get; set; } = costCenter;
    public HeadcountSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a headcount snapshot in status idle.</summary>
    public static HeadcountSnapshot NewHeadcountSnapshot(string code, string label)
    {
        return new HeadcountSnapshot(code, label, 0, "", 0, 0.0, "", HeadcountSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountSnapshotStatus next)
    {
        switch (Status)
        {
            case HeadcountSnapshotStatus.Idle:
                switch (next)
                {
                    case HeadcountSnapshotStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountSnapshotStatus.Warming:
                switch (next)
                {
                    case HeadcountSnapshotStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountSnapshotStatus.Ready:
                switch (next)
                {
                    case HeadcountSnapshotStatus.Draining:
                        return true;
                }
                return false;
            case HeadcountSnapshotStatus.Draining:
                return next == HeadcountSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(HeadcountSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Limit * (double)Version, 2);
    }

    public HeadcountSnapshot Copy()
    {
        return new HeadcountSnapshot(Code, Label, Limit, Unit, Version, Score, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
