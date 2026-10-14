namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop ledger is in its life cycle.</summary>
public enum LaptopLedgerStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A laptop ledger as the laptop team keeps it.</summary>
public sealed class LaptopLedger(string code, string label, string costCenter, int revision, int windowDays, double rating, string currency, LaptopLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int Revision { get; set; } = revision;
    public int WindowDays { get; set; } = windowDays;
    public double Rating { get; set; } = rating;
    public string Currency { get; set; } = currency;
    public LaptopLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a laptop ledger in status idle.</summary>
    public static LaptopLedger NewLaptopLedger(string code, string label)
    {
        return new LaptopLedger(code, label, "", 0, 0, 0.0, "", LaptopLedgerStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopLedgerStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopLedgerStatus next)
    {
        switch (Status)
        {
            case LaptopLedgerStatus.Idle:
                return next == LaptopLedgerStatus.Stopped;
            case LaptopLedgerStatus.Warming:
                switch (next)
                {
                    case LaptopLedgerStatus.Ready:
                    case LaptopLedgerStatus.Stopped:
                        return true;
                }
                return false;
            case LaptopLedgerStatus.Ready:
                return next == LaptopLedgerStatus.Stopped;
            case LaptopLedgerStatus.Draining:
                return next == LaptopLedgerStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LaptopLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Rating - (double)WindowDays, 2);
    }

    public LaptopLedger Copy()
    {
        return new LaptopLedger(Code, Label, CostCenter, Revision, WindowDays, Rating, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
