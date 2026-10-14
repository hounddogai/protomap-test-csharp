namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending rate window is in its life cycle.</summary>
public enum PendingRateWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pending rate window, with what the planners need to act on it.</summary>
public sealed class PendingRateWindow(string code, string title, string channel, double score, int expiresAt, bool archived, int retries, int graceDays, int sequence, double rate, double cost, PendingRateWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Archived { get; set; } = archived;
    public int Retries { get; set; } = retries;
    public int GraceDays { get; set; } = graceDays;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public PendingRateWindowStatus Status { get; set; } = status;

    /// <summary>Starts a pending rate window in status created.</summary>
    public static PendingRateWindow NewPendingRateWindow(string code, string title)
    {
        return new PendingRateWindow(
            code,
            title,
            "",
            0.0,
            0,
            false,
            0,
            0,
            0,
            0.0,
            0.0,
            PendingRateWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRateWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRateWindowStatus next)
    {
        switch (Status)
        {
            case PendingRateWindowStatus.Created:
                return next == PendingRateWindowStatus.Validated;
            case PendingRateWindowStatus.Validated:
                return next == PendingRateWindowStatus.Published || next == PendingRateWindowStatus.Retired;
            case PendingRateWindowStatus.Published:
                switch (next)
                {
                    case PendingRateWindowStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingRateWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending rate window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending rate window"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost * Score, 2);
    }

    public PendingRateWindow Copy()
    {
        return new PendingRateWindow(
            Code,
            Title,
            Channel,
            Score,
            ExpiresAt,
            Archived,
            Retries,
            GraceDays,
            Sequence,
            Rate,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
