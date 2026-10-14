namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan draft is in its life cycle.</summary>
public enum ActionPlanDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One action plan draft, with what the planners need to act on it.</summary>
public sealed class ActionPlanDraft(string code, string label, string notes, string phone, int windowDays, string salaryBand, int limit, int updatedAt, ActionPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public int WindowDays { get; set; } = windowDays;
    public string SalaryBand { get; set; } = salaryBand;
    public int Limit { get; set; } = limit;
    public int UpdatedAt { get; set; } = updatedAt;
    public ActionPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a action plan draft in status requested.</summary>
    public static ActionPlanDraft NewActionPlanDraft(string code, string label)
    {
        return new ActionPlanDraft(code, label, "", "", 0, "", 0, 0, ActionPlanDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanDraftStatus next)
    {
        switch (Status)
        {
            case ActionPlanDraftStatus.Requested:
                return next == ActionPlanDraftStatus.Revoked || next == ActionPlanDraftStatus.Expired;
            case ActionPlanDraftStatus.Granted:
                switch (next)
                {
                    case ActionPlanDraftStatus.Revoked:
                    case ActionPlanDraftStatus.Expired:
                        return true;
                }
                return false;
            case ActionPlanDraftStatus.Revoked:
                switch (next)
                {
                    case ActionPlanDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Limit * (double)WindowDays, 2);
    }

    public ActionPlanDraft Copy()
    {
        return new ActionPlanDraft(Code, Label, Notes, Phone, WindowDays, SalaryBand, Limit, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
