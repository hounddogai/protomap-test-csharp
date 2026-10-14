namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback desk slot is in its life cycle.</summary>
public enum FallbackDeskSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a fallback desk slot and its current status.</summary>
public sealed class FallbackDeskSlot(string code, string title, Dictionary<string, string> attributes, string department, string costCenter, string salaryBand, double ceiling, FallbackDeskSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Department { get; set; } = department;
    public string CostCenter { get; set; } = costCenter;
    public string SalaryBand { get; set; } = salaryBand;
    public double Ceiling { get; set; } = ceiling;
    public FallbackDeskSlotStatus Status { get; set; } = status;

    /// <summary>Starts a fallback desk slot in status proposed.</summary>
    public static FallbackDeskSlot NewFallbackDeskSlot(string code, string title)
    {
        return new FallbackDeskSlot(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            FallbackDeskSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackDeskSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackDeskSlotStatus next)
    {
        switch (Status)
        {
            case FallbackDeskSlotStatus.Proposed:
                return next == FallbackDeskSlotStatus.Approved;
            case FallbackDeskSlotStatus.Approved:
                return next == FallbackDeskSlotStatus.Rejected;
            case FallbackDeskSlotStatus.Rejected:
                switch (next)
                {
                    case FallbackDeskSlotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackDeskSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback desk slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public FallbackDeskSlot Copy()
    {
        return new FallbackDeskSlot(Code, Title, Attributes, Department, CostCenter, SalaryBand, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
