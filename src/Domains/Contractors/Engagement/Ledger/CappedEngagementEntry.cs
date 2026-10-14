namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped engagement entry is in its life cycle.</summary>
public enum CappedEngagementEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped engagement entry. Its code is unique within a tenant.</summary>
public sealed class CappedEngagementEntry(string code, string label, string contactEmail, bool visible, int endsAt, double rating, string team, double score, CappedEngagementEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Visible { get; set; } = visible;
    public int EndsAt { get; set; } = endsAt;
    public double Rating { get; set; } = rating;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public CappedEngagementEntryStatus Status { get; set; } = status;

    /// <summary>Starts a capped engagement entry in status open.</summary>
    public static CappedEngagementEntry NewCappedEngagementEntry(string code, string label)
    {
        return new CappedEngagementEntry(code, label, "", false, 0, 0.0, "", 0.0, CappedEngagementEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedEngagementEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedEngagementEntryStatus next)
    {
        switch (Status)
        {
            case CappedEngagementEntryStatus.Open:
                return next == CappedEngagementEntryStatus.InReview;
            case CappedEngagementEntryStatus.InReview:
                switch (next)
                {
                    case CappedEngagementEntryStatus.Resolved:
                        return true;
                }
                return false;
            case CappedEngagementEntryStatus.Resolved:
                switch (next)
                {
                    case CappedEngagementEntryStatus.Reopened:
                        return true;
                }
                return false;
            case CappedEngagementEntryStatus.Reopened:
                switch (next)
                {
                    case CappedEngagementEntryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedEngagementEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped engagement entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)EndsAt + Score, 2);
    }

    public CappedEngagementEntry Copy()
    {
        return new CappedEngagementEntry(Code, Label, ContactEmail, Visible, EndsAt, Rating, Team, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
