namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant snapshot is in its life cycle.</summary>
public enum EquityGrantSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A equity grant snapshot. Its code is unique within a tenant.</summary>
public sealed class EquityGrantSnapshot(string code, string label, int tenureMonths, int capacity, int windowDays, int limit, EquityGrantSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public int WindowDays { get; set; } = windowDays;
    public int Limit { get; set; } = limit;
    public EquityGrantSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant snapshot in status created.</summary>
    public static EquityGrantSnapshot NewEquityGrantSnapshot(string code, string label)
    {
        return new EquityGrantSnapshot(code, label, 0, 0, 0, 0, EquityGrantSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantSnapshotStatus next)
    {
        switch (Status)
        {
            case EquityGrantSnapshotStatus.Created:
                return next == EquityGrantSnapshotStatus.Validated || next == EquityGrantSnapshotStatus.Published;
            case EquityGrantSnapshotStatus.Validated:
                switch (next)
                {
                    case EquityGrantSnapshotStatus.Published:
                    case EquityGrantSnapshotStatus.Retired:
                        return true;
                }
                return false;
            case EquityGrantSnapshotStatus.Published:
                switch (next)
                {
                    case EquityGrantSnapshotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)WindowDays * (double)Limit, 2);
    }

    public EquityGrantSnapshot Copy()
    {
        return new EquityGrantSnapshot(Code, Label, TenureMonths, Capacity, WindowDays, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
