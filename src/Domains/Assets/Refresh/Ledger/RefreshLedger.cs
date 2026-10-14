namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh ledger is in its life cycle.</summary>
public enum RefreshLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One refresh ledger, with what the planners need to act on it.</summary>
public sealed class RefreshLedger(string code, string label, double discount, bool archived, string category, int windowDays, string unit, string country, string reviewer, string skill, double fte, RefreshLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public bool Archived { get; set; } = archived;
    public string Category { get; set; } = category;
    public int WindowDays { get; set; } = windowDays;
    public string Unit { get; set; } = unit;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public string Skill { get; set; } = skill;
    public double Fte { get; set; } = fte;
    public RefreshLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a refresh ledger in status new.</summary>
    public static RefreshLedger NewRefreshLedger(string code, string label)
    {
        return new RefreshLedger(code, label, 0.0, false, "", 0, "", "", "", "", 0.0, RefreshLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshLedgerStatus next)
    {
        switch (Status)
        {
            case RefreshLedgerStatus.New:
                switch (next)
                {
                    case RefreshLedgerStatus.Running:
                    case RefreshLedgerStatus.Failed:
                        return true;
                }
                return false;
            case RefreshLedgerStatus.Queued:
                return next == RefreshLedgerStatus.Running || next == RefreshLedgerStatus.Failed;
            case RefreshLedgerStatus.Running:
                switch (next)
                {
                    case RefreshLedgerStatus.Done:
                        return true;
                }
                return false;
            case RefreshLedgerStatus.Done:
                switch (next)
                {
                    case RefreshLedgerStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round(Discount * Fte, 2);
    }

    public RefreshLedger Copy()
    {
        return new RefreshLedger(
            Code,
            Label,
            Discount,
            Archived,
            Category,
            WindowDays,
            Unit,
            Country,
            Reviewer,
            Skill,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
