namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal adjustment reservation is in its life cycle.</summary>
public enum SeasonalAdjustmentReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One seasonal adjustment reservation, with what the planners need to act on it.</summary>
public sealed class SeasonalAdjustmentReservation(string code, string label, string source, double amount, int version, List<string> tags, string category, string description, double floorValue, int batchSize, SeasonalAdjustmentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Amount { get; set; } = amount;
    public int Version { get; set; } = version;
    public List<string> Tags { get; set; } = tags;
    public string Category { get; set; } = category;
    public string Description { get; set; } = description;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public SeasonalAdjustmentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal adjustment reservation in status draft.</summary>
    public static SeasonalAdjustmentReservation NewSeasonalAdjustmentReservation(string code, string label)
    {
        return new SeasonalAdjustmentReservation(
            code,
            label,
            "",
            0.0,
            0,
            new List<string>(),
            "",
            "",
            0.0,
            0,
            SeasonalAdjustmentReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAdjustmentReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAdjustmentReservationStatus next)
    {
        switch (Status)
        {
            case SeasonalAdjustmentReservationStatus.Draft:
                return next == SeasonalAdjustmentReservationStatus.Suspended || next == SeasonalAdjustmentReservationStatus.Closed;
            case SeasonalAdjustmentReservationStatus.Pending:
                switch (next)
                {
                    case SeasonalAdjustmentReservationStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalAdjustmentReservationStatus.Active:
                switch (next)
                {
                    case SeasonalAdjustmentReservationStatus.Suspended:
                    case SeasonalAdjustmentReservationStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalAdjustmentReservationStatus.Suspended:
                return next == SeasonalAdjustmentReservationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalAdjustmentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal adjustment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double NetVolume()
    {
        return Math.Round(Amount * FloorValue, 2);
    }

    public SeasonalAdjustmentReservation Copy()
    {
        return new SeasonalAdjustmentReservation(
            Code,
            Label,
            Source,
            Amount,
            Version,
            Tags,
            Category,
            Description,
            FloorValue,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
