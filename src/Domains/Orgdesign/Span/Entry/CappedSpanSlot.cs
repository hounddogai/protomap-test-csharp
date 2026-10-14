namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped span slot is in its life cycle.</summary>
public enum CappedSpanSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped span slot. Its code is unique within a tenant.</summary>
public sealed class CappedSpanSlot(string code, string label, string salaryBand, string phone, string name, string unit, string title, string summary, double score, CappedSpanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Phone { get; set; } = phone;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double Score { get; set; } = score;
    public CappedSpanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a capped span slot in status draft.</summary>
    public static CappedSpanSlot NewCappedSpanSlot(string code, string label)
    {
        return new CappedSpanSlot(code, label, "", "", "", "", "", "", 0.0, CappedSpanSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSpanSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSpanSlotStatus next)
    {
        switch (Status)
        {
            case CappedSpanSlotStatus.Draft:
                return next == CappedSpanSlotStatus.Pending || next == CappedSpanSlotStatus.Closed;
            case CappedSpanSlotStatus.Pending:
                switch (next)
                {
                    case CappedSpanSlotStatus.Closed:
                        return true;
                }
                return false;
            case CappedSpanSlotStatus.Active:
                switch (next)
                {
                    case CappedSpanSlotStatus.Closed:
                        return true;
                }
                return false;
            case CappedSpanSlotStatus.Suspended:
                return next == CappedSpanSlotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedSpanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped span slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped span slot"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public CappedSpanSlot Copy()
    {
        return new CappedSpanSlot(Code, Label, SalaryBand, Phone, Name, Unit, Title, Summary, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
