namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant tier is in its life cycle.</summary>
public enum EquityGrantTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A equity grant tier as the equity grant team keeps it.</summary>
public sealed class EquityGrantTier(string code, string label, string location, int startsAt, string channel, double budget, EquityGrantTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public int StartsAt { get; set; } = startsAt;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public EquityGrantTierStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant tier in status open.</summary>
    public static EquityGrantTier NewEquityGrantTier(string code, string label)
    {
        return new EquityGrantTier(code, label, "", 0, "", 0.0, EquityGrantTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantTierStatus next)
    {
        switch (Status)
        {
            case EquityGrantTierStatus.Open:
                switch (next)
                {
                    case EquityGrantTierStatus.Resolved:
                    case EquityGrantTierStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantTierStatus.InReview:
                switch (next)
                {
                    case EquityGrantTierStatus.Resolved:
                    case EquityGrantTierStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantTierStatus.Resolved:
                return next == EquityGrantTierStatus.Reopened;
            case EquityGrantTierStatus.Reopened:
                switch (next)
                {
                    case EquityGrantTierStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"equity grant tier"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Budget - (double)StartsAt, 2);
    }

    public EquityGrantTier Copy()
    {
        return new EquityGrantTier(Code, Label, Location, StartsAt, Channel, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
