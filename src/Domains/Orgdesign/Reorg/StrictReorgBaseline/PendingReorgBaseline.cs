namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending reorg baseline is in its life cycle.</summary>
public enum PendingReorgBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pending reorg baseline, with what the planners need to act on it.</summary>
public sealed class PendingReorgBaseline(string code, string label, string contactEmail, int endsAt, int graceDays, string badge, int priority, int quantity, PendingReorgBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public int EndsAt { get; set; } = endsAt;
    public int GraceDays { get; set; } = graceDays;
    public string Badge { get; set; } = badge;
    public int Priority { get; set; } = priority;
    public int Quantity { get; set; } = quantity;
    public PendingReorgBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pending reorg baseline in status idle.</summary>
    public static PendingReorgBaseline NewPendingReorgBaseline(string code, string label)
    {
        return new PendingReorgBaseline(code, label, "", 0, 0, "", 0, 0, PendingReorgBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingReorgBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingReorgBaselineStatus next)
    {
        switch (Status)
        {
            case PendingReorgBaselineStatus.Idle:
                return next == PendingReorgBaselineStatus.Warming || next == PendingReorgBaselineStatus.Draining;
            case PendingReorgBaselineStatus.Warming:
                return next == PendingReorgBaselineStatus.Ready || next == PendingReorgBaselineStatus.Stopped;
            case PendingReorgBaselineStatus.Ready:
                return next == PendingReorgBaselineStatus.Draining || next == PendingReorgBaselineStatus.Stopped;
            case PendingReorgBaselineStatus.Draining:
                return next == PendingReorgBaselineStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingReorgBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending reorg baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Priority - (double)GraceDays, 2);
    }

    public PendingReorgBaseline Copy()
    {
        return new PendingReorgBaseline(
            Code,
            Label,
            ContactEmail,
            EndsAt,
            GraceDays,
            Badge,
            Priority,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
