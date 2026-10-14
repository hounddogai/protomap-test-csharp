namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback recognition baseline is in its life cycle.</summary>
public enum FallbackRecognitionBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A fallback recognition baseline as the recognition team keeps it.</summary>
public sealed class FallbackRecognitionBaseline(string code, string name, string unit, double cost, string location, string badge, int startsAt, string channel, double ratio, int capacity, FallbackRecognitionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public string Badge { get; set; } = badge;
    public int StartsAt { get; set; } = startsAt;
    public string Channel { get; set; } = channel;
    public double Ratio { get; set; } = ratio;
    public int Capacity { get; set; } = capacity;
    public FallbackRecognitionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a fallback recognition baseline in status draft.</summary>
    public static FallbackRecognitionBaseline NewFallbackRecognitionBaseline(string code, string name)
    {
        return new FallbackRecognitionBaseline(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0,
            "",
            0.0,
            0,
            FallbackRecognitionBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRecognitionBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRecognitionBaselineStatus next)
    {
        switch (Status)
        {
            case FallbackRecognitionBaselineStatus.Draft:
                switch (next)
                {
                    case FallbackRecognitionBaselineStatus.Pending:
                    case FallbackRecognitionBaselineStatus.Closed:
                        return true;
                }
                return false;
            case FallbackRecognitionBaselineStatus.Pending:
                return next == FallbackRecognitionBaselineStatus.Suspended;
            case FallbackRecognitionBaselineStatus.Active:
                switch (next)
                {
                    case FallbackRecognitionBaselineStatus.Closed:
                        return true;
                }
                return false;
            case FallbackRecognitionBaselineStatus.Suspended:
                return next == FallbackRecognitionBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FallbackRecognitionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback recognition baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Cost + (double)StartsAt, 2);
    }

    public FallbackRecognitionBaseline Copy()
    {
        return new FallbackRecognitionBaseline(
            Code,
            Name,
            Unit,
            Cost,
            Location,
            Badge,
            StartsAt,
            Channel,
            Ratio,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
