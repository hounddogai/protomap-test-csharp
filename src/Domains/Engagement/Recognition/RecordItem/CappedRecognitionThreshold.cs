namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped recognition threshold is in its life cycle.</summary>
public enum CappedRecognitionThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped recognition threshold as the recognition team keeps it.</summary>
public sealed class CappedRecognitionThreshold(string code, string title, string notes, string phone, string channel, int endsAt, Dictionary<string, string> attributes, int attempts, double ceiling, CappedRecognitionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public string Channel { get; set; } = channel;
    public int EndsAt { get; set; } = endsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public double Ceiling { get; set; } = ceiling;
    public CappedRecognitionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped recognition threshold in status new.</summary>
    public static CappedRecognitionThreshold NewCappedRecognitionThreshold(string code, string title)
    {
        return new CappedRecognitionThreshold(
            code,
            title,
            "",
            "",
            "",
            0,
            new Dictionary<string, string>(),
            0,
            0.0,
            CappedRecognitionThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRecognitionThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRecognitionThresholdStatus next)
    {
        switch (Status)
        {
            case CappedRecognitionThresholdStatus.New:
                switch (next)
                {
                    case CappedRecognitionThresholdStatus.Running:
                    case CappedRecognitionThresholdStatus.Failed:
                        return true;
                }
                return false;
            case CappedRecognitionThresholdStatus.Queued:
                switch (next)
                {
                    case CappedRecognitionThresholdStatus.Running:
                    case CappedRecognitionThresholdStatus.Failed:
                        return true;
                }
                return false;
            case CappedRecognitionThresholdStatus.Running:
                return next == CappedRecognitionThresholdStatus.Done;
            case CappedRecognitionThresholdStatus.Done:
                switch (next)
                {
                    case CappedRecognitionThresholdStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRecognitionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped recognition threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped recognition threshold"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Attempts + Ceiling, 2);
    }

    public CappedRecognitionThreshold Copy()
    {
        return new CappedRecognitionThreshold(
            Code,
            Title,
            Notes,
            Phone,
            Channel,
            EndsAt,
            Attributes,
            Attempts,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
