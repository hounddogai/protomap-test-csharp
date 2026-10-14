namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling deduction adjustment is in its life cycle.</summary>
public enum RollingDeductionAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rolling deduction adjustment as the deduction team keeps it.</summary>
public sealed class RollingDeductionAdjustment(string code, string name, int revision, string unit, int updatedAt, int retries, double amount, int tenureMonths, string grade, RollingDeductionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Grade { get; set; } = grade;
    public RollingDeductionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling deduction adjustment in status new.</summary>
    public static RollingDeductionAdjustment NewRollingDeductionAdjustment(string code, string name)
    {
        return new RollingDeductionAdjustment(
            code,
            name,
            0,
            "",
            0,
            0,
            0.0,
            0,
            "",
            RollingDeductionAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingDeductionAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingDeductionAdjustmentStatus next)
    {
        switch (Status)
        {
            case RollingDeductionAdjustmentStatus.New:
                return next == RollingDeductionAdjustmentStatus.Running;
            case RollingDeductionAdjustmentStatus.Queued:
                return next == RollingDeductionAdjustmentStatus.Failed;
            case RollingDeductionAdjustmentStatus.Running:
                switch (next)
                {
                    case RollingDeductionAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case RollingDeductionAdjustmentStatus.Done:
                switch (next)
                {
                    case RollingDeductionAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingDeductionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling deduction adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Amount - (double)TenureMonths, 2);
    }

    public RollingDeductionAdjustment Copy()
    {
        return new RollingDeductionAdjustment(
            Code,
            Name,
            Revision,
            Unit,
            UpdatedAt,
            Retries,
            Amount,
            TenureMonths,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
