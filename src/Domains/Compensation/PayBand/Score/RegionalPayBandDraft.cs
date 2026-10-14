namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pay band draft is in its life cycle.</summary>
public enum RegionalPayBandDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One regional pay band draft, with what the planners need to act on it.</summary>
public sealed class RegionalPayBandDraft(string code, string title, string notes, string phone, int leaveDays, int windowDays, int quantity, bool archived, int startsAt, List<string> tags, RegionalPayBandDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public int LeaveDays { get; set; } = leaveDays;
    public int WindowDays { get; set; } = windowDays;
    public int Quantity { get; set; } = quantity;
    public bool Archived { get; set; } = archived;
    public int StartsAt { get; set; } = startsAt;
    public List<string> Tags { get; set; } = tags;
    public RegionalPayBandDraftStatus Status { get; set; } = status;

    /// <summary>Starts a regional pay band draft in status requested.</summary>
    public static RegionalPayBandDraft NewRegionalPayBandDraft(string code, string title)
    {
        return new RegionalPayBandDraft(
            code,
            title,
            "",
            "",
            0,
            0,
            0,
            false,
            0,
            new List<string>(),
            RegionalPayBandDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayBandDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayBandDraftStatus next)
    {
        switch (Status)
        {
            case RegionalPayBandDraftStatus.Requested:
                switch (next)
                {
                    case RegionalPayBandDraftStatus.Granted:
                        return true;
                }
                return false;
            case RegionalPayBandDraftStatus.Granted:
                return next == RegionalPayBandDraftStatus.Revoked;
            case RegionalPayBandDraftStatus.Revoked:
                switch (next)
                {
                    case RegionalPayBandDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalPayBandDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pay band draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt * (double)LeaveDays, 2);
    }

    public RegionalPayBandDraft Copy()
    {
        return new RegionalPayBandDraft(
            Code,
            Title,
            Notes,
            Phone,
            LeaveDays,
            WindowDays,
            Quantity,
            Archived,
            StartsAt,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
