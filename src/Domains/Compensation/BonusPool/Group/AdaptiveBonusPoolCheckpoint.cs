namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive bonus pool checkpoint is in its life cycle.</summary>
public enum AdaptiveBonusPoolCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive bonus pool checkpoint as the bonus pool team keeps it.</summary>
public sealed class AdaptiveBonusPoolCheckpoint(string code, string name, int createdAt, string location, string checksum, Dictionary<string, string> attributes, string reviewer, string jobCode, string source, string notes, AdaptiveBonusPoolCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Reviewer { get; set; } = reviewer;
    public string JobCode { get; set; } = jobCode;
    public string Source { get; set; } = source;
    public string Notes { get; set; } = notes;
    public AdaptiveBonusPoolCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive bonus pool checkpoint in status open.</summary>
    public static AdaptiveBonusPoolCheckpoint NewAdaptiveBonusPoolCheckpoint(string code, string name)
    {
        return new AdaptiveBonusPoolCheckpoint(
            code,
            name,
            0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            "",
            "",
            AdaptiveBonusPoolCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBonusPoolCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBonusPoolCheckpointStatus next)
    {
        switch (Status)
        {
            case AdaptiveBonusPoolCheckpointStatus.Open:
                switch (next)
                {
                    case AdaptiveBonusPoolCheckpointStatus.Resolved:
                    case AdaptiveBonusPoolCheckpointStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveBonusPoolCheckpointStatus.InReview:
                switch (next)
                {
                    case AdaptiveBonusPoolCheckpointStatus.Resolved:
                        return true;
                }
                return false;
            case AdaptiveBonusPoolCheckpointStatus.Resolved:
                return next == AdaptiveBonusPoolCheckpointStatus.Reopened || next == AdaptiveBonusPoolCheckpointStatus.Archived;
            case AdaptiveBonusPoolCheckpointStatus.Reopened:
                return next == AdaptiveBonusPoolCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveBonusPoolCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive bonus pool checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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
        return (double)CreatedAt;
    }

    public AdaptiveBonusPoolCheckpoint Copy()
    {
        return new AdaptiveBonusPoolCheckpoint(
            Code,
            Name,
            CreatedAt,
            Location,
            Checksum,
            Attributes,
            Reviewer,
            JobCode,
            Source,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
