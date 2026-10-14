namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run budget is in its life cycle.</summary>
public enum PayRunBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pay run budget and its current status.</summary>
public sealed class PayRunBudget(string code, string name, string grade, double amount, double margin, int leaveDays, double score, double fte, PayRunBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public PayRunBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a pay run budget in status draft.</summary>
    public static PayRunBudget NewPayRunBudget(string code, string name)
    {
        return new PayRunBudget(code, name, "", 0.0, 0.0, 0, 0.0, 0.0, PayRunBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunBudgetStatus next)
    {
        switch (Status)
        {
            case PayRunBudgetStatus.Draft:
                return next == PayRunBudgetStatus.Active;
            case PayRunBudgetStatus.Pending:
                switch (next)
                {
                    case PayRunBudgetStatus.Closed:
                        return true;
                }
                return false;
            case PayRunBudgetStatus.Active:
                return next == PayRunBudgetStatus.Suspended || next == PayRunBudgetStatus.Closed;
            case PayRunBudgetStatus.Suspended:
                return next == PayRunBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayRunBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay run budget"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Score * (double)LeaveDays, 2);
    }

    public PayRunBudget Copy()
    {
        return new PayRunBudget(Code, Name, Grade, Amount, Margin, LeaveDays, Score, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
