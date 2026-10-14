namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip target is in its life cycle.</summary>
public enum PayslipTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a payslip target and its current status.</summary>
public sealed class PayslipTarget(string code, string label, double cost, bool visible, double score, int limit, int endsAt, double rate, string phone, string source, string grade, PayslipTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public bool Visible { get; set; } = visible;
    public double Score { get; set; } = score;
    public int Limit { get; set; } = limit;
    public int EndsAt { get; set; } = endsAt;
    public double Rate { get; set; } = rate;
    public string Phone { get; set; } = phone;
    public string Source { get; set; } = source;
    public string Grade { get; set; } = grade;
    public PayslipTargetStatus Status { get; set; } = status;

    /// <summary>Starts a payslip target in status new.</summary>
    public static PayslipTarget NewPayslipTarget(string code, string label)
    {
        return new PayslipTarget(code, label, 0.0, false, 0.0, 0, 0, 0.0, "", "", "", PayslipTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipTargetStatus next)
    {
        switch (Status)
        {
            case PayslipTargetStatus.New:
                switch (next)
                {
                    case PayslipTargetStatus.Queued:
                    case PayslipTargetStatus.Failed:
                        return true;
                }
                return false;
            case PayslipTargetStatus.Queued:
                return next == PayslipTargetStatus.Done;
            case PayslipTargetStatus.Running:
                return next == PayslipTargetStatus.Done || next == PayslipTargetStatus.Failed;
            case PayslipTargetStatus.Done:
                switch (next)
                {
                    case PayslipTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Limit * (double)EndsAt, 2);
    }

    public PayslipTarget Copy()
    {
        return new PayslipTarget(
            Code,
            Label,
            Cost,
            Visible,
            Score,
            Limit,
            EndsAt,
            Rate,
            Phone,
            Source,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
