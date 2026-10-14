namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday batch is in its life cycle.</summary>
public enum HolidayBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a holiday batch and its current status.</summary>
public sealed class HolidayBatch(string code, string title, string channel, string description, string segment, double threshold, string country, HolidayBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public double Threshold { get; set; } = threshold;
    public string Country { get; set; } = country;
    public HolidayBatchStatus Status { get; set; } = status;

    /// <summary>Starts a holiday batch in status requested.</summary>
    public static HolidayBatch NewHolidayBatch(string code, string title)
    {
        return new HolidayBatch(code, title, "", "", "", 0.0, "", HolidayBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayBatchStatus next)
    {
        switch (Status)
        {
            case HolidayBatchStatus.Requested:
                return next == HolidayBatchStatus.Granted;
            case HolidayBatchStatus.Granted:
                switch (next)
                {
                    case HolidayBatchStatus.Revoked:
                    case HolidayBatchStatus.Expired:
                        return true;
                }
                return false;
            case HolidayBatchStatus.Revoked:
                return next == HolidayBatchStatus.Expired;
        }
        return false;
    }

    public void MoveTo(HolidayBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public HolidayBatch Copy()
    {
        return new HolidayBatch(Code, Title, Channel, Description, Segment, Threshold, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
