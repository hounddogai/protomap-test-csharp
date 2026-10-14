namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive refresh checkpoint is in its life cycle.</summary>
public enum AdaptiveRefreshCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adaptive refresh checkpoint, with what the planners need to act on it.</summary>
public sealed class AdaptiveRefreshCheckpoint(string code, string name, string contactEmail, double score, string grade, double rating, string title, bool pinned, AdaptiveRefreshCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public double Score { get; set; } = score;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public AdaptiveRefreshCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive refresh checkpoint in status open.</summary>
    public static AdaptiveRefreshCheckpoint NewAdaptiveRefreshCheckpoint(string code, string name)
    {
        return new AdaptiveRefreshCheckpoint(
            code,
            name,
            "",
            0.0,
            "",
            0.0,
            "",
            false,
            AdaptiveRefreshCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveRefreshCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveRefreshCheckpointStatus next)
    {
        switch (Status)
        {
            case AdaptiveRefreshCheckpointStatus.Open:
                return next == AdaptiveRefreshCheckpointStatus.Resolved || next == AdaptiveRefreshCheckpointStatus.Archived;
            case AdaptiveRefreshCheckpointStatus.InReview:
                return next == AdaptiveRefreshCheckpointStatus.Resolved || next == AdaptiveRefreshCheckpointStatus.Reopened;
            case AdaptiveRefreshCheckpointStatus.Resolved:
                return next == AdaptiveRefreshCheckpointStatus.Reopened || next == AdaptiveRefreshCheckpointStatus.Archived;
            case AdaptiveRefreshCheckpointStatus.Reopened:
                return next == AdaptiveRefreshCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveRefreshCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive refresh checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Rating * Score, 2);
    }

    public AdaptiveRefreshCheckpoint Copy()
    {
        return new AdaptiveRefreshCheckpoint(Code, Name, ContactEmail, Score, Grade, Rating, Title, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
