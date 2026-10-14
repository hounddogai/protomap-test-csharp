namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered bonus pool batch is in its life cycle.</summary>
public enum TieredBonusPoolBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered bonus pool batch as the bonus pool team keeps it.</summary>
public sealed class TieredBonusPoolBatch(string code, string name, string costCenter, bool archived, string title, string notes, double amount, TieredBonusPoolBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public bool Archived { get; set; } = archived;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public double Amount { get; set; } = amount;
    public TieredBonusPoolBatchStatus Status { get; set; } = status;

    /// <summary>Starts a tiered bonus pool batch in status created.</summary>
    public static TieredBonusPoolBatch NewTieredBonusPoolBatch(string code, string name)
    {
        return new TieredBonusPoolBatch(code, name, "", false, "", "", 0.0, TieredBonusPoolBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBonusPoolBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBonusPoolBatchStatus next)
    {
        switch (Status)
        {
            case TieredBonusPoolBatchStatus.Created:
                return next == TieredBonusPoolBatchStatus.Retired;
            case TieredBonusPoolBatchStatus.Validated:
                switch (next)
                {
                    case TieredBonusPoolBatchStatus.Published:
                    case TieredBonusPoolBatchStatus.Retired:
                        return true;
                }
                return false;
            case TieredBonusPoolBatchStatus.Published:
                switch (next)
                {
                    case TieredBonusPoolBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBonusPoolBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered bonus pool batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public TieredBonusPoolBatch Copy()
    {
        return new TieredBonusPoolBatch(Code, Name, CostCenter, Archived, Title, Notes, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
