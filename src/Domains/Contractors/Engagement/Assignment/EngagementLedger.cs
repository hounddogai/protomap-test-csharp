namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement ledger is in its life cycle.</summary>
public enum EngagementLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a engagement ledger and its current status.</summary>
public sealed class EngagementLedger(string code, string label, string ownerId, int attempts, double floorValue, string location, double rating, EngagementLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public int Attempts { get; set; } = attempts;
    public double FloorValue { get; set; } = floorValue;
    public string Location { get; set; } = location;
    public double Rating { get; set; } = rating;
    public EngagementLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a engagement ledger in status created.</summary>
    public static EngagementLedger NewEngagementLedger(string code, string label)
    {
        return new EngagementLedger(code, label, "", 0, 0.0, "", 0.0, EngagementLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementLedgerStatus next)
    {
        switch (Status)
        {
            case EngagementLedgerStatus.Created:
                switch (next)
                {
                    case EngagementLedgerStatus.Published:
                        return true;
                }
                return false;
            case EngagementLedgerStatus.Validated:
                return next == EngagementLedgerStatus.Published;
            case EngagementLedgerStatus.Published:
                switch (next)
                {
                    case EngagementLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"engagement ledger"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Attempts - FloorValue, 2);
    }

    public EngagementLedger Copy()
    {
        return new EngagementLedger(Code, Label, OwnerId, Attempts, FloorValue, Location, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
