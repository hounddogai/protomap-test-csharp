namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay ledger is in its life cycle.</summary>
public enum RetroPayLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A retro pay ledger. Its code is unique within a tenant.</summary>
public sealed class RetroPayLedger(string code, string title, string notes, double minimum, int startsAt, RetroPayLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public double Minimum { get; set; } = minimum;
    public int StartsAt { get; set; } = startsAt;
    public RetroPayLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay ledger in status planned.</summary>
    public static RetroPayLedger NewRetroPayLedger(string code, string title)
    {
        return new RetroPayLedger(code, title, "", 0.0, 0, RetroPayLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayLedgerStatus next)
    {
        switch (Status)
        {
            case RetroPayLedgerStatus.Planned:
                return next == RetroPayLedgerStatus.Scheduled;
            case RetroPayLedgerStatus.Scheduled:
                return next == RetroPayLedgerStatus.InProgress || next == RetroPayLedgerStatus.Completed;
            case RetroPayLedgerStatus.InProgress:
                switch (next)
                {
                    case RetroPayLedgerStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayLedgerStatus.Completed:
                return next == RetroPayLedgerStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RetroPayLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)StartsAt * Minimum, 2);
    }

    public RetroPayLedger Copy()
    {
        return new RetroPayLedger(Code, Title, Notes, Minimum, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
