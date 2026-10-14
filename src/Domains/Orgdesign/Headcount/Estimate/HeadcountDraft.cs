namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount draft is in its life cycle.</summary>
public enum HeadcountDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a headcount draft and its current status.</summary>
public sealed class HeadcountDraft(string code, string title, bool archived, string summary, int limit, int graceDays, bool pinned, int durationMinutes, string contactEmail, int endsAt, double minimum, HeadcountDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Summary { get; set; } = summary;
    public int Limit { get; set; } = limit;
    public int GraceDays { get; set; } = graceDays;
    public bool Pinned { get; set; } = pinned;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public int EndsAt { get; set; } = endsAt;
    public double Minimum { get; set; } = minimum;
    public HeadcountDraftStatus Status { get; set; } = status;

    /// <summary>Starts a headcount draft in status requested.</summary>
    public static HeadcountDraft NewHeadcountDraft(string code, string title)
    {
        return new HeadcountDraft(
            code,
            title,
            false,
            "",
            0,
            0,
            false,
            0,
            "",
            0,
            0.0,
            HeadcountDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountDraftStatus next)
    {
        switch (Status)
        {
            case HeadcountDraftStatus.Requested:
                return next == HeadcountDraftStatus.Revoked || next == HeadcountDraftStatus.Expired;
            case HeadcountDraftStatus.Granted:
                switch (next)
                {
                    case HeadcountDraftStatus.Revoked:
                    case HeadcountDraftStatus.Expired:
                        return true;
                }
                return false;
            case HeadcountDraftStatus.Revoked:
                switch (next)
                {
                    case HeadcountDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)EndsAt * (double)DurationMinutes, 2);
    }

    public HeadcountDraft Copy()
    {
        return new HeadcountDraft(
            Code,
            Title,
            Archived,
            Summary,
            Limit,
            GraceDays,
            Pinned,
            DurationMinutes,
            ContactEmail,
            EndsAt,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
