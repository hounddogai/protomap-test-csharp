namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip adjustment is in its life cycle.</summary>
public enum PayslipAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A payslip adjustment. Its code is unique within a tenant.</summary>
public sealed class PayslipAdjustment(string code, string name, double weight, bool visible, double score, int attempts, string ownerId, PayslipAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public bool Visible { get; set; } = visible;
    public double Score { get; set; } = score;
    public int Attempts { get; set; } = attempts;
    public string OwnerId { get; set; } = ownerId;
    public PayslipAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a payslip adjustment in status idle.</summary>
    public static PayslipAdjustment NewPayslipAdjustment(string code, string name)
    {
        return new PayslipAdjustment(code, name, 0.0, false, 0.0, 0, "", PayslipAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipAdjustmentStatus next)
    {
        switch (Status)
        {
            case PayslipAdjustmentStatus.Idle:
                switch (next)
                {
                    case PayslipAdjustmentStatus.Ready:
                        return true;
                }
                return false;
            case PayslipAdjustmentStatus.Warming:
                switch (next)
                {
                    case PayslipAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case PayslipAdjustmentStatus.Ready:
                switch (next)
                {
                    case PayslipAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case PayslipAdjustmentStatus.Draining:
                return next == PayslipAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayslipAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts - Score, 2);
    }

    public PayslipAdjustment Copy()
    {
        return new PayslipAdjustment(Code, Name, Weight, Visible, Score, Attempts, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
