namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient reorg slot is in its life cycle.</summary>
public enum LenientReorgSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One lenient reorg slot, with what the planners need to act on it.</summary>
public sealed class LenientReorgSlot(string code, string label, string salaryBand, string externalRef, int sequence, string name, double budget, string notes, int attempts, LenientReorgSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string ExternalRef { get; set; } = externalRef;
    public int Sequence { get; set; } = sequence;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public LenientReorgSlotStatus Status { get; set; } = status;

    /// <summary>Starts a lenient reorg slot in status proposed.</summary>
    public static LenientReorgSlot NewLenientReorgSlot(string code, string label)
    {
        return new LenientReorgSlot(code, label, "", "", 0, "", 0.0, "", 0, LenientReorgSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientReorgSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientReorgSlotStatus next)
    {
        switch (Status)
        {
            case LenientReorgSlotStatus.Proposed:
                return next == LenientReorgSlotStatus.Withdrawn;
            case LenientReorgSlotStatus.Approved:
                switch (next)
                {
                    case LenientReorgSlotStatus.Rejected:
                        return true;
                }
                return false;
            case LenientReorgSlotStatus.Rejected:
                return next == LenientReorgSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientReorgSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient reorg slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round(Budget + (double)Attempts, 2);
    }

    public LenientReorgSlot Copy()
    {
        return new LenientReorgSlot(
            Code,
            Label,
            SalaryBand,
            ExternalRef,
            Sequence,
            Name,
            Budget,
            Notes,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
