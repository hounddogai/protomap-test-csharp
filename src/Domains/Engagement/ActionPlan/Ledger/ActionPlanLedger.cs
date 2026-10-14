namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan ledger is in its life cycle.</summary>
public enum ActionPlanLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A action plan ledger. Its code is unique within a tenant.</summary>
public sealed class ActionPlanLedger(string code, string title, double fte, int capacity, string team, ActionPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public ActionPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a action plan ledger in status proposed.</summary>
    public static ActionPlanLedger NewActionPlanLedger(string code, string title)
    {
        return new ActionPlanLedger(code, title, 0.0, 0, "", ActionPlanLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanLedgerStatus next)
    {
        switch (Status)
        {
            case ActionPlanLedgerStatus.Proposed:
                return next == ActionPlanLedgerStatus.Rejected;
            case ActionPlanLedgerStatus.Approved:
                return next == ActionPlanLedgerStatus.Rejected;
            case ActionPlanLedgerStatus.Rejected:
                return next == ActionPlanLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ActionPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Fte * (double)Capacity, 2);
    }

    public ActionPlanLedger Copy()
    {
        return new ActionPlanLedger(Code, Title, Fte, Capacity, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
