namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict reorg draft is in its life cycle.</summary>
public enum StrictReorgDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One strict reorg draft, with what the planners need to act on it.</summary>
public sealed class StrictReorgDraft(string code, string title, int createdAt, string team, double amount, double rate, string skill, string employeeId, StrictReorgDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Team { get; set; } = team;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public StrictReorgDraftStatus Status { get; set; } = status;

    /// <summary>Starts a strict reorg draft in status proposed.</summary>
    public static StrictReorgDraft NewStrictReorgDraft(string code, string title)
    {
        return new StrictReorgDraft(code, title, 0, "", 0.0, 0.0, "", "", StrictReorgDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReorgDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReorgDraftStatus next)
    {
        switch (Status)
        {
            case StrictReorgDraftStatus.Proposed:
                return next == StrictReorgDraftStatus.Approved;
            case StrictReorgDraftStatus.Approved:
                return next == StrictReorgDraftStatus.Rejected || next == StrictReorgDraftStatus.Withdrawn;
            case StrictReorgDraftStatus.Rejected:
                switch (next)
                {
                    case StrictReorgDraftStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReorgDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict reorg draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)CreatedAt - Amount, 2);
    }

    public StrictReorgDraft Copy()
    {
        return new StrictReorgDraft(Code, Title, CreatedAt, Team, Amount, Rate, Skill, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
