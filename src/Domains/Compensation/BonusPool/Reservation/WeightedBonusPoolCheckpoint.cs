namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted bonus pool checkpoint is in its life cycle.</summary>
public enum WeightedBonusPoolCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted bonus pool checkpoint. Its code is unique within a tenant.</summary>
public sealed class WeightedBonusPoolCheckpoint(string code, string label, double amount, List<string> tags, double ceiling, string category, int windowDays, string externalRef, string checksum, WeightedBonusPoolCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public List<string> Tags { get; set; } = tags;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public int WindowDays { get; set; } = windowDays;
    public string ExternalRef { get; set; } = externalRef;
    public string Checksum { get; set; } = checksum;
    public WeightedBonusPoolCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a weighted bonus pool checkpoint in status proposed.</summary>
    public static WeightedBonusPoolCheckpoint NewWeightedBonusPoolCheckpoint(string code, string label)
    {
        return new WeightedBonusPoolCheckpoint(
            code,
            label,
            0.0,
            new List<string>(),
            0.0,
            "",
            0,
            "",
            "",
            WeightedBonusPoolCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBonusPoolCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBonusPoolCheckpointStatus next)
    {
        switch (Status)
        {
            case WeightedBonusPoolCheckpointStatus.Proposed:
                return next == WeightedBonusPoolCheckpointStatus.Approved || next == WeightedBonusPoolCheckpointStatus.Withdrawn;
            case WeightedBonusPoolCheckpointStatus.Approved:
                switch (next)
                {
                    case WeightedBonusPoolCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedBonusPoolCheckpointStatus.Rejected:
                switch (next)
                {
                    case WeightedBonusPoolCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedBonusPoolCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted bonus pool checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted bonus pool checkpoint"} {Code}";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)WindowDays - Amount, 2);
    }

    public WeightedBonusPoolCheckpoint Copy()
    {
        return new WeightedBonusPoolCheckpoint(
            Code,
            Label,
            Amount,
            Tags,
            Ceiling,
            Category,
            WindowDays,
            ExternalRef,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
