namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction entry is in its life cycle.</summary>
public enum DeductionEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One deduction entry, with what the planners need to act on it.</summary>
public sealed class DeductionEntry(string code, string label, string channel, string country, string phone, double score, DeductionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public string Phone { get; set; } = phone;
    public double Score { get; set; } = score;
    public DeductionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a deduction entry in status proposed.</summary>
    public static DeductionEntry NewDeductionEntry(string code, string label)
    {
        return new DeductionEntry(code, label, "", "", "", 0.0, DeductionEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionEntryStatus next)
    {
        switch (Status)
        {
            case DeductionEntryStatus.Proposed:
                switch (next)
                {
                    case DeductionEntryStatus.Approved:
                    case DeductionEntryStatus.Rejected:
                        return true;
                }
                return false;
            case DeductionEntryStatus.Approved:
                switch (next)
                {
                    case DeductionEntryStatus.Rejected:
                    case DeductionEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeductionEntryStatus.Rejected:
                return next == DeductionEntryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeductionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public DeductionEntry Copy()
    {
        return new DeductionEntry(Code, Label, Channel, Country, Phone, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
