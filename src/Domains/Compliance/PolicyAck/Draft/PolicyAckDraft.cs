namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack draft is in its life cycle.</summary>
public enum PolicyAckDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One policy ack draft, with what the planners need to act on it.</summary>
public sealed class PolicyAckDraft(string code, string title, int durationMinutes, Dictionary<string, string> attributes, string notes, int version, double discount, string timezone, PolicyAckDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Notes { get; set; } = notes;
    public int Version { get; set; } = version;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public PolicyAckDraftStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack draft in status open.</summary>
    public static PolicyAckDraft NewPolicyAckDraft(string code, string title)
    {
        return new PolicyAckDraft(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            "",
            PolicyAckDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckDraftStatus next)
    {
        switch (Status)
        {
            case PolicyAckDraftStatus.Open:
                switch (next)
                {
                    case PolicyAckDraftStatus.Reopened:
                        return true;
                }
                return false;
            case PolicyAckDraftStatus.InReview:
                return next == PolicyAckDraftStatus.Reopened || next == PolicyAckDraftStatus.Archived;
            case PolicyAckDraftStatus.Resolved:
                switch (next)
                {
                    case PolicyAckDraftStatus.Archived:
                        return true;
                }
                return false;
            case PolicyAckDraftStatus.Reopened:
                switch (next)
                {
                    case PolicyAckDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)Version - (double)DurationMinutes, 2);
    }

    public PolicyAckDraft Copy()
    {
        return new PolicyAckDraft(
            Code,
            Title,
            DurationMinutes,
            Attributes,
            Notes,
            Version,
            Discount,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
