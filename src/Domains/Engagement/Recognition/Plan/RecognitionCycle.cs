namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition cycle is in its life cycle.</summary>
public enum RecognitionCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A recognition cycle as the recognition team keeps it.</summary>
public sealed class RecognitionCycle(string code, string name, int graceDays, double cost, string category, string segment, bool approved, string channel, int expiresAt, double margin, double discount, RecognitionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public string Category { get; set; } = category;
    public string Segment { get; set; } = segment;
    public bool Approved { get; set; } = approved;
    public string Channel { get; set; } = channel;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public RecognitionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a recognition cycle in status draft.</summary>
    public static RecognitionCycle NewRecognitionCycle(string code, string name)
    {
        return new RecognitionCycle(
            code,
            name,
            0,
            0.0,
            "",
            "",
            false,
            "",
            0,
            0.0,
            0.0,
            RecognitionCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionCycleStatus next)
    {
        switch (Status)
        {
            case RecognitionCycleStatus.Draft:
                switch (next)
                {
                    case RecognitionCycleStatus.Pending:
                    case RecognitionCycleStatus.Active:
                        return true;
                }
                return false;
            case RecognitionCycleStatus.Pending:
                return next == RecognitionCycleStatus.Active;
            case RecognitionCycleStatus.Active:
                switch (next)
                {
                    case RecognitionCycleStatus.Suspended:
                    case RecognitionCycleStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionCycleStatus.Suspended:
                return next == RecognitionCycleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RecognitionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)ExpiresAt * Cost, 2);
    }

    public RecognitionCycle Copy()
    {
        return new RecognitionCycle(
            Code,
            Name,
            GraceDays,
            Cost,
            Category,
            Segment,
            Approved,
            Channel,
            ExpiresAt,
            Margin,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
