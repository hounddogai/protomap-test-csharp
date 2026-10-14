namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk ledger is in its life cycle.</summary>
public enum DeskLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A desk ledger as the desk team keeps it.</summary>
public sealed class DeskLedger(string code, string title, Dictionary<string, string> attributes, double discount, bool active, string ownerId, string grade, string notes, double ratio, DeskLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Discount { get; set; } = discount;
    public bool Active { get; set; } = active;
    public string OwnerId { get; set; } = ownerId;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public DeskLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a desk ledger in status proposed.</summary>
    public static DeskLedger NewDeskLedger(string code, string title)
    {
        return new DeskLedger(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            false,
            "",
            "",
            "",
            0.0,
            DeskLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskLedgerStatus next)
    {
        switch (Status)
        {
            case DeskLedgerStatus.Proposed:
                return next == DeskLedgerStatus.Approved || next == DeskLedgerStatus.Withdrawn;
            case DeskLedgerStatus.Approved:
                return next == DeskLedgerStatus.Rejected || next == DeskLedgerStatus.Withdrawn;
            case DeskLedgerStatus.Rejected:
                return next == DeskLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeskLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double EffectiveExposure()
    {
        return Math.Round(Discount - Ratio, 2);
    }

    public DeskLedger Copy()
    {
        return new DeskLedger(Code, Title, Attributes, Discount, Active, OwnerId, Grade, Notes, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
