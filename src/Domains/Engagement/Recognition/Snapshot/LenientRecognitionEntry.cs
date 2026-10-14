namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient recognition entry is in its life cycle.</summary>
public enum LenientRecognitionEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient recognition entry. Its code is unique within a tenant.</summary>
public sealed class LenientRecognitionEntry(string code, string title, string summary, string description, int sequence, string channel, double hours, string team, double score, int tenureMonths, int graceDays, LenientRecognitionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string Description { get; set; } = description;
    public int Sequence { get; set; } = sequence;
    public string Channel { get; set; } = channel;
    public double Hours { get; set; } = hours;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public int TenureMonths { get; set; } = tenureMonths;
    public int GraceDays { get; set; } = graceDays;
    public LenientRecognitionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a lenient recognition entry in status planned.</summary>
    public static LenientRecognitionEntry NewLenientRecognitionEntry(string code, string title)
    {
        return new LenientRecognitionEntry(
            code,
            title,
            "",
            "",
            0,
            "",
            0.0,
            "",
            0.0,
            0,
            0,
            LenientRecognitionEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRecognitionEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRecognitionEntryStatus next)
    {
        switch (Status)
        {
            case LenientRecognitionEntryStatus.Planned:
                return next == LenientRecognitionEntryStatus.InProgress || next == LenientRecognitionEntryStatus.Cancelled;
            case LenientRecognitionEntryStatus.Scheduled:
                return next == LenientRecognitionEntryStatus.Completed || next == LenientRecognitionEntryStatus.Cancelled;
            case LenientRecognitionEntryStatus.InProgress:
                return next == LenientRecognitionEntryStatus.Completed || next == LenientRecognitionEntryStatus.Cancelled;
            case LenientRecognitionEntryStatus.Completed:
                switch (next)
                {
                    case LenientRecognitionEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientRecognitionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient recognition entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient recognition entry"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)GraceDays + (double)Sequence, 2);
    }

    public LenientRecognitionEntry Copy()
    {
        return new LenientRecognitionEntry(
            Code,
            Title,
            Summary,
            Description,
            Sequence,
            Channel,
            Hours,
            Team,
            Score,
            TenureMonths,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
