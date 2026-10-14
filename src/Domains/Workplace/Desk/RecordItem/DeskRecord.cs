namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk record is in its life cycle.</summary>
public enum DeskRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One desk record, with what the planners need to act on it.</summary>
public sealed class DeskRecord(string code, string title, string ownerId, string source, double discount, string timezone, double hours, DeskRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Source { get; set; } = source;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public DeskRecordStatus Status { get; set; } = status;

    /// <summary>Starts a desk record in status idle.</summary>
    public static DeskRecord NewDeskRecord(string code, string title)
    {
        return new DeskRecord(code, title, "", "", 0.0, "", 0.0, DeskRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskRecordStatus next)
    {
        switch (Status)
        {
            case DeskRecordStatus.Idle:
                return next == DeskRecordStatus.Stopped;
            case DeskRecordStatus.Warming:
                return next == DeskRecordStatus.Draining;
            case DeskRecordStatus.Ready:
                return next == DeskRecordStatus.Draining;
            case DeskRecordStatus.Draining:
                switch (next)
                {
                    case DeskRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Hours * Discount, 2);
    }

    public DeskRecord Copy()
    {
        return new DeskRecord(Code, Title, OwnerId, Source, Discount, Timezone, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
