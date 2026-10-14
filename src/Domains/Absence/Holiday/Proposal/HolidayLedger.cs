namespace Acme.Workforce.Absence.Holiday.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday ledger is in its life cycle.</summary>
public enum HolidayLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A holiday ledger as the holiday team keeps it.</summary>
public sealed class HolidayLedger(string code, string label, int durationMinutes, int endsAt, double score, int windowDays, double margin, double ceiling, string grade, string ownerId, HolidayLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int EndsAt { get; set; } = endsAt;
    public double Score { get; set; } = score;
    public int WindowDays { get; set; } = windowDays;
    public double Margin { get; set; } = margin;
    public double Ceiling { get; set; } = ceiling;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public HolidayLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a holiday ledger in status open.</summary>
    public static HolidayLedger NewHolidayLedger(string code, string label)
    {
        return new HolidayLedger(code, label, 0, 0, 0.0, 0, 0.0, 0.0, "", "", HolidayLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayLedgerStatus next)
    {
        switch (Status)
        {
            case HolidayLedgerStatus.Open:
                switch (next)
                {
                    case HolidayLedgerStatus.Resolved:
                        return true;
                }
                return false;
            case HolidayLedgerStatus.InReview:
                switch (next)
                {
                    case HolidayLedgerStatus.Resolved:
                    case HolidayLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case HolidayLedgerStatus.Resolved:
                return next == HolidayLedgerStatus.Reopened || next == HolidayLedgerStatus.Archived;
            case HolidayLedgerStatus.Reopened:
                switch (next)
                {
                    case HolidayLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Margin * Score, 2);
    }

    public HolidayLedger Copy()
    {
        return new HolidayLedger(
            Code,
            Label,
            DurationMinutes,
            EndsAt,
            Score,
            WindowDays,
            Margin,
            Ceiling,
            Grade,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
