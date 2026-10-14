namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band ledger is in its life cycle.</summary>
public enum PayBandLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pay band ledger, with what the planners need to act on it.</summary>
public sealed class PayBandLedger(string code, string name, string phone, List<string> tags, int startsAt, string summary, string region, double hours, string contactEmail, PayBandLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public int StartsAt { get; set; } = startsAt;
    public string Summary { get; set; } = summary;
    public string Region { get; set; } = region;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public PayBandLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a pay band ledger in status draft.</summary>
    public static PayBandLedger NewPayBandLedger(string code, string name)
    {
        return new PayBandLedger(code, name, "", new List<string>(), 0, "", "", 0.0, "", PayBandLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandLedgerStatus next)
    {
        switch (Status)
        {
            case PayBandLedgerStatus.Draft:
                switch (next)
                {
                    case PayBandLedgerStatus.Pending:
                        return true;
                }
                return false;
            case PayBandLedgerStatus.Pending:
                return next == PayBandLedgerStatus.Closed;
            case PayBandLedgerStatus.Active:
                switch (next)
                {
                    case PayBandLedgerStatus.Suspended:
                    case PayBandLedgerStatus.Closed:
                        return true;
                }
                return false;
            case PayBandLedgerStatus.Suspended:
                return next == PayBandLedgerStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayBandLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pay band ledger"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)StartsAt + Hours, 2);
    }

    public PayBandLedger Copy()
    {
        return new PayBandLedger(Code, Name, Phone, Tags, StartsAt, Summary, Region, Hours, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
