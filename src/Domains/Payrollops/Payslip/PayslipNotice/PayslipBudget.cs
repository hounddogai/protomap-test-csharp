namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip budget is in its life cycle.</summary>
public enum PayslipBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A payslip budget as the payslip team keeps it.</summary>
public sealed class PayslipBudget(string code, string title, int graceDays, double rate, int attempts, PayslipBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public PayslipBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a payslip budget in status requested.</summary>
    public static PayslipBudget NewPayslipBudget(string code, string title)
    {
        return new PayslipBudget(code, title, 0, 0.0, 0, PayslipBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipBudgetStatus next)
    {
        switch (Status)
        {
            case PayslipBudgetStatus.Requested:
                switch (next)
                {
                    case PayslipBudgetStatus.Revoked:
                        return true;
                }
                return false;
            case PayslipBudgetStatus.Granted:
                return next == PayslipBudgetStatus.Expired;
            case PayslipBudgetStatus.Revoked:
                return next == PayslipBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayslipBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"payslip budget"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Attempts * (double)GraceDays, 2);
    }

    public PayslipBudget Copy()
    {
        return new PayslipBudget(Code, Title, GraceDays, Rate, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
