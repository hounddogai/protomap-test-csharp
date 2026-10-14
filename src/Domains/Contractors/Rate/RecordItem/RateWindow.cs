namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate window is in its life cycle.</summary>
public enum RateWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a rate window and its current status.</summary>
public sealed class RateWindow(string code, string title, int expiresAt, double amount, int capacity, RateWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Amount { get; set; } = amount;
    public int Capacity { get; set; } = capacity;
    public RateWindowStatus Status { get; set; } = status;

    /// <summary>Starts a rate window in status idle.</summary>
    public static RateWindow NewRateWindow(string code, string title)
    {
        return new RateWindow(code, title, 0, 0.0, 0, RateWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateWindowStatus next)
    {
        switch (Status)
        {
            case RateWindowStatus.Idle:
                return next == RateWindowStatus.Warming || next == RateWindowStatus.Ready;
            case RateWindowStatus.Warming:
                return next == RateWindowStatus.Ready || next == RateWindowStatus.Draining;
            case RateWindowStatus.Ready:
                switch (next)
                {
                    case RateWindowStatus.Draining:
                        return true;
                }
                return false;
            case RateWindowStatus.Draining:
                return next == RateWindowStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RateWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Capacity + (double)ExpiresAt, 2);
    }

    public RateWindow Copy()
    {
        return new RateWindow(Code, Title, ExpiresAt, Amount, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
