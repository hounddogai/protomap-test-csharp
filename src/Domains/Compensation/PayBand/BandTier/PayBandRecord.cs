namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band record is in its life cycle.</summary>
public enum PayBandRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pay band record, with what the planners need to act on it.</summary>
public sealed class PayBandRecord(string code, string name, string reviewer, string source, string channel, string team, string phone, double amount, PayBandRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string Source { get; set; } = source;
    public string Channel { get; set; } = channel;
    public string Team { get; set; } = team;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public PayBandRecordStatus Status { get; set; } = status;

    /// <summary>Starts a pay band record in status proposed.</summary>
    public static PayBandRecord NewPayBandRecord(string code, string name)
    {
        return new PayBandRecord(code, name, "", "", "", "", "", 0.0, PayBandRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandRecordStatus next)
    {
        switch (Status)
        {
            case PayBandRecordStatus.Proposed:
                return next == PayBandRecordStatus.Rejected || next == PayBandRecordStatus.Withdrawn;
            case PayBandRecordStatus.Approved:
                switch (next)
                {
                    case PayBandRecordStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayBandRecordStatus.Rejected:
                switch (next)
                {
                    case PayBandRecordStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PayBandRecord Copy()
    {
        return new PayBandRecord(Code, Name, Reviewer, Source, Channel, Team, Phone, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
