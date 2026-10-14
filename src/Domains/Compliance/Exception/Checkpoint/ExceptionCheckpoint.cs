namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception checkpoint is in its life cycle.</summary>
public enum ExceptionCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A exception checkpoint as the exception team keeps it.</summary>
public sealed class ExceptionCheckpoint(string code, string label, int updatedAt, double ratio, double threshold, double floorValue, double rate, double fte, bool archived, int batchSize, int startsAt, ExceptionCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public double Threshold { get; set; } = threshold;
    public double FloorValue { get; set; } = floorValue;
    public double Rate { get; set; } = rate;
    public double Fte { get; set; } = fte;
    public bool Archived { get; set; } = archived;
    public int BatchSize { get; set; } = batchSize;
    public int StartsAt { get; set; } = startsAt;
    public ExceptionCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a exception checkpoint in status created.</summary>
    public static ExceptionCheckpoint NewExceptionCheckpoint(string code, string label)
    {
        return new ExceptionCheckpoint(
            code,
            label,
            0,
            0.0,
            0.0,
            0.0,
            0.0,
            0.0,
            false,
            0,
            0,
            ExceptionCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionCheckpointStatus next)
    {
        switch (Status)
        {
            case ExceptionCheckpointStatus.Created:
                return next == ExceptionCheckpointStatus.Validated || next == ExceptionCheckpointStatus.Published;
            case ExceptionCheckpointStatus.Validated:
                switch (next)
                {
                    case ExceptionCheckpointStatus.Published:
                        return true;
                }
                return false;
            case ExceptionCheckpointStatus.Published:
                return next == ExceptionCheckpointStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ExceptionCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"exception checkpoint"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Threshold - Rate, 2);
    }

    public ExceptionCheckpoint Copy()
    {
        return new ExceptionCheckpoint(
            Code,
            Label,
            UpdatedAt,
            Ratio,
            Threshold,
            FloorValue,
            Rate,
            Fte,
            Archived,
            BatchSize,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
