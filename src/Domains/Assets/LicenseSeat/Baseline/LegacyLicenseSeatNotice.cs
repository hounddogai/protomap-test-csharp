namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy license seat notice is in its life cycle.</summary>
public enum LegacyLicenseSeatNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy license seat notice as the license seat team keeps it.</summary>
public sealed class LegacyLicenseSeatNotice(string code, string label, string jobCode, double maximum, int leaveDays, LegacyLicenseSeatNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public LegacyLicenseSeatNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a legacy license seat notice in status proposed.</summary>
    public static LegacyLicenseSeatNotice NewLegacyLicenseSeatNotice(string code, string label)
    {
        return new LegacyLicenseSeatNotice(code, label, "", 0.0, 0, LegacyLicenseSeatNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLicenseSeatNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLicenseSeatNoticeStatus next)
    {
        switch (Status)
        {
            case LegacyLicenseSeatNoticeStatus.Proposed:
                switch (next)
                {
                    case LegacyLicenseSeatNoticeStatus.Approved:
                        return true;
                }
                return false;
            case LegacyLicenseSeatNoticeStatus.Approved:
                switch (next)
                {
                    case LegacyLicenseSeatNoticeStatus.Rejected:
                    case LegacyLicenseSeatNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyLicenseSeatNoticeStatus.Rejected:
                return next == LegacyLicenseSeatNoticeStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyLicenseSeatNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy license seat notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"legacy license seat notice"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays - Maximum, 2);
    }

    public LegacyLicenseSeatNotice Copy()
    {
        return new LegacyLicenseSeatNotice(Code, Label, JobCode, Maximum, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
