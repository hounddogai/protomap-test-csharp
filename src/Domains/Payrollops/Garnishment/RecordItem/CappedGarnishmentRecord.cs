namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped garnishment record is in its life cycle.</summary>
public enum CappedGarnishmentRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped garnishment record as the garnishment team keeps it.</summary>
public sealed class CappedGarnishmentRecord(string code, string name, int limit, int startsAt, double weight, string currency, bool approved, int retries, CappedGarnishmentRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public string Currency { get; set; } = currency;
    public bool Approved { get; set; } = approved;
    public int Retries { get; set; } = retries;
    public CappedGarnishmentRecordStatus Status { get; set; } = status;

    /// <summary>Starts a capped garnishment record in status created.</summary>
    public static CappedGarnishmentRecord NewCappedGarnishmentRecord(string code, string name)
    {
        return new CappedGarnishmentRecord(
            code,
            name,
            0,
            0,
            0.0,
            "",
            false,
            0,
            CappedGarnishmentRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedGarnishmentRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedGarnishmentRecordStatus next)
    {
        switch (Status)
        {
            case CappedGarnishmentRecordStatus.Created:
                return next == CappedGarnishmentRecordStatus.Retired;
            case CappedGarnishmentRecordStatus.Validated:
                switch (next)
                {
                    case CappedGarnishmentRecordStatus.Published:
                    case CappedGarnishmentRecordStatus.Retired:
                        return true;
                }
                return false;
            case CappedGarnishmentRecordStatus.Published:
                switch (next)
                {
                    case CappedGarnishmentRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedGarnishmentRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped garnishment record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)StartsAt - (double)Retries, 2);
    }

    public CappedGarnishmentRecord Copy()
    {
        return new CappedGarnishmentRecord(Code, Name, Limit, StartsAt, Weight, Currency, Approved, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
