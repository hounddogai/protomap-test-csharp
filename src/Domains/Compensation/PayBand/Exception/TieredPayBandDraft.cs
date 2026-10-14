namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pay band draft is in its life cycle.</summary>
public enum TieredPayBandDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered pay band draft. Its code is unique within a tenant.</summary>
public sealed class TieredPayBandDraft(string code, string label, double hours, int sequence, string channel, TieredPayBandDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int Sequence { get; set; } = sequence;
    public string Channel { get; set; } = channel;
    public TieredPayBandDraftStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pay band draft in status planned.</summary>
    public static TieredPayBandDraft NewTieredPayBandDraft(string code, string label)
    {
        return new TieredPayBandDraft(code, label, 0.0, 0, "", TieredPayBandDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPayBandDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPayBandDraftStatus next)
    {
        switch (Status)
        {
            case TieredPayBandDraftStatus.Planned:
                return next == TieredPayBandDraftStatus.InProgress || next == TieredPayBandDraftStatus.Cancelled;
            case TieredPayBandDraftStatus.Scheduled:
                switch (next)
                {
                    case TieredPayBandDraftStatus.InProgress:
                    case TieredPayBandDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredPayBandDraftStatus.InProgress:
                return next == TieredPayBandDraftStatus.Completed || next == TieredPayBandDraftStatus.Cancelled;
            case TieredPayBandDraftStatus.Completed:
                return next == TieredPayBandDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredPayBandDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pay band draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round(Hours * (double)Sequence, 2);
    }

    public TieredPayBandDraft Copy()
    {
        return new TieredPayBandDraft(Code, Label, Hours, Sequence, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
