namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan entry is in its life cycle.</summary>
public enum ReturnPlanEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A return plan entry. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanEntry(string code, string title, string skill, int leaveDays, int retries, ReturnPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public int Retries { get; set; } = retries;
    public ReturnPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a return plan entry in status proposed.</summary>
    public static ReturnPlanEntry NewReturnPlanEntry(string code, string title)
    {
        return new ReturnPlanEntry(code, title, "", 0, 0, ReturnPlanEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanEntryStatus next)
    {
        switch (Status)
        {
            case ReturnPlanEntryStatus.Proposed:
                switch (next)
                {
                    case ReturnPlanEntryStatus.Rejected:
                        return true;
                }
                return false;
            case ReturnPlanEntryStatus.Approved:
                return next == ReturnPlanEntryStatus.Withdrawn;
            case ReturnPlanEntryStatus.Rejected:
                return next == ReturnPlanEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ReturnPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Retries * (double)LeaveDays, 2);
    }

    public ReturnPlanEntry Copy()
    {
        return new ReturnPlanEntry(Code, Title, Skill, LeaveDays, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
