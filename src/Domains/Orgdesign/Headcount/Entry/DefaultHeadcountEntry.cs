namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default headcount entry is in its life cycle.</summary>
public enum DefaultHeadcountEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One default headcount entry, with what the planners need to act on it.</summary>
public sealed class DefaultHeadcountEntry(string code, string label, string checksum, double floorValue, double weight, DefaultHeadcountEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public double FloorValue { get; set; } = floorValue;
    public double Weight { get; set; } = weight;
    public DefaultHeadcountEntryStatus Status { get; set; } = status;

    /// <summary>Starts a default headcount entry in status proposed.</summary>
    public static DefaultHeadcountEntry NewDefaultHeadcountEntry(string code, string label)
    {
        return new DefaultHeadcountEntry(code, label, "", 0.0, 0.0, DefaultHeadcountEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHeadcountEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHeadcountEntryStatus next)
    {
        switch (Status)
        {
            case DefaultHeadcountEntryStatus.Proposed:
                switch (next)
                {
                    case DefaultHeadcountEntryStatus.Rejected:
                    case DefaultHeadcountEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultHeadcountEntryStatus.Approved:
                return next == DefaultHeadcountEntryStatus.Rejected || next == DefaultHeadcountEntryStatus.Withdrawn;
            case DefaultHeadcountEntryStatus.Rejected:
                return next == DefaultHeadcountEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultHeadcountEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default headcount entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Weight - FloorValue, 2);
    }

    public DefaultHeadcountEntry Copy()
    {
        return new DefaultHeadcountEntry(Code, Label, Checksum, FloorValue, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
