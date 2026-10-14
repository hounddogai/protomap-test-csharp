namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile slot is in its life cycle.</summary>
public enum SkillProfileSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A skill profile slot as the skill profile team keeps it.</summary>
public sealed class SkillProfileSlot(string code, string label, string checksum, int updatedAt, double minimum, int limit, int createdAt, string title, string externalRef, string reviewer, SkillProfileSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public int Limit { get; set; } = limit;
    public int CreatedAt { get; set; } = createdAt;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Reviewer { get; set; } = reviewer;
    public SkillProfileSlotStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile slot in status planned.</summary>
    public static SkillProfileSlot NewSkillProfileSlot(string code, string label)
    {
        return new SkillProfileSlot(code, label, "", 0, 0.0, 0, 0, "", "", "", SkillProfileSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileSlotStatus next)
    {
        switch (Status)
        {
            case SkillProfileSlotStatus.Planned:
                return next == SkillProfileSlotStatus.Completed;
            case SkillProfileSlotStatus.Scheduled:
                return next == SkillProfileSlotStatus.InProgress || next == SkillProfileSlotStatus.Completed;
            case SkillProfileSlotStatus.InProgress:
                return next == SkillProfileSlotStatus.Completed || next == SkillProfileSlotStatus.Cancelled;
            case SkillProfileSlotStatus.Completed:
                return next == SkillProfileSlotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SkillProfileSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)UpdatedAt * Minimum, 2);
    }

    public SkillProfileSlot Copy()
    {
        return new SkillProfileSlot(
            Code,
            Label,
            Checksum,
            UpdatedAt,
            Minimum,
            Limit,
            CreatedAt,
            Title,
            ExternalRef,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
