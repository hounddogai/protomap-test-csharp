namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient span ledger is in its life cycle.</summary>
public enum LenientSpanLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One lenient span ledger, with what the planners need to act on it.</summary>
public sealed class LenientSpanLedger(string code, string label, bool visible, int updatedAt, int revision, double hours, int endsAt, LenientSpanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Revision { get; set; } = revision;
    public double Hours { get; set; } = hours;
    public int EndsAt { get; set; } = endsAt;
    public LenientSpanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a lenient span ledger in status proposed.</summary>
    public static LenientSpanLedger NewLenientSpanLedger(string code, string label)
    {
        return new LenientSpanLedger(code, label, false, 0, 0, 0.0, 0, LenientSpanLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSpanLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSpanLedgerStatus next)
    {
        switch (Status)
        {
            case LenientSpanLedgerStatus.Proposed:
                return next == LenientSpanLedgerStatus.Approved || next == LenientSpanLedgerStatus.Rejected;
            case LenientSpanLedgerStatus.Approved:
                switch (next)
                {
                    case LenientSpanLedgerStatus.Rejected:
                        return true;
                }
                return false;
            case LenientSpanLedgerStatus.Rejected:
                switch (next)
                {
                    case LenientSpanLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientSpanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient span ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)EndsAt * Hours, 2);
    }

    public LenientSpanLedger Copy()
    {
        return new LenientSpanLedger(Code, Label, Visible, UpdatedAt, Revision, Hours, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
