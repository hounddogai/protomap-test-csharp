namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal access badge draft is in its life cycle.</summary>
public enum SeasonalAccessBadgeDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a seasonal access badge draft and its current status.</summary>
public sealed class SeasonalAccessBadgeDraft(string code, string title, bool pinned, string checksum, int startsAt, Dictionary<string, string> attributes, SeasonalAccessBadgeDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Checksum { get; set; } = checksum;
    public int StartsAt { get; set; } = startsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public SeasonalAccessBadgeDraftStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal access badge draft in status open.</summary>
    public static SeasonalAccessBadgeDraft NewSeasonalAccessBadgeDraft(string code, string title)
    {
        return new SeasonalAccessBadgeDraft(
            code,
            title,
            false,
            "",
            0,
            new Dictionary<string, string>(),
            SeasonalAccessBadgeDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAccessBadgeDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAccessBadgeDraftStatus next)
    {
        switch (Status)
        {
            case SeasonalAccessBadgeDraftStatus.Open:
                switch (next)
                {
                    case SeasonalAccessBadgeDraftStatus.Reopened:
                    case SeasonalAccessBadgeDraftStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalAccessBadgeDraftStatus.InReview:
                switch (next)
                {
                    case SeasonalAccessBadgeDraftStatus.Resolved:
                        return true;
                }
                return false;
            case SeasonalAccessBadgeDraftStatus.Resolved:
                return next == SeasonalAccessBadgeDraftStatus.Reopened || next == SeasonalAccessBadgeDraftStatus.Archived;
            case SeasonalAccessBadgeDraftStatus.Reopened:
                switch (next)
                {
                    case SeasonalAccessBadgeDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAccessBadgeDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal access badge draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal access badge draft"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public SeasonalAccessBadgeDraft Copy()
    {
        return new SeasonalAccessBadgeDraft(Code, Title, Pinned, Checksum, StartsAt, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
