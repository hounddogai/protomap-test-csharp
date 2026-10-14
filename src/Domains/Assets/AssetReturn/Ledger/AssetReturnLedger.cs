namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return ledger is in its life cycle.</summary>
public enum AssetReturnLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One asset return ledger, with what the planners need to act on it.</summary>
public sealed class AssetReturnLedger(string code, string title, int attempts, double threshold, string location, string currency, AssetReturnLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public double Threshold { get; set; } = threshold;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public AssetReturnLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a asset return ledger in status created.</summary>
    public static AssetReturnLedger NewAssetReturnLedger(string code, string title)
    {
        return new AssetReturnLedger(code, title, 0, 0.0, "", "", AssetReturnLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnLedgerStatus next)
    {
        switch (Status)
        {
            case AssetReturnLedgerStatus.Created:
                return next == AssetReturnLedgerStatus.Published || next == AssetReturnLedgerStatus.Retired;
            case AssetReturnLedgerStatus.Validated:
                switch (next)
                {
                    case AssetReturnLedgerStatus.Published:
                    case AssetReturnLedgerStatus.Retired:
                        return true;
                }
                return false;
            case AssetReturnLedgerStatus.Published:
                return next == AssetReturnLedgerStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AssetReturnLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"asset return ledger"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Threshold + (double)Attempts, 2);
    }

    public AssetReturnLedger Copy()
    {
        return new AssetReturnLedger(Code, Title, Attempts, Threshold, Location, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
