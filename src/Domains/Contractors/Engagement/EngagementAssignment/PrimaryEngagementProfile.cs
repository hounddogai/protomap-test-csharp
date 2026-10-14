namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary engagement profile is in its life cycle.</summary>
public enum PrimaryEngagementProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a primary engagement profile and its current status.</summary>
public sealed class PrimaryEngagementProfile(string code, string title, double maximum, int endsAt, string skill, Dictionary<string, string> attributes, bool visible, string source, string checksum, string channel, double budget, PrimaryEngagementProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Visible { get; set; } = visible;
    public string Source { get; set; } = source;
    public string Checksum { get; set; } = checksum;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public PrimaryEngagementProfileStatus Status { get; set; } = status;

    /// <summary>Starts a primary engagement profile in status planned.</summary>
    public static PrimaryEngagementProfile NewPrimaryEngagementProfile(string code, string title)
    {
        return new PrimaryEngagementProfile(
            code,
            title,
            0.0,
            0,
            "",
            new Dictionary<string, string>(),
            false,
            "",
            "",
            "",
            0.0,
            PrimaryEngagementProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryEngagementProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryEngagementProfileStatus next)
    {
        switch (Status)
        {
            case PrimaryEngagementProfileStatus.Planned:
                switch (next)
                {
                    case PrimaryEngagementProfileStatus.Scheduled:
                    case PrimaryEngagementProfileStatus.InProgress:
                        return true;
                }
                return false;
            case PrimaryEngagementProfileStatus.Scheduled:
                return next == PrimaryEngagementProfileStatus.InProgress;
            case PrimaryEngagementProfileStatus.InProgress:
                switch (next)
                {
                    case PrimaryEngagementProfileStatus.Completed:
                    case PrimaryEngagementProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryEngagementProfileStatus.Completed:
                return next == PrimaryEngagementProfileStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimaryEngagementProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary engagement profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double AdjustedLoad()
    {
        return Math.Round((double)EndsAt + Maximum, 2);
    }

    public PrimaryEngagementProfile Copy()
    {
        return new PrimaryEngagementProfile(
            Code,
            Title,
            Maximum,
            EndsAt,
            Skill,
            Attributes,
            Visible,
            Source,
            Checksum,
            Channel,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
