namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request budget is in its life cycle.</summary>
public enum LeaveRequestBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A leave request budget as the leave request team keeps it.</summary>
public sealed class LeaveRequestBudget(string code, string label, string locale, int priority, string grade, bool pinned, LeaveRequestBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int Priority { get; set; } = priority;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public LeaveRequestBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a leave request budget in status proposed.</summary>
    public static LeaveRequestBudget NewLeaveRequestBudget(string code, string label)
    {
        return new LeaveRequestBudget(code, label, "", 0, "", false, LeaveRequestBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestBudgetStatus next)
    {
        switch (Status)
        {
            case LeaveRequestBudgetStatus.Proposed:
                return next == LeaveRequestBudgetStatus.Withdrawn;
            case LeaveRequestBudgetStatus.Approved:
                switch (next)
                {
                    case LeaveRequestBudgetStatus.Rejected:
                        return true;
                }
                return false;
            case LeaveRequestBudgetStatus.Rejected:
                switch (next)
                {
                    case LeaveRequestBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public LeaveRequestBudget Copy()
    {
        return new LeaveRequestBudget(Code, Label, Locale, Priority, Grade, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
