namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary action plan ledger is in its life cycle.</summary>
public enum PrimaryActionPlanLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary action plan ledger. Its code is unique within a tenant.</summary>
public sealed class PrimaryActionPlanLedger(string code, string title, string region, bool pinned, string reviewer, double budget, string contactEmail, double score, PrimaryActionPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public bool Pinned { get; set; } = pinned;
    public string Reviewer { get; set; } = reviewer;
    public double Budget { get; set; } = budget;
    public string ContactEmail { get; set; } = contactEmail;
    public double Score { get; set; } = score;
    public PrimaryActionPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a primary action plan ledger in status open.</summary>
    public static PrimaryActionPlanLedger NewPrimaryActionPlanLedger(string code, string title)
    {
        return new PrimaryActionPlanLedger(
            code,
            title,
            "",
            false,
            "",
            0.0,
            "",
            0.0,
            PrimaryActionPlanLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryActionPlanLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryActionPlanLedgerStatus next)
    {
        switch (Status)
        {
            case PrimaryActionPlanLedgerStatus.Open:
                return next == PrimaryActionPlanLedgerStatus.InReview || next == PrimaryActionPlanLedgerStatus.Resolved;
            case PrimaryActionPlanLedgerStatus.InReview:
                return next == PrimaryActionPlanLedgerStatus.Reopened || next == PrimaryActionPlanLedgerStatus.Archived;
            case PrimaryActionPlanLedgerStatus.Resolved:
                return next == PrimaryActionPlanLedgerStatus.Reopened || next == PrimaryActionPlanLedgerStatus.Archived;
            case PrimaryActionPlanLedgerStatus.Reopened:
                return next == PrimaryActionPlanLedgerStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryActionPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary action plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Score * Budget, 2);
    }

    public PrimaryActionPlanLedger Copy()
    {
        return new PrimaryActionPlanLedger(
            Code,
            Title,
            Region,
            Pinned,
            Reviewer,
            Budget,
            ContactEmail,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
