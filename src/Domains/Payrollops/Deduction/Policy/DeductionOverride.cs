namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction override is in its life cycle.</summary>
public enum DeductionOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A deduction override. Its code is unique within a tenant.</summary>
public sealed class DeductionOverride(string code, string name, int updatedAt, string costCenter, double ratio, DeductionOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string CostCenter { get; set; } = costCenter;
    public double Ratio { get; set; } = ratio;
    public DeductionOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a deduction override in status planned.</summary>
    public static DeductionOverride NewDeductionOverride(string code, string name)
    {
        return new DeductionOverride(code, name, 0, "", 0.0, DeductionOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionOverrideStatus next)
    {
        switch (Status)
        {
            case DeductionOverrideStatus.Planned:
                switch (next)
                {
                    case DeductionOverrideStatus.Scheduled:
                    case DeductionOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case DeductionOverrideStatus.Scheduled:
                return next == DeductionOverrideStatus.InProgress;
            case DeductionOverrideStatus.InProgress:
                switch (next)
                {
                    case DeductionOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case DeductionOverrideStatus.Completed:
                switch (next)
                {
                    case DeductionOverrideStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"deduction override"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)UpdatedAt + Ratio, 2);
    }

    public DeductionOverride Copy()
    {
        return new DeductionOverride(Code, Name, UpdatedAt, CostCenter, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
