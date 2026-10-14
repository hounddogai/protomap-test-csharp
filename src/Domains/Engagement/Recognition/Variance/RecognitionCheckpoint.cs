namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition checkpoint is in its life cycle.</summary>
public enum RecognitionCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A recognition checkpoint as the recognition team keeps it.</summary>
public sealed class RecognitionCheckpoint(string code, string title, int createdAt, string team, string label, string name, double rate, int startsAt, string costCenter, RecognitionCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Team { get; set; } = team;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int StartsAt { get; set; } = startsAt;
    public string CostCenter { get; set; } = costCenter;
    public RecognitionCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a recognition checkpoint in status draft.</summary>
    public static RecognitionCheckpoint NewRecognitionCheckpoint(string code, string title)
    {
        return new RecognitionCheckpoint(code, title, 0, "", "", "", 0.0, 0, "", RecognitionCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionCheckpointStatus next)
    {
        switch (Status)
        {
            case RecognitionCheckpointStatus.Draft:
                return next == RecognitionCheckpointStatus.Pending || next == RecognitionCheckpointStatus.Active;
            case RecognitionCheckpointStatus.Pending:
                return next == RecognitionCheckpointStatus.Closed;
            case RecognitionCheckpointStatus.Active:
                return next == RecognitionCheckpointStatus.Suspended || next == RecognitionCheckpointStatus.Closed;
            case RecognitionCheckpointStatus.Suspended:
                switch (next)
                {
                    case RecognitionCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"recognition checkpoint"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate + (double)CreatedAt, 2);
    }

    public RecognitionCheckpoint Copy()
    {
        return new RecognitionCheckpoint(
            Code,
            Title,
            CreatedAt,
            Team,
            Label,
            Name,
            Rate,
            StartsAt,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
