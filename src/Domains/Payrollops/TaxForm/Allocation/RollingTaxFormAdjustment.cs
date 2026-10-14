namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling tax form adjustment is in its life cycle.</summary>
public enum RollingTaxFormAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One rolling tax form adjustment, with what the planners need to act on it.</summary>
public sealed class RollingTaxFormAdjustment(string code, string name, string region, bool pinned, int attempts, string grade, int capacity, RollingTaxFormAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public bool Pinned { get; set; } = pinned;
    public int Attempts { get; set; } = attempts;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public RollingTaxFormAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling tax form adjustment in status idle.</summary>
    public static RollingTaxFormAdjustment NewRollingTaxFormAdjustment(string code, string name)
    {
        return new RollingTaxFormAdjustment(code, name, "", false, 0, "", 0, RollingTaxFormAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingTaxFormAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingTaxFormAdjustmentStatus next)
    {
        switch (Status)
        {
            case RollingTaxFormAdjustmentStatus.Idle:
                return next == RollingTaxFormAdjustmentStatus.Ready || next == RollingTaxFormAdjustmentStatus.Stopped;
            case RollingTaxFormAdjustmentStatus.Warming:
                return next == RollingTaxFormAdjustmentStatus.Stopped;
            case RollingTaxFormAdjustmentStatus.Ready:
                return next == RollingTaxFormAdjustmentStatus.Stopped;
            case RollingTaxFormAdjustmentStatus.Draining:
                return next == RollingTaxFormAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingTaxFormAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling tax form adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Attempts - (double)Capacity, 2);
    }

    public RollingTaxFormAdjustment Copy()
    {
        return new RollingTaxFormAdjustment(Code, Name, Region, Pinned, Attempts, Grade, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
