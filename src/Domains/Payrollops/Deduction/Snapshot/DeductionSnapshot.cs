namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction snapshot is in its life cycle.</summary>
public enum DeductionSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a deduction snapshot and its current status.</summary>
public sealed class DeductionSnapshot(string code, string label, double ceiling, string country, string skill, bool pinned, int limit, string externalRef, string title, string salaryBand, DeductionSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Country { get; set; } = country;
    public string Skill { get; set; } = skill;
    public bool Pinned { get; set; } = pinned;
    public int Limit { get; set; } = limit;
    public string ExternalRef { get; set; } = externalRef;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public DeductionSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a deduction snapshot in status idle.</summary>
    public static DeductionSnapshot NewDeductionSnapshot(string code, string label)
    {
        return new DeductionSnapshot(code, label, 0.0, "", "", false, 0, "", "", "", DeductionSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionSnapshotStatus next)
    {
        switch (Status)
        {
            case DeductionSnapshotStatus.Idle:
                switch (next)
                {
                    case DeductionSnapshotStatus.Ready:
                        return true;
                }
                return false;
            case DeductionSnapshotStatus.Warming:
                return next == DeductionSnapshotStatus.Ready || next == DeductionSnapshotStatus.Stopped;
            case DeductionSnapshotStatus.Ready:
                switch (next)
                {
                    case DeductionSnapshotStatus.Draining:
                    case DeductionSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case DeductionSnapshotStatus.Draining:
                return next == DeductionSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DeductionSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Limit - Ceiling, 2);
    }

    public DeductionSnapshot Copy()
    {
        return new DeductionSnapshot(
            Code,
            Label,
            Ceiling,
            Country,
            Skill,
            Pinned,
            Limit,
            ExternalRef,
            Title,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
