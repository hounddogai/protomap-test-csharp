namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band slot is in its life cycle.</summary>
public enum PayBandSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a pay band slot and its current status.</summary>
public sealed class PayBandSlot(string code, string title, double threshold, string source, string ownerId, PayBandSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public PayBandSlotStatus Status { get; set; } = status;

    /// <summary>Starts a pay band slot in status requested.</summary>
    public static PayBandSlot NewPayBandSlot(string code, string title)
    {
        return new PayBandSlot(code, title, 0.0, "", "", PayBandSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandSlotStatus next)
    {
        switch (Status)
        {
            case PayBandSlotStatus.Requested:
                return next == PayBandSlotStatus.Revoked;
            case PayBandSlotStatus.Granted:
                return next == PayBandSlotStatus.Revoked;
            case PayBandSlotStatus.Revoked:
                switch (next)
                {
                    case PayBandSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band slot"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public PayBandSlot Copy()
    {
        return new PayBandSlot(Code, Title, Threshold, Source, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
