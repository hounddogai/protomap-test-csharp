namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position ledger is in its life cycle.</summary>
public enum LegacyPositionLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy position ledger and its current status.</summary>
public sealed class LegacyPositionLedger(string code, string title, double ratio, double ceiling, string badge, string checksum, int windowDays, int tenureMonths, string department, string source, string segment, LegacyPositionLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Ceiling { get; set; } = ceiling;
    public string Badge { get; set; } = badge;
    public string Checksum { get; set; } = checksum;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Department { get; set; } = department;
    public string Source { get; set; } = source;
    public string Segment { get; set; } = segment;
    public LegacyPositionLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position ledger in status new.</summary>
    public static LegacyPositionLedger NewLegacyPositionLedger(string code, string title)
    {
        return new LegacyPositionLedger(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            0,
            0,
            "",
            "",
            "",
            LegacyPositionLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionLedgerStatus next)
    {
        switch (Status)
        {
            case LegacyPositionLedgerStatus.New:
                return next == LegacyPositionLedgerStatus.Failed;
            case LegacyPositionLedgerStatus.Queued:
                return next == LegacyPositionLedgerStatus.Running || next == LegacyPositionLedgerStatus.Done;
            case LegacyPositionLedgerStatus.Running:
                return next == LegacyPositionLedgerStatus.Done || next == LegacyPositionLedgerStatus.Failed;
            case LegacyPositionLedgerStatus.Done:
                switch (next)
                {
                    case LegacyPositionLedgerStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPositionLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy position ledger"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ceiling - (double)WindowDays, 2);
    }

    public LegacyPositionLedger Copy()
    {
        return new LegacyPositionLedger(
            Code,
            Title,
            Ratio,
            Ceiling,
            Badge,
            Checksum,
            WindowDays,
            TenureMonths,
            Department,
            Source,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
