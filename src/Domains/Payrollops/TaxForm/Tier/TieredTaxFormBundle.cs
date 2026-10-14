namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered tax form bundle is in its life cycle.</summary>
public enum TieredTaxFormBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One tiered tax form bundle, with what the planners need to act on it.</summary>
public sealed class TieredTaxFormBundle(string code, string name, double threshold, double score, string summary, string currency, int sequence, double amount, double budget, TieredTaxFormBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Score { get; set; } = score;
    public string Summary { get; set; } = summary;
    public string Currency { get; set; } = currency;
    public int Sequence { get; set; } = sequence;
    public double Amount { get; set; } = amount;
    public double Budget { get; set; } = budget;
    public TieredTaxFormBundleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered tax form bundle in status requested.</summary>
    public static TieredTaxFormBundle NewTieredTaxFormBundle(string code, string name)
    {
        return new TieredTaxFormBundle(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            0,
            0.0,
            0.0,
            TieredTaxFormBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredTaxFormBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredTaxFormBundleStatus next)
    {
        switch (Status)
        {
            case TieredTaxFormBundleStatus.Requested:
                switch (next)
                {
                    case TieredTaxFormBundleStatus.Expired:
                        return true;
                }
                return false;
            case TieredTaxFormBundleStatus.Granted:
                return next == TieredTaxFormBundleStatus.Revoked || next == TieredTaxFormBundleStatus.Expired;
            case TieredTaxFormBundleStatus.Revoked:
                return next == TieredTaxFormBundleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredTaxFormBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered tax form bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Sequence * Budget, 2);
    }

    public TieredTaxFormBundle Copy()
    {
        return new TieredTaxFormBundle(
            Code,
            Name,
            Threshold,
            Score,
            Summary,
            Currency,
            Sequence,
            Amount,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
