namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor ledger is in its life cycle.</summary>
public enum VisitorLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a visitor ledger and its current status.</summary>
public sealed class VisitorLedger(string code, string title, int leaveDays, string description, string ownerId, bool pinned, double maximum, double discount, string location, int updatedAt, int expiresAt, VisitorLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Description { get; set; } = description;
    public string OwnerId { get; set; } = ownerId;
    public bool Pinned { get; set; } = pinned;
    public double Maximum { get; set; } = maximum;
    public double Discount { get; set; } = discount;
    public string Location { get; set; } = location;
    public int UpdatedAt { get; set; } = updatedAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public VisitorLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a visitor ledger in status created.</summary>
    public static VisitorLedger NewVisitorLedger(string code, string title)
    {
        return new VisitorLedger(code, title, 0, "", "", false, 0.0, 0.0, "", 0, 0, VisitorLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorLedgerStatus next)
    {
        switch (Status)
        {
            case VisitorLedgerStatus.Created:
                return next == VisitorLedgerStatus.Retired;
            case VisitorLedgerStatus.Validated:
                switch (next)
                {
                    case VisitorLedgerStatus.Published:
                    case VisitorLedgerStatus.Retired:
                        return true;
                }
                return false;
            case VisitorLedgerStatus.Published:
                switch (next)
                {
                    case VisitorLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"visitor ledger"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Maximum * (double)UpdatedAt, 2);
    }

    public VisitorLedger Copy()
    {
        return new VisitorLedger(
            Code,
            Title,
            LeaveDays,
            Description,
            OwnerId,
            Pinned,
            Maximum,
            Discount,
            Location,
            UpdatedAt,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
