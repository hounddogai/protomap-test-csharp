namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool checkpoint is in its life cycle.</summary>
public enum BonusPoolCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A bonus pool checkpoint. Its code is unique within a tenant.</summary>
public sealed class BonusPoolCheckpoint(string code, string label, string reviewer, string notes, string segment, bool archived, string title, bool visible, string phone, List<string> tags, string description, double amount, BonusPoolCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Notes { get; set; } = notes;
    public string Segment { get; set; } = segment;
    public bool Archived { get; set; } = archived;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public double Amount { get; set; } = amount;
    public BonusPoolCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool checkpoint in status draft.</summary>
    public static BonusPoolCheckpoint NewBonusPoolCheckpoint(string code, string label)
    {
        return new BonusPoolCheckpoint(
            code,
            label,
            "",
            "",
            "",
            false,
            "",
            false,
            "",
            new List<string>(),
            "",
            0.0,
            BonusPoolCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolCheckpointStatus next)
    {
        switch (Status)
        {
            case BonusPoolCheckpointStatus.Draft:
                switch (next)
                {
                    case BonusPoolCheckpointStatus.Pending:
                        return true;
                }
                return false;
            case BonusPoolCheckpointStatus.Pending:
                switch (next)
                {
                    case BonusPoolCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case BonusPoolCheckpointStatus.Active:
                return next == BonusPoolCheckpointStatus.Suspended || next == BonusPoolCheckpointStatus.Closed;
            case BonusPoolCheckpointStatus.Suspended:
                switch (next)
                {
                    case BonusPoolCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double MeasuredValue()
    {
        return Amount;
    }

    public BonusPoolCheckpoint Copy()
    {
        return new BonusPoolCheckpoint(
            Code,
            Label,
            Reviewer,
            Notes,
            Segment,
            Archived,
            Title,
            Visible,
            Phone,
            Tags,
            Description,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
