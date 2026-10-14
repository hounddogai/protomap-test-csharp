namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition profile is in its life cycle.</summary>
public enum RecognitionProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A recognition profile as the recognition team keeps it.</summary>
public sealed class RecognitionProfile(string code, string label, string country, int batchSize, List<string> tags, int durationMinutes, string ownerId, RecognitionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int BatchSize { get; set; } = batchSize;
    public List<string> Tags { get; set; } = tags;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string OwnerId { get; set; } = ownerId;
    public RecognitionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a recognition profile in status requested.</summary>
    public static RecognitionProfile NewRecognitionProfile(string code, string label)
    {
        return new RecognitionProfile(
            code,
            label,
            "",
            0,
            new List<string>(),
            0,
            "",
            RecognitionProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionProfileStatus next)
    {
        switch (Status)
        {
            case RecognitionProfileStatus.Requested:
                switch (next)
                {
                    case RecognitionProfileStatus.Granted:
                        return true;
                }
                return false;
            case RecognitionProfileStatus.Granted:
                switch (next)
                {
                    case RecognitionProfileStatus.Revoked:
                    case RecognitionProfileStatus.Expired:
                        return true;
                }
                return false;
            case RecognitionProfileStatus.Revoked:
                return next == RecognitionProfileStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RecognitionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"recognition profile"} {Code}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)DurationMinutes * (double)BatchSize, 2);
    }

    public RecognitionProfile Copy()
    {
        return new RecognitionProfile(Code, Label, Country, BatchSize, Tags, DurationMinutes, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
