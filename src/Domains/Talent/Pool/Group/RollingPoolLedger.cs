namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pool ledger is in its life cycle.</summary>
public enum RollingPoolLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rolling pool ledger and its current status.</summary>
public sealed class RollingPoolLedger(string code, string label, double rate, string location, double margin, int attempts, int quantity, RollingPoolLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Location { get; set; } = location;
    public double Margin { get; set; } = margin;
    public int Attempts { get; set; } = attempts;
    public int Quantity { get; set; } = quantity;
    public RollingPoolLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pool ledger in status new.</summary>
    public static RollingPoolLedger NewRollingPoolLedger(string code, string label)
    {
        return new RollingPoolLedger(code, label, 0.0, "", 0.0, 0, 0, RollingPoolLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPoolLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPoolLedgerStatus next)
    {
        switch (Status)
        {
            case RollingPoolLedgerStatus.New:
                switch (next)
                {
                    case RollingPoolLedgerStatus.Running:
                    case RollingPoolLedgerStatus.Failed:
                        return true;
                }
                return false;
            case RollingPoolLedgerStatus.Queued:
                return next == RollingPoolLedgerStatus.Done;
            case RollingPoolLedgerStatus.Running:
                return next == RollingPoolLedgerStatus.Failed;
            case RollingPoolLedgerStatus.Done:
                switch (next)
                {
                    case RollingPoolLedgerStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPoolLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pool ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling pool ledger"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Rate - Margin, 2);
    }

    public RollingPoolLedger Copy()
    {
        return new RollingPoolLedger(Code, Label, Rate, Location, Margin, Attempts, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
