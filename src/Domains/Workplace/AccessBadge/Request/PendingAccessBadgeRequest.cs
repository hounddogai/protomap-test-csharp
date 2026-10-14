namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending access badge request is in its life cycle.</summary>
public enum PendingAccessBadgeRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pending access badge request and its current status.</summary>
public sealed class PendingAccessBadgeRequest(string code, string name, int retries, int durationMinutes, string summary, int revision, Dictionary<string, string> attributes, string channel, PendingAccessBadgeRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Summary { get; set; } = summary;
    public int Revision { get; set; } = revision;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Channel { get; set; } = channel;
    public PendingAccessBadgeRequestStatus Status { get; set; } = status;

    /// <summary>Starts a pending access badge request in status open.</summary>
    public static PendingAccessBadgeRequest NewPendingAccessBadgeRequest(string code, string name)
    {
        return new PendingAccessBadgeRequest(
            code,
            name,
            0,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            "",
            PendingAccessBadgeRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAccessBadgeRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAccessBadgeRequestStatus next)
    {
        switch (Status)
        {
            case PendingAccessBadgeRequestStatus.Open:
                return next == PendingAccessBadgeRequestStatus.Archived;
            case PendingAccessBadgeRequestStatus.InReview:
                return next == PendingAccessBadgeRequestStatus.Archived;
            case PendingAccessBadgeRequestStatus.Resolved:
                return next == PendingAccessBadgeRequestStatus.Archived;
            case PendingAccessBadgeRequestStatus.Reopened:
                switch (next)
                {
                    case PendingAccessBadgeRequestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingAccessBadgeRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending access badge request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"pending access badge request"} {Code}";
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

    public double AdjustedLoad()
    {
        return Math.Round((double)Revision * (double)DurationMinutes, 2);
    }

    public PendingAccessBadgeRequest Copy()
    {
        return new PendingAccessBadgeRequest(
            Code,
            Name,
            Retries,
            DurationMinutes,
            Summary,
            Revision,
            Attributes,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
