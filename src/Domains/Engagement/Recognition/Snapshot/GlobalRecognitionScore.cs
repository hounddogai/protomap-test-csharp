namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global recognition score is in its life cycle.</summary>
public enum GlobalRecognitionScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A global recognition score as the recognition team keeps it.</summary>
public sealed class GlobalRecognitionScore(string code, string title, string timezone, bool visible, double floorValue, int limit, double ratio, int graceDays, int capacity, GlobalRecognitionScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public bool Visible { get; set; } = visible;
    public double FloorValue { get; set; } = floorValue;
    public int Limit { get; set; } = limit;
    public double Ratio { get; set; } = ratio;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public GlobalRecognitionScoreStatus Status { get; set; } = status;

    /// <summary>Starts a global recognition score in status draft.</summary>
    public static GlobalRecognitionScore NewGlobalRecognitionScore(string code, string title)
    {
        return new GlobalRecognitionScore(
            code,
            title,
            "",
            false,
            0.0,
            0,
            0.0,
            0,
            0,
            GlobalRecognitionScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalRecognitionScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalRecognitionScoreStatus next)
    {
        switch (Status)
        {
            case GlobalRecognitionScoreStatus.Draft:
                return next == GlobalRecognitionScoreStatus.Pending;
            case GlobalRecognitionScoreStatus.Pending:
                switch (next)
                {
                    case GlobalRecognitionScoreStatus.Active:
                    case GlobalRecognitionScoreStatus.Closed:
                        return true;
                }
                return false;
            case GlobalRecognitionScoreStatus.Active:
                switch (next)
                {
                    case GlobalRecognitionScoreStatus.Suspended:
                        return true;
                }
                return false;
            case GlobalRecognitionScoreStatus.Suspended:
                return next == GlobalRecognitionScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalRecognitionScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global recognition score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Limit + FloorValue, 2);
    }

    public GlobalRecognitionScore Copy()
    {
        return new GlobalRecognitionScore(
            Code,
            Title,
            Timezone,
            Visible,
            FloorValue,
            Limit,
            Ratio,
            GraceDays,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
