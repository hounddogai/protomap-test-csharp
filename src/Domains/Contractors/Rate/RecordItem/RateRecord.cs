namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate record is in its life cycle.</summary>
public enum RateRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rate record as the rate team keeps it.</summary>
public sealed class RateRecord(string code, string title, string channel, string country, double score, string currency, int limit, string segment, RateRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public double Score { get; set; } = score;
    public string Currency { get; set; } = currency;
    public int Limit { get; set; } = limit;
    public string Segment { get; set; } = segment;
    public RateRecordStatus Status { get; set; } = status;

    /// <summary>Starts a rate record in status created.</summary>
    public static RateRecord NewRateRecord(string code, string title)
    {
        return new RateRecord(code, title, "", "", 0.0, "", 0, "", RateRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateRecordStatus next)
    {
        switch (Status)
        {
            case RateRecordStatus.Created:
                switch (next)
                {
                    case RateRecordStatus.Retired:
                        return true;
                }
                return false;
            case RateRecordStatus.Validated:
                return next == RateRecordStatus.Published || next == RateRecordStatus.Retired;
            case RateRecordStatus.Published:
                switch (next)
                {
                    case RateRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetVolume()
    {
        return Math.Round(Score - (double)Limit, 2);
    }

    public RateRecord Copy()
    {
        return new RateRecord(Code, Title, Channel, Country, Score, Currency, Limit, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
