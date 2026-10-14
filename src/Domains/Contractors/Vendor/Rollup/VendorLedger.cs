namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor ledger is in its life cycle.</summary>
public enum VendorLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One vendor ledger, with what the planners need to act on it.</summary>
public sealed class VendorLedger(string code, string title, bool archived, string team, int windowDays, string skill, Dictionary<string, string> attributes, double margin, string label, VendorLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public string Skill { get; set; } = skill;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Margin { get; set; } = margin;
    public string Label { get; set; } = label;
    public VendorLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a vendor ledger in status created.</summary>
    public static VendorLedger NewVendorLedger(string code, string title)
    {
        return new VendorLedger(
            code,
            title,
            false,
            "",
            0,
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            VendorLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorLedgerStatus next)
    {
        switch (Status)
        {
            case VendorLedgerStatus.Created:
                return next == VendorLedgerStatus.Validated;
            case VendorLedgerStatus.Validated:
                return next == VendorLedgerStatus.Published;
            case VendorLedgerStatus.Published:
                switch (next)
                {
                    case VendorLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
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
        return Math.Round(Margin - (double)WindowDays, 2);
    }

    public VendorLedger Copy()
    {
        return new VendorLedger(Code, Title, Archived, Team, WindowDays, Skill, Attributes, Margin, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
