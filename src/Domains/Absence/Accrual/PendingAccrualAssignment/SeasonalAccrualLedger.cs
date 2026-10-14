namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal accrual ledger is in its life cycle.</summary>
public enum SeasonalAccrualLedgerStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal accrual ledger. Its code is unique within a tenant.</summary>
public sealed class SeasonalAccrualLedger(string code, string title, double threshold, int quantity, string contactEmail, double ratio, int createdAt, SeasonalAccrualLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ratio { get; set; } = ratio;
    public int CreatedAt { get; set; } = createdAt;
    public SeasonalAccrualLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal accrual ledger in status idle.</summary>
    public static SeasonalAccrualLedger NewSeasonalAccrualLedger(string code, string title)
    {
        return new SeasonalAccrualLedger(code, title, 0.0, 0, "", 0.0, 0, SeasonalAccrualLedgerStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAccrualLedgerStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAccrualLedgerStatus next)
    {
        switch (Status)
        {
            case SeasonalAccrualLedgerStatus.Idle:
                switch (next)
                {
                    case SeasonalAccrualLedgerStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalAccrualLedgerStatus.Warming:
                switch (next)
                {
                    case SeasonalAccrualLedgerStatus.Ready:
                    case SeasonalAccrualLedgerStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalAccrualLedgerStatus.Ready:
                return next == SeasonalAccrualLedgerStatus.Draining || next == SeasonalAccrualLedgerStatus.Stopped;
            case SeasonalAccrualLedgerStatus.Draining:
                switch (next)
                {
                    case SeasonalAccrualLedgerStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAccrualLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal accrual ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal accrual ledger"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Threshold * (double)CreatedAt, 2);
    }

    public SeasonalAccrualLedger Copy()
    {
        return new SeasonalAccrualLedger(Code, Title, Threshold, Quantity, ContactEmail, Ratio, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
