namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement tier is in its life cycle.</summary>
public enum EngagementTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A engagement tier. Its code is unique within a tenant.</summary>
public sealed class EngagementTier(string code, string label, List<string> tags, int sequence, string timezone, int revision, string summary, EngagementTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public int Sequence { get; set; } = sequence;
    public string Timezone { get; set; } = timezone;
    public int Revision { get; set; } = revision;
    public string Summary { get; set; } = summary;
    public EngagementTierStatus Status { get; set; } = status;

    /// <summary>Starts a engagement tier in status open.</summary>
    public static EngagementTier NewEngagementTier(string code, string label)
    {
        return new EngagementTier(code, label, new List<string>(), 0, "", 0, "", EngagementTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementTierStatus next)
    {
        switch (Status)
        {
            case EngagementTierStatus.Open:
                switch (next)
                {
                    case EngagementTierStatus.Resolved:
                        return true;
                }
                return false;
            case EngagementTierStatus.InReview:
                return next == EngagementTierStatus.Reopened;
            case EngagementTierStatus.Resolved:
                switch (next)
                {
                    case EngagementTierStatus.Reopened:
                    case EngagementTierStatus.Archived:
                        return true;
                }
                return false;
            case EngagementTierStatus.Reopened:
                return next == EngagementTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(EngagementTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"engagement tier"} {Code}";
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

    public double NetExposure()
    {
        return Math.Round((double)Revision - (double)Sequence, 2);
    }

    public EngagementTier Copy()
    {
        return new EngagementTier(Code, Label, Tags, Sequence, Timezone, Revision, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
