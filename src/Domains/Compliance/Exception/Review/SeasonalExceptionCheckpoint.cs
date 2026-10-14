namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal exception checkpoint is in its life cycle.</summary>
public enum SeasonalExceptionCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A seasonal exception checkpoint as the exception team keeps it.</summary>
public sealed class SeasonalExceptionCheckpoint(string code, string label, double amount, string unit, double discount, string description, string country, double rating, int durationMinutes, string title, int updatedAt, SeasonalExceptionCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Unit { get; set; } = unit;
    public double Discount { get; set; } = discount;
    public string Description { get; set; } = description;
    public string Country { get; set; } = country;
    public double Rating { get; set; } = rating;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public SeasonalExceptionCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal exception checkpoint in status proposed.</summary>
    public static SeasonalExceptionCheckpoint NewSeasonalExceptionCheckpoint(string code, string label)
    {
        return new SeasonalExceptionCheckpoint(
            code,
            label,
            0.0,
            "",
            0.0,
            "",
            "",
            0.0,
            0,
            "",
            0,
            SeasonalExceptionCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalExceptionCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalExceptionCheckpointStatus next)
    {
        switch (Status)
        {
            case SeasonalExceptionCheckpointStatus.Proposed:
                switch (next)
                {
                    case SeasonalExceptionCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalExceptionCheckpointStatus.Approved:
                return next == SeasonalExceptionCheckpointStatus.Rejected || next == SeasonalExceptionCheckpointStatus.Withdrawn;
            case SeasonalExceptionCheckpointStatus.Rejected:
                return next == SeasonalExceptionCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalExceptionCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal exception checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"seasonal exception checkpoint"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Amount - Discount, 2);
    }

    public SeasonalExceptionCheckpoint Copy()
    {
        return new SeasonalExceptionCheckpoint(
            Code,
            Label,
            Amount,
            Unit,
            Discount,
            Description,
            Country,
            Rating,
            DurationMinutes,
            Title,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
