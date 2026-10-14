namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position tier is in its life cycle.</summary>
public enum PositionTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One position tier, with what the planners need to act on it.</summary>
public sealed class PositionTier(string code, string title, double weight, string currency, int startsAt, string grade, PositionTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string Currency { get; set; } = currency;
    public int StartsAt { get; set; } = startsAt;
    public string Grade { get; set; } = grade;
    public PositionTierStatus Status { get; set; } = status;

    /// <summary>Starts a position tier in status planned.</summary>
    public static PositionTier NewPositionTier(string code, string title)
    {
        return new PositionTier(code, title, 0.0, "", 0, "", PositionTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionTierStatus next)
    {
        switch (Status)
        {
            case PositionTierStatus.Planned:
                return next == PositionTierStatus.Cancelled;
            case PositionTierStatus.Scheduled:
                switch (next)
                {
                    case PositionTierStatus.Completed:
                    case PositionTierStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionTierStatus.InProgress:
                switch (next)
                {
                    case PositionTierStatus.Completed:
                    case PositionTierStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionTierStatus.Completed:
                switch (next)
                {
                    case PositionTierStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Weight - (double)StartsAt, 2);
    }

    public PositionTier Copy()
    {
        return new PositionTier(Code, Title, Weight, Currency, StartsAt, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
