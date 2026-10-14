namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position slot is in its life cycle.</summary>
public enum PositionSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A position slot. Its code is unique within a tenant.</summary>
public sealed class PositionSlot(string code, string name, string channel, int capacity, string checksum, string contactEmail, string badge, List<string> tags, int attempts, int updatedAt, int sequence, PositionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int Capacity { get; set; } = capacity;
    public string Checksum { get; set; } = checksum;
    public string ContactEmail { get; set; } = contactEmail;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Sequence { get; set; } = sequence;
    public PositionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a position slot in status new.</summary>
    public static PositionSlot NewPositionSlot(string code, string name)
    {
        return new PositionSlot(code, name, "", 0, "", "", "", new List<string>(), 0, 0, 0, PositionSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionSlotStatus next)
    {
        switch (Status)
        {
            case PositionSlotStatus.New:
                return next == PositionSlotStatus.Queued;
            case PositionSlotStatus.Queued:
                return next == PositionSlotStatus.Running || next == PositionSlotStatus.Done;
            case PositionSlotStatus.Running:
                return next == PositionSlotStatus.Done || next == PositionSlotStatus.Failed;
            case PositionSlotStatus.Done:
                switch (next)
                {
                    case PositionSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveValue()
    {
        return Math.Round((double)Attempts * (double)Sequence, 2);
    }

    public PositionSlot Copy()
    {
        return new PositionSlot(
            Code,
            Name,
            Channel,
            Capacity,
            Checksum,
            ContactEmail,
            Badge,
            Tags,
            Attempts,
            UpdatedAt,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
