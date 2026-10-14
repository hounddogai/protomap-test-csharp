namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction milestone is in its life cycle.</summary>
public enum DeductionMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A deduction milestone. Its code is unique within a tenant.</summary>
public sealed class DeductionMilestone(string code, string name, double maximum, string jobCode, double weight, string externalRef, string badge, int updatedAt, double minimum, DeductionMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string JobCode { get; set; } = jobCode;
    public double Weight { get; set; } = weight;
    public string ExternalRef { get; set; } = externalRef;
    public string Badge { get; set; } = badge;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public DeductionMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a deduction milestone in status idle.</summary>
    public static DeductionMilestone NewDeductionMilestone(string code, string name)
    {
        return new DeductionMilestone(code, name, 0.0, "", 0.0, "", "", 0, 0.0, DeductionMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionMilestoneStatus next)
    {
        switch (Status)
        {
            case DeductionMilestoneStatus.Idle:
                return next == DeductionMilestoneStatus.Draining;
            case DeductionMilestoneStatus.Warming:
                switch (next)
                {
                    case DeductionMilestoneStatus.Ready:
                    case DeductionMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case DeductionMilestoneStatus.Ready:
                switch (next)
                {
                    case DeductionMilestoneStatus.Draining:
                        return true;
                }
                return false;
            case DeductionMilestoneStatus.Draining:
                switch (next)
                {
                    case DeductionMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum + Weight, 2);
    }

    public DeductionMilestone Copy()
    {
        return new DeductionMilestone(
            Code,
            Name,
            Maximum,
            JobCode,
            Weight,
            ExternalRef,
            Badge,
            UpdatedAt,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
