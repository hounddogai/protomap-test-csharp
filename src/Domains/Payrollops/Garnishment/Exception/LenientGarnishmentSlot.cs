namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient garnishment slot is in its life cycle.</summary>
public enum LenientGarnishmentSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient garnishment slot. Its code is unique within a tenant.</summary>
public sealed class LenientGarnishmentSlot(string code, string title, string summary, string externalRef, string timezone, int sequence, double discount, LenientGarnishmentSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string ExternalRef { get; set; } = externalRef;
    public string Timezone { get; set; } = timezone;
    public int Sequence { get; set; } = sequence;
    public double Discount { get; set; } = discount;
    public LenientGarnishmentSlotStatus Status { get; set; } = status;

    /// <summary>Starts a lenient garnishment slot in status planned.</summary>
    public static LenientGarnishmentSlot NewLenientGarnishmentSlot(string code, string title)
    {
        return new LenientGarnishmentSlot(code, title, "", "", "", 0, 0.0, LenientGarnishmentSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientGarnishmentSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientGarnishmentSlotStatus next)
    {
        switch (Status)
        {
            case LenientGarnishmentSlotStatus.Planned:
                return next == LenientGarnishmentSlotStatus.Cancelled;
            case LenientGarnishmentSlotStatus.Scheduled:
                switch (next)
                {
                    case LenientGarnishmentSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientGarnishmentSlotStatus.InProgress:
                return next == LenientGarnishmentSlotStatus.Completed || next == LenientGarnishmentSlotStatus.Cancelled;
            case LenientGarnishmentSlotStatus.Completed:
                switch (next)
                {
                    case LenientGarnishmentSlotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientGarnishmentSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient garnishment slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Sequence * Discount, 2);
    }

    public LenientGarnishmentSlot Copy()
    {
        return new LenientGarnishmentSlot(Code, Title, Summary, ExternalRef, Timezone, Sequence, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
