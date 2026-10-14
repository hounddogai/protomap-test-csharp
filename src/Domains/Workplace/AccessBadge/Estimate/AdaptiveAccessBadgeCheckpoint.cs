namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive access badge checkpoint is in its life cycle.</summary>
public enum AdaptiveAccessBadgeCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive access badge checkpoint, with what the planners need to act on it.</summary>
public sealed class AdaptiveAccessBadgeCheckpoint(string code, string name, double minimum, string notes, string phone, double discount, bool archived, double budget, double ceiling, string summary, AdaptiveAccessBadgeCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public double Discount { get; set; } = discount;
    public bool Archived { get; set; } = archived;
    public double Budget { get; set; } = budget;
    public double Ceiling { get; set; } = ceiling;
    public string Summary { get; set; } = summary;
    public AdaptiveAccessBadgeCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive access badge checkpoint in status new.</summary>
    public static AdaptiveAccessBadgeCheckpoint NewAdaptiveAccessBadgeCheckpoint(string code, string name)
    {
        return new AdaptiveAccessBadgeCheckpoint(
            code,
            name,
            0.0,
            "",
            "",
            0.0,
            false,
            0.0,
            0.0,
            "",
            AdaptiveAccessBadgeCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAccessBadgeCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAccessBadgeCheckpointStatus next)
    {
        switch (Status)
        {
            case AdaptiveAccessBadgeCheckpointStatus.New:
                return next == AdaptiveAccessBadgeCheckpointStatus.Done || next == AdaptiveAccessBadgeCheckpointStatus.Failed;
            case AdaptiveAccessBadgeCheckpointStatus.Queued:
                switch (next)
                {
                    case AdaptiveAccessBadgeCheckpointStatus.Done:
                        return true;
                }
                return false;
            case AdaptiveAccessBadgeCheckpointStatus.Running:
                return next == AdaptiveAccessBadgeCheckpointStatus.Failed;
            case AdaptiveAccessBadgeCheckpointStatus.Done:
                return next == AdaptiveAccessBadgeCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveAccessBadgeCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive access badge checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Discount * Ceiling, 2);
    }

    public AdaptiveAccessBadgeCheckpoint Copy()
    {
        return new AdaptiveAccessBadgeCheckpoint(
            Code,
            Name,
            Minimum,
            Notes,
            Phone,
            Discount,
            Archived,
            Budget,
            Ceiling,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
