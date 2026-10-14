namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive town hall ledger is in its life cycle.</summary>
public enum AdaptiveTownHallLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive town hall ledger as the town hall team keeps it.</summary>
public sealed class AdaptiveTownHallLedger(string code, string name, string managerId, bool approved, double score, string title, string summary, string reviewer, AdaptiveTownHallLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public bool Approved { get; set; } = approved;
    public double Score { get; set; } = score;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string Reviewer { get; set; } = reviewer;
    public AdaptiveTownHallLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive town hall ledger in status proposed.</summary>
    public static AdaptiveTownHallLedger NewAdaptiveTownHallLedger(string code, string name)
    {
        return new AdaptiveTownHallLedger(
            code,
            name,
            "",
            false,
            0.0,
            "",
            "",
            "",
            AdaptiveTownHallLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTownHallLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTownHallLedgerStatus next)
    {
        switch (Status)
        {
            case AdaptiveTownHallLedgerStatus.Proposed:
                switch (next)
                {
                    case AdaptiveTownHallLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case AdaptiveTownHallLedgerStatus.Approved:
                return next == AdaptiveTownHallLedgerStatus.Rejected || next == AdaptiveTownHallLedgerStatus.Withdrawn;
            case AdaptiveTownHallLedgerStatus.Rejected:
                return next == AdaptiveTownHallLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveTownHallLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive town hall ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AdaptiveTownHallLedger Copy()
    {
        return new AdaptiveTownHallLedger(Code, Name, ManagerId, Approved, Score, Title, Summary, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
