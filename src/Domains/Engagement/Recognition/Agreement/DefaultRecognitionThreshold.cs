namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default recognition threshold is in its life cycle.</summary>
public enum DefaultRecognitionThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default recognition threshold, with what the planners need to act on it.</summary>
public sealed class DefaultRecognitionThreshold(string code, string title, string channel, int startsAt, string salaryBand, double floorValue, double rating, DefaultRecognitionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public double FloorValue { get; set; } = floorValue;
    public double Rating { get; set; } = rating;
    public DefaultRecognitionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a default recognition threshold in status requested.</summary>
    public static DefaultRecognitionThreshold NewDefaultRecognitionThreshold(string code, string title)
    {
        return new DefaultRecognitionThreshold(
            code,
            title,
            "",
            0,
            "",
            0.0,
            0.0,
            DefaultRecognitionThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultRecognitionThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultRecognitionThresholdStatus next)
    {
        switch (Status)
        {
            case DefaultRecognitionThresholdStatus.Requested:
                switch (next)
                {
                    case DefaultRecognitionThresholdStatus.Granted:
                    case DefaultRecognitionThresholdStatus.Expired:
                        return true;
                }
                return false;
            case DefaultRecognitionThresholdStatus.Granted:
                switch (next)
                {
                    case DefaultRecognitionThresholdStatus.Expired:
                        return true;
                }
                return false;
            case DefaultRecognitionThresholdStatus.Revoked:
                switch (next)
                {
                    case DefaultRecognitionThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultRecognitionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default recognition threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default recognition threshold"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(FloorValue * Rating, 2);
    }

    public DefaultRecognitionThreshold Copy()
    {
        return new DefaultRecognitionThreshold(
            Code,
            Title,
            Channel,
            StartsAt,
            SalaryBand,
            FloorValue,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
