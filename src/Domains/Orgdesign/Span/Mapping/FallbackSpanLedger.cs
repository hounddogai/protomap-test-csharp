namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback span ledger is in its life cycle.</summary>
public enum FallbackSpanLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One fallback span ledger, with what the planners need to act on it.</summary>
public sealed class FallbackSpanLedger(string code, string name, int durationMinutes, int startsAt, int windowDays, FallbackSpanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public FallbackSpanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a fallback span ledger in status planned.</summary>
    public static FallbackSpanLedger NewFallbackSpanLedger(string code, string name)
    {
        return new FallbackSpanLedger(code, name, 0, 0, 0, FallbackSpanLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSpanLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSpanLedgerStatus next)
    {
        switch (Status)
        {
            case FallbackSpanLedgerStatus.Planned:
                switch (next)
                {
                    case FallbackSpanLedgerStatus.Scheduled:
                        return true;
                }
                return false;
            case FallbackSpanLedgerStatus.Scheduled:
                switch (next)
                {
                    case FallbackSpanLedgerStatus.InProgress:
                    case FallbackSpanLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackSpanLedgerStatus.InProgress:
                switch (next)
                {
                    case FallbackSpanLedgerStatus.Completed:
                        return true;
                }
                return false;
            case FallbackSpanLedgerStatus.Completed:
                switch (next)
                {
                    case FallbackSpanLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSpanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback span ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)WindowDays * (double)DurationMinutes, 2);
    }

    public FallbackSpanLedger Copy()
    {
        return new FallbackSpanLedger(Code, Name, DurationMinutes, StartsAt, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
