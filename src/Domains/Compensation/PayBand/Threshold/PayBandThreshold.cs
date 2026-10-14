namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band threshold is in its life cycle.</summary>
public enum PayBandThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pay band threshold. Its code is unique within a tenant.</summary>
public sealed class PayBandThreshold(string code, string label, bool visible, string team, int tenureMonths, string phone, string checksum, int leaveDays, PayBandThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Team { get; set; } = team;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Phone { get; set; } = phone;
    public string Checksum { get; set; } = checksum;
    public int LeaveDays { get; set; } = leaveDays;
    public PayBandThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a pay band threshold in status proposed.</summary>
    public static PayBandThreshold NewPayBandThreshold(string code, string label)
    {
        return new PayBandThreshold(code, label, false, "", 0, "", "", 0, PayBandThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandThresholdStatus next)
    {
        switch (Status)
        {
            case PayBandThresholdStatus.Proposed:
                return next == PayBandThresholdStatus.Rejected || next == PayBandThresholdStatus.Withdrawn;
            case PayBandThresholdStatus.Approved:
                switch (next)
                {
                    case PayBandThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayBandThresholdStatus.Rejected:
                return next == PayBandThresholdStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayBandThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay band threshold"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)LeaveDays + (double)TenureMonths, 2);
    }

    public PayBandThreshold Copy()
    {
        return new PayBandThreshold(Code, Label, Visible, Team, TenureMonths, Phone, Checksum, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
