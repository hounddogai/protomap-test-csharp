namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered equity grant record is in its life cycle.</summary>
public enum TieredEquityGrantRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered equity grant record. Its code is unique within a tenant.</summary>
public sealed class TieredEquityGrantRecord(string code, string title, double budget, string notes, string country, int windowDays, int version, bool pinned, string jobCode, TieredEquityGrantRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public string Notes { get; set; } = notes;
    public string Country { get; set; } = country;
    public int WindowDays { get; set; } = windowDays;
    public int Version { get; set; } = version;
    public bool Pinned { get; set; } = pinned;
    public string JobCode { get; set; } = jobCode;
    public TieredEquityGrantRecordStatus Status { get; set; } = status;

    /// <summary>Starts a tiered equity grant record in status requested.</summary>
    public static TieredEquityGrantRecord NewTieredEquityGrantRecord(string code, string title)
    {
        return new TieredEquityGrantRecord(
            code,
            title,
            0.0,
            "",
            "",
            0,
            0,
            false,
            "",
            TieredEquityGrantRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredEquityGrantRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredEquityGrantRecordStatus next)
    {
        switch (Status)
        {
            case TieredEquityGrantRecordStatus.Requested:
                switch (next)
                {
                    case TieredEquityGrantRecordStatus.Expired:
                        return true;
                }
                return false;
            case TieredEquityGrantRecordStatus.Granted:
                switch (next)
                {
                    case TieredEquityGrantRecordStatus.Expired:
                        return true;
                }
                return false;
            case TieredEquityGrantRecordStatus.Revoked:
                switch (next)
                {
                    case TieredEquityGrantRecordStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredEquityGrantRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered equity grant record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"tiered equity grant record"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)WindowDays - (double)Version, 2);
    }

    public TieredEquityGrantRecord Copy()
    {
        return new TieredEquityGrantRecord(
            Code,
            Title,
            Budget,
            Notes,
            Country,
            WindowDays,
            Version,
            Pinned,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
