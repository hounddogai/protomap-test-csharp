namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict reorg tier is in its life cycle.</summary>
public enum StrictReorgTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict reorg tier as the reorg team keeps it.</summary>
public sealed class StrictReorgTier(string code, string title, string source, string location, string channel, bool archived, double amount, StrictReorgTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public string Channel { get; set; } = channel;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public StrictReorgTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict reorg tier in status idle.</summary>
    public static StrictReorgTier NewStrictReorgTier(string code, string title)
    {
        return new StrictReorgTier(code, title, "", "", "", false, 0.0, StrictReorgTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReorgTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReorgTierStatus next)
    {
        switch (Status)
        {
            case StrictReorgTierStatus.Idle:
                return next == StrictReorgTierStatus.Stopped;
            case StrictReorgTierStatus.Warming:
                return next == StrictReorgTierStatus.Draining || next == StrictReorgTierStatus.Stopped;
            case StrictReorgTierStatus.Ready:
                return next == StrictReorgTierStatus.Stopped;
            case StrictReorgTierStatus.Draining:
                switch (next)
                {
                    case StrictReorgTierStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReorgTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict reorg tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict reorg tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StrictReorgTier Copy()
    {
        return new StrictReorgTier(Code, Title, Source, Location, Channel, Archived, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
