namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive payslip claim is in its life cycle.</summary>
public enum AdaptivePayslipClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive payslip claim, with what the planners need to act on it.</summary>
public sealed class AdaptivePayslipClaim(string code, string label, string team, int retries, double maximum, int leaveDays, AdaptivePayslipClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public AdaptivePayslipClaimStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive payslip claim in status new.</summary>
    public static AdaptivePayslipClaim NewAdaptivePayslipClaim(string code, string label)
    {
        return new AdaptivePayslipClaim(code, label, "", 0, 0.0, 0, AdaptivePayslipClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePayslipClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePayslipClaimStatus next)
    {
        switch (Status)
        {
            case AdaptivePayslipClaimStatus.New:
                return next == AdaptivePayslipClaimStatus.Done || next == AdaptivePayslipClaimStatus.Failed;
            case AdaptivePayslipClaimStatus.Queued:
                switch (next)
                {
                    case AdaptivePayslipClaimStatus.Running:
                    case AdaptivePayslipClaimStatus.Done:
                        return true;
                }
                return false;
            case AdaptivePayslipClaimStatus.Running:
                switch (next)
                {
                    case AdaptivePayslipClaimStatus.Done:
                    case AdaptivePayslipClaimStatus.Failed:
                        return true;
                }
                return false;
            case AdaptivePayslipClaimStatus.Done:
                switch (next)
                {
                    case AdaptivePayslipClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePayslipClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive payslip claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)LeaveDays * (double)Retries, 2);
    }

    public AdaptivePayslipClaim Copy()
    {
        return new AdaptivePayslipClaim(Code, Label, Team, Retries, Maximum, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
