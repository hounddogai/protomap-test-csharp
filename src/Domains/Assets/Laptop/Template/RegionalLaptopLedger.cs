namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional laptop ledger is in its life cycle.</summary>
public enum RegionalLaptopLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a regional laptop ledger and its current status.</summary>
public sealed class RegionalLaptopLedger(string code, string title, int expiresAt, double amount, int quantity, double threshold, double cost, RegionalLaptopLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Amount { get; set; } = amount;
    public int Quantity { get; set; } = quantity;
    public double Threshold { get; set; } = threshold;
    public double Cost { get; set; } = cost;
    public RegionalLaptopLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a regional laptop ledger in status new.</summary>
    public static RegionalLaptopLedger NewRegionalLaptopLedger(string code, string title)
    {
        return new RegionalLaptopLedger(code, title, 0, 0.0, 0, 0.0, 0.0, RegionalLaptopLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalLaptopLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalLaptopLedgerStatus next)
    {
        switch (Status)
        {
            case RegionalLaptopLedgerStatus.New:
                switch (next)
                {
                    case RegionalLaptopLedgerStatus.Failed:
                        return true;
                }
                return false;
            case RegionalLaptopLedgerStatus.Queued:
                switch (next)
                {
                    case RegionalLaptopLedgerStatus.Done:
                    case RegionalLaptopLedgerStatus.Failed:
                        return true;
                }
                return false;
            case RegionalLaptopLedgerStatus.Running:
                return next == RegionalLaptopLedgerStatus.Failed;
            case RegionalLaptopLedgerStatus.Done:
                return next == RegionalLaptopLedgerStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalLaptopLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional laptop ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Cost * Amount, 2);
    }

    public RegionalLaptopLedger Copy()
    {
        return new RegionalLaptopLedger(Code, Title, ExpiresAt, Amount, Quantity, Threshold, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
