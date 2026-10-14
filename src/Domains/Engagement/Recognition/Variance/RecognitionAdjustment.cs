namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition adjustment is in its life cycle.</summary>
public enum RecognitionAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A recognition adjustment as the recognition team keeps it.</summary>
public sealed class RecognitionAdjustment(string code, string name, int attempts, string reviewer, double ceiling, double cost, double maximum, string description, string timezone, int sequence, string managerId, RecognitionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Reviewer { get; set; } = reviewer;
    public double Ceiling { get; set; } = ceiling;
    public double Cost { get; set; } = cost;
    public double Maximum { get; set; } = maximum;
    public string Description { get; set; } = description;
    public string Timezone { get; set; } = timezone;
    public int Sequence { get; set; } = sequence;
    public string ManagerId { get; set; } = managerId;
    public RecognitionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a recognition adjustment in status draft.</summary>
    public static RecognitionAdjustment NewRecognitionAdjustment(string code, string name)
    {
        return new RecognitionAdjustment(
            code,
            name,
            0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            "",
            0,
            "",
            RecognitionAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionAdjustmentStatus next)
    {
        switch (Status)
        {
            case RecognitionAdjustmentStatus.Draft:
                switch (next)
                {
                    case RecognitionAdjustmentStatus.Pending:
                        return true;
                }
                return false;
            case RecognitionAdjustmentStatus.Pending:
                return next == RecognitionAdjustmentStatus.Closed;
            case RecognitionAdjustmentStatus.Active:
                return next == RecognitionAdjustmentStatus.Suspended;
            case RecognitionAdjustmentStatus.Suspended:
                return next == RecognitionAdjustmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RecognitionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Maximum * (double)Attempts, 2);
    }

    public RecognitionAdjustment Copy()
    {
        return new RecognitionAdjustment(
            Code,
            Name,
            Attempts,
            Reviewer,
            Ceiling,
            Cost,
            Maximum,
            Description,
            Timezone,
            Sequence,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
