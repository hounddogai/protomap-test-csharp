namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered asset return ledger is in its life cycle.</summary>
public enum TieredAssetReturnLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One tiered asset return ledger, with what the planners need to act on it.</summary>
public sealed class TieredAssetReturnLedger(string code, string title, bool pinned, int durationMinutes, string segment, string name, string jobCode, int updatedAt, double rate, TieredAssetReturnLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Segment { get; set; } = segment;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rate { get; set; } = rate;
    public TieredAssetReturnLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a tiered asset return ledger in status draft.</summary>
    public static TieredAssetReturnLedger NewTieredAssetReturnLedger(string code, string title)
    {
        return new TieredAssetReturnLedger(
            code,
            title,
            false,
            0,
            "",
            "",
            "",
            0,
            0.0,
            TieredAssetReturnLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAssetReturnLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAssetReturnLedgerStatus next)
    {
        switch (Status)
        {
            case TieredAssetReturnLedgerStatus.Draft:
                switch (next)
                {
                    case TieredAssetReturnLedgerStatus.Active:
                    case TieredAssetReturnLedgerStatus.Suspended:
                        return true;
                }
                return false;
            case TieredAssetReturnLedgerStatus.Pending:
                switch (next)
                {
                    case TieredAssetReturnLedgerStatus.Active:
                        return true;
                }
                return false;
            case TieredAssetReturnLedgerStatus.Active:
                return next == TieredAssetReturnLedgerStatus.Suspended || next == TieredAssetReturnLedgerStatus.Closed;
            case TieredAssetReturnLedgerStatus.Suspended:
                return next == TieredAssetReturnLedgerStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TieredAssetReturnLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered asset return ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt - Rate, 2);
    }

    public TieredAssetReturnLedger Copy()
    {
        return new TieredAssetReturnLedger(
            Code,
            Title,
            Pinned,
            DurationMinutes,
            Segment,
            Name,
            JobCode,
            UpdatedAt,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
