namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment ledger is in its life cycle.</summary>
public enum GarnishmentLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a garnishment ledger and its current status.</summary>
public sealed class GarnishmentLedger(string code, string name, int updatedAt, int version, double floorValue, List<string> tags, string unit, GarnishmentLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Version { get; set; } = version;
    public double FloorValue { get; set; } = floorValue;
    public List<string> Tags { get; set; } = tags;
    public string Unit { get; set; } = unit;
    public GarnishmentLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment ledger in status proposed.</summary>
    public static GarnishmentLedger NewGarnishmentLedger(string code, string name)
    {
        return new GarnishmentLedger(
            code,
            name,
            0,
            0,
            0.0,
            new List<string>(),
            "",
            GarnishmentLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentLedgerStatus next)
    {
        switch (Status)
        {
            case GarnishmentLedgerStatus.Proposed:
                switch (next)
                {
                    case GarnishmentLedgerStatus.Approved:
                    case GarnishmentLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case GarnishmentLedgerStatus.Approved:
                return next == GarnishmentLedgerStatus.Rejected;
            case GarnishmentLedgerStatus.Rejected:
                return next == GarnishmentLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GarnishmentLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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
        return Math.Round(FloorValue - (double)UpdatedAt, 2);
    }

    public GarnishmentLedger Copy()
    {
        return new GarnishmentLedger(Code, Name, UpdatedAt, Version, FloorValue, Tags, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
