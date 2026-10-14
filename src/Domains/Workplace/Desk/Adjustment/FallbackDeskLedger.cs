namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback desk ledger is in its life cycle.</summary>
public enum FallbackDeskLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a fallback desk ledger and its current status.</summary>
public sealed class FallbackDeskLedger(string code, string name, string category, string locale, int batchSize, bool archived, string timezone, Dictionary<string, string> attributes, double amount, FallbackDeskLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public int BatchSize { get; set; } = batchSize;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Amount { get; set; } = amount;
    public FallbackDeskLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a fallback desk ledger in status proposed.</summary>
    public static FallbackDeskLedger NewFallbackDeskLedger(string code, string name)
    {
        return new FallbackDeskLedger(
            code,
            name,
            "",
            "",
            0,
            false,
            "",
            new Dictionary<string, string>(),
            0.0,
            FallbackDeskLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackDeskLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackDeskLedgerStatus next)
    {
        switch (Status)
        {
            case FallbackDeskLedgerStatus.Proposed:
                switch (next)
                {
                    case FallbackDeskLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case FallbackDeskLedgerStatus.Approved:
                return next == FallbackDeskLedgerStatus.Rejected || next == FallbackDeskLedgerStatus.Withdrawn;
            case FallbackDeskLedgerStatus.Rejected:
                return next == FallbackDeskLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackDeskLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback desk ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback desk ledger"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round(Amount * (double)BatchSize, 2);
    }

    public FallbackDeskLedger Copy()
    {
        return new FallbackDeskLedger(
            Code,
            Name,
            Category,
            Locale,
            BatchSize,
            Archived,
            Timezone,
            Attributes,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
