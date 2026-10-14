namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending laptop ledger is in its life cycle.</summary>
public enum PendingLaptopLedgerStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending laptop ledger as the laptop team keeps it.</summary>
public sealed class PendingLaptopLedger(string code, string name, int revision, bool pinned, int windowDays, int retries, double cost, bool approved, bool visible, Dictionary<string, string> attributes, string currency, PendingLaptopLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public int WindowDays { get; set; } = windowDays;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public bool Approved { get; set; } = approved;
    public bool Visible { get; set; } = visible;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Currency { get; set; } = currency;
    public PendingLaptopLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a pending laptop ledger in status requested.</summary>
    public static PendingLaptopLedger NewPendingLaptopLedger(string code, string name)
    {
        return new PendingLaptopLedger(
            code,
            name,
            0,
            false,
            0,
            0,
            0.0,
            false,
            false,
            new Dictionary<string, string>(),
            "",
            PendingLaptopLedgerStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLaptopLedgerStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLaptopLedgerStatus next)
    {
        switch (Status)
        {
            case PendingLaptopLedgerStatus.Requested:
                switch (next)
                {
                    case PendingLaptopLedgerStatus.Revoked:
                    case PendingLaptopLedgerStatus.Expired:
                        return true;
                }
                return false;
            case PendingLaptopLedgerStatus.Granted:
                return next == PendingLaptopLedgerStatus.Revoked;
            case PendingLaptopLedgerStatus.Revoked:
                switch (next)
                {
                    case PendingLaptopLedgerStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingLaptopLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending laptop ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetVolume()
    {
        return Math.Round((double)Retries * (double)Revision, 2);
    }

    public PendingLaptopLedger Copy()
    {
        return new PendingLaptopLedger(
            Code,
            Name,
            Revision,
            Pinned,
            WindowDays,
            Retries,
            Cost,
            Approved,
            Visible,
            Attributes,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
