namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending garnishment tier is in its life cycle.</summary>
public enum PendingGarnishmentTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pending garnishment tier, with what the planners need to act on it.</summary>
public sealed class PendingGarnishmentTier(string code, string title, string description, int capacity, string source, int endsAt, int priority, int quantity, string badge, PendingGarnishmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public int Capacity { get; set; } = capacity;
    public string Source { get; set; } = source;
    public int EndsAt { get; set; } = endsAt;
    public int Priority { get; set; } = priority;
    public int Quantity { get; set; } = quantity;
    public string Badge { get; set; } = badge;
    public PendingGarnishmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a pending garnishment tier in status planned.</summary>
    public static PendingGarnishmentTier NewPendingGarnishmentTier(string code, string title)
    {
        return new PendingGarnishmentTier(code, title, "", 0, "", 0, 0, 0, "", PendingGarnishmentTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingGarnishmentTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingGarnishmentTierStatus next)
    {
        switch (Status)
        {
            case PendingGarnishmentTierStatus.Planned:
                return next == PendingGarnishmentTierStatus.InProgress;
            case PendingGarnishmentTierStatus.Scheduled:
                switch (next)
                {
                    case PendingGarnishmentTierStatus.InProgress:
                        return true;
                }
                return false;
            case PendingGarnishmentTierStatus.InProgress:
                return next == PendingGarnishmentTierStatus.Completed || next == PendingGarnishmentTierStatus.Cancelled;
            case PendingGarnishmentTierStatus.Completed:
                switch (next)
                {
                    case PendingGarnishmentTierStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingGarnishmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending garnishment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Capacity - (double)Priority, 2);
    }

    public PendingGarnishmentTier Copy()
    {
        return new PendingGarnishmentTier(
            Code,
            Title,
            Description,
            Capacity,
            Source,
            EndsAt,
            Priority,
            Quantity,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
