namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted payslip draft is in its life cycle.</summary>
public enum WeightedPayslipDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One weighted payslip draft, with what the planners need to act on it.</summary>
public sealed class WeightedPayslipDraft(string code, string label, double budget, int retries, string unit, string managerId, double floorValue, WeightedPayslipDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public string Unit { get; set; } = unit;
    public string ManagerId { get; set; } = managerId;
    public double FloorValue { get; set; } = floorValue;
    public WeightedPayslipDraftStatus Status { get; set; } = status;

    /// <summary>Starts a weighted payslip draft in status requested.</summary>
    public static WeightedPayslipDraft NewWeightedPayslipDraft(string code, string label)
    {
        return new WeightedPayslipDraft(code, label, 0.0, 0, "", "", 0.0, WeightedPayslipDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPayslipDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPayslipDraftStatus next)
    {
        switch (Status)
        {
            case WeightedPayslipDraftStatus.Requested:
                switch (next)
                {
                    case WeightedPayslipDraftStatus.Revoked:
                        return true;
                }
                return false;
            case WeightedPayslipDraftStatus.Granted:
                switch (next)
                {
                    case WeightedPayslipDraftStatus.Expired:
                        return true;
                }
                return false;
            case WeightedPayslipDraftStatus.Revoked:
                return next == WeightedPayslipDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedPayslipDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted payslip draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Retries * Budget, 2);
    }

    public WeightedPayslipDraft Copy()
    {
        return new WeightedPayslipDraft(Code, Label, Budget, Retries, Unit, ManagerId, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
