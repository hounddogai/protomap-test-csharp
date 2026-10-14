namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual equity grant draft is in its life cycle.</summary>
public enum ManualEquityGrantDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a manual equity grant draft and its current status.</summary>
public sealed class ManualEquityGrantDraft(string code, string title, int version, double budget, string checksum, bool pinned, double rating, ManualEquityGrantDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public double Budget { get; set; } = budget;
    public string Checksum { get; set; } = checksum;
    public bool Pinned { get; set; } = pinned;
    public double Rating { get; set; } = rating;
    public ManualEquityGrantDraftStatus Status { get; set; } = status;

    /// <summary>Starts a manual equity grant draft in status open.</summary>
    public static ManualEquityGrantDraft NewManualEquityGrantDraft(string code, string title)
    {
        return new ManualEquityGrantDraft(code, title, 0, 0.0, "", false, 0.0, ManualEquityGrantDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualEquityGrantDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualEquityGrantDraftStatus next)
    {
        switch (Status)
        {
            case ManualEquityGrantDraftStatus.Open:
                return next == ManualEquityGrantDraftStatus.Reopened;
            case ManualEquityGrantDraftStatus.InReview:
                return next == ManualEquityGrantDraftStatus.Reopened;
            case ManualEquityGrantDraftStatus.Resolved:
                return next == ManualEquityGrantDraftStatus.Reopened || next == ManualEquityGrantDraftStatus.Archived;
            case ManualEquityGrantDraftStatus.Reopened:
                switch (next)
                {
                    case ManualEquityGrantDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualEquityGrantDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual equity grant draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Version * Budget, 2);
    }

    public ManualEquityGrantDraft Copy()
    {
        return new ManualEquityGrantDraft(Code, Title, Version, Budget, Checksum, Pinned, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
