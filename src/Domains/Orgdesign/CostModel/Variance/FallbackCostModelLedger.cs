namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback cost model ledger is in its life cycle.</summary>
public enum FallbackCostModelLedgerStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a fallback cost model ledger and its current status.</summary>
public sealed class FallbackCostModelLedger(string code, string title, int endsAt, string notes, string contactEmail, int durationMinutes, string grade, FallbackCostModelLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string Notes { get; set; } = notes;
    public string ContactEmail { get; set; } = contactEmail;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Grade { get; set; } = grade;
    public FallbackCostModelLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a fallback cost model ledger in status idle.</summary>
    public static FallbackCostModelLedger NewFallbackCostModelLedger(string code, string title)
    {
        return new FallbackCostModelLedger(code, title, 0, "", "", 0, "", FallbackCostModelLedgerStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCostModelLedgerStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCostModelLedgerStatus next)
    {
        switch (Status)
        {
            case FallbackCostModelLedgerStatus.Idle:
                switch (next)
                {
                    case FallbackCostModelLedgerStatus.Ready:
                    case FallbackCostModelLedgerStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackCostModelLedgerStatus.Warming:
                switch (next)
                {
                    case FallbackCostModelLedgerStatus.Ready:
                        return true;
                }
                return false;
            case FallbackCostModelLedgerStatus.Ready:
                return next == FallbackCostModelLedgerStatus.Stopped;
            case FallbackCostModelLedgerStatus.Draining:
                return next == FallbackCostModelLedgerStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackCostModelLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback cost model ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes * (double)EndsAt, 2);
    }

    public FallbackCostModelLedger Copy()
    {
        return new FallbackCostModelLedger(Code, Title, EndsAt, Notes, ContactEmail, DurationMinutes, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
