namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback license seat ledger is in its life cycle.</summary>
public enum FallbackLicenseSeatLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One fallback license seat ledger, with what the planners need to act on it.</summary>
public sealed class FallbackLicenseSeatLedger(string code, string title, int sequence, bool approved, int version, bool archived, string segment, int expiresAt, string checksum, string channel, FallbackLicenseSeatLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public bool Approved { get; set; } = approved;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public string Channel { get; set; } = channel;
    public FallbackLicenseSeatLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a fallback license seat ledger in status created.</summary>
    public static FallbackLicenseSeatLedger NewFallbackLicenseSeatLedger(string code, string title)
    {
        return new FallbackLicenseSeatLedger(
            code,
            title,
            0,
            false,
            0,
            false,
            "",
            0,
            "",
            "",
            FallbackLicenseSeatLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackLicenseSeatLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackLicenseSeatLedgerStatus next)
    {
        switch (Status)
        {
            case FallbackLicenseSeatLedgerStatus.Created:
                switch (next)
                {
                    case FallbackLicenseSeatLedgerStatus.Published:
                    case FallbackLicenseSeatLedgerStatus.Retired:
                        return true;
                }
                return false;
            case FallbackLicenseSeatLedgerStatus.Validated:
                return next == FallbackLicenseSeatLedgerStatus.Published || next == FallbackLicenseSeatLedgerStatus.Retired;
            case FallbackLicenseSeatLedgerStatus.Published:
                switch (next)
                {
                    case FallbackLicenseSeatLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackLicenseSeatLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback license seat ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Version + (double)Sequence, 2);
    }

    public FallbackLicenseSeatLedger Copy()
    {
        return new FallbackLicenseSeatLedger(
            Code,
            Title,
            Sequence,
            Approved,
            Version,
            Archived,
            Segment,
            ExpiresAt,
            Checksum,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
