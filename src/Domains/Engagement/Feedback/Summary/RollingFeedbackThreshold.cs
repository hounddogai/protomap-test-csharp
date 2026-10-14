namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling feedback threshold is in its life cycle.</summary>
public enum RollingFeedbackThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One rolling feedback threshold, with what the planners need to act on it.</summary>
public sealed class RollingFeedbackThreshold(string code, string title, int updatedAt, string unit, string badge, int sequence, string description, List<string> tags, double maximum, Dictionary<string, string> attributes, int version, RollingFeedbackThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Unit { get; set; } = unit;
    public string Badge { get; set; } = badge;
    public int Sequence { get; set; } = sequence;
    public string Description { get; set; } = description;
    public List<string> Tags { get; set; } = tags;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Version { get; set; } = version;
    public RollingFeedbackThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a rolling feedback threshold in status open.</summary>
    public static RollingFeedbackThreshold NewRollingFeedbackThreshold(string code, string title)
    {
        return new RollingFeedbackThreshold(
            code,
            title,
            0,
            "",
            "",
            0,
            "",
            new List<string>(),
            0.0,
            new Dictionary<string, string>(),
            0,
            RollingFeedbackThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFeedbackThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFeedbackThresholdStatus next)
    {
        switch (Status)
        {
            case RollingFeedbackThresholdStatus.Open:
                return next == RollingFeedbackThresholdStatus.Resolved || next == RollingFeedbackThresholdStatus.Reopened;
            case RollingFeedbackThresholdStatus.InReview:
                switch (next)
                {
                    case RollingFeedbackThresholdStatus.Resolved:
                    case RollingFeedbackThresholdStatus.Reopened:
                        return true;
                }
                return false;
            case RollingFeedbackThresholdStatus.Resolved:
                switch (next)
                {
                    case RollingFeedbackThresholdStatus.Reopened:
                    case RollingFeedbackThresholdStatus.Archived:
                        return true;
                }
                return false;
            case RollingFeedbackThresholdStatus.Reopened:
                return next == RollingFeedbackThresholdStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingFeedbackThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling feedback threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double AdjustedExposure()
    {
        return Math.Round((double)Sequence * Maximum, 2);
    }

    public RollingFeedbackThreshold Copy()
    {
        return new RollingFeedbackThreshold(
            Code,
            Title,
            UpdatedAt,
            Unit,
            Badge,
            Sequence,
            Description,
            Tags,
            Maximum,
            Attributes,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
