namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default bonus pool checkpoint is in its life cycle.</summary>
public enum DefaultBonusPoolCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default bonus pool checkpoint as the bonus pool team keeps it.</summary>
public sealed class DefaultBonusPoolCheckpoint(string code, string name, int quantity, double rating, List<string> tags, string timezone, int tenureMonths, double budget, DefaultBonusPoolCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Rating { get; set; } = rating;
    public List<string> Tags { get; set; } = tags;
    public string Timezone { get; set; } = timezone;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Budget { get; set; } = budget;
    public DefaultBonusPoolCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a default bonus pool checkpoint in status draft.</summary>
    public static DefaultBonusPoolCheckpoint NewDefaultBonusPoolCheckpoint(string code, string name)
    {
        return new DefaultBonusPoolCheckpoint(
            code,
            name,
            0,
            0.0,
            new List<string>(),
            "",
            0,
            0.0,
            DefaultBonusPoolCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBonusPoolCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBonusPoolCheckpointStatus next)
    {
        switch (Status)
        {
            case DefaultBonusPoolCheckpointStatus.Draft:
                switch (next)
                {
                    case DefaultBonusPoolCheckpointStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultBonusPoolCheckpointStatus.Pending:
                return next == DefaultBonusPoolCheckpointStatus.Suspended || next == DefaultBonusPoolCheckpointStatus.Closed;
            case DefaultBonusPoolCheckpointStatus.Active:
                return next == DefaultBonusPoolCheckpointStatus.Suspended || next == DefaultBonusPoolCheckpointStatus.Closed;
            case DefaultBonusPoolCheckpointStatus.Suspended:
                return next == DefaultBonusPoolCheckpointStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultBonusPoolCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default bonus pool checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double AdjustedValue()
    {
        return Math.Round((double)TenureMonths * Budget, 2);
    }

    public DefaultBonusPoolCheckpoint Copy()
    {
        return new DefaultBonusPoolCheckpoint(
            Code,
            Name,
            Quantity,
            Rating,
            Tags,
            Timezone,
            TenureMonths,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
