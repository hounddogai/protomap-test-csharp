namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted reorg target is in its life cycle.</summary>
public enum WeightedReorgTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One weighted reorg target, with what the planners need to act on it.</summary>
public sealed class WeightedReorgTarget(string code, string label, int expiresAt, int updatedAt, bool approved, int priority, WeightedReorgTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Approved { get; set; } = approved;
    public int Priority { get; set; } = priority;
    public WeightedReorgTargetStatus Status { get; set; } = status;

    /// <summary>Starts a weighted reorg target in status planned.</summary>
    public static WeightedReorgTarget NewWeightedReorgTarget(string code, string label)
    {
        return new WeightedReorgTarget(code, label, 0, 0, false, 0, WeightedReorgTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedReorgTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedReorgTargetStatus next)
    {
        switch (Status)
        {
            case WeightedReorgTargetStatus.Planned:
                switch (next)
                {
                    case WeightedReorgTargetStatus.Scheduled:
                        return true;
                }
                return false;
            case WeightedReorgTargetStatus.Scheduled:
                switch (next)
                {
                    case WeightedReorgTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedReorgTargetStatus.InProgress:
                return next == WeightedReorgTargetStatus.Completed;
            case WeightedReorgTargetStatus.Completed:
                switch (next)
                {
                    case WeightedReorgTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedReorgTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted reorg target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Priority * (double)UpdatedAt, 2);
    }

    public WeightedReorgTarget Copy()
    {
        return new WeightedReorgTarget(Code, Label, ExpiresAt, UpdatedAt, Approved, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
