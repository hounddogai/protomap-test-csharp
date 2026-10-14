namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional recognition threshold is in its life cycle.</summary>
public enum RegionalRecognitionThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a regional recognition threshold and its current status.</summary>
public sealed class RegionalRecognitionThreshold(string code, string label, string description, string phone, List<string> tags, string country, double score, RegionalRecognitionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public double Score { get; set; } = score;
    public RegionalRecognitionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a regional recognition threshold in status planned.</summary>
    public static RegionalRecognitionThreshold NewRegionalRecognitionThreshold(string code, string label)
    {
        return new RegionalRecognitionThreshold(
            code,
            label,
            "",
            "",
            new List<string>(),
            "",
            0.0,
            RegionalRecognitionThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalRecognitionThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalRecognitionThresholdStatus next)
    {
        switch (Status)
        {
            case RegionalRecognitionThresholdStatus.Planned:
                switch (next)
                {
                    case RegionalRecognitionThresholdStatus.Completed:
                        return true;
                }
                return false;
            case RegionalRecognitionThresholdStatus.Scheduled:
                switch (next)
                {
                    case RegionalRecognitionThresholdStatus.InProgress:
                        return true;
                }
                return false;
            case RegionalRecognitionThresholdStatus.InProgress:
                return next == RegionalRecognitionThresholdStatus.Completed || next == RegionalRecognitionThresholdStatus.Cancelled;
            case RegionalRecognitionThresholdStatus.Completed:
                switch (next)
                {
                    case RegionalRecognitionThresholdStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalRecognitionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional recognition threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RegionalRecognitionThreshold Copy()
    {
        return new RegionalRecognitionThreshold(Code, Label, Description, Phone, Tags, Country, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
