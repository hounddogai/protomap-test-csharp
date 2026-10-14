namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition threshold is in its life cycle.</summary>
public enum RecognitionThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A recognition threshold. Its code is unique within a tenant.</summary>
public sealed class RecognitionThreshold(string code, string title, string source, string timezone, string department, string name, string label, string currency, bool approved, double amount, RecognitionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Timezone { get; set; } = timezone;
    public string Department { get; set; } = department;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public bool Approved { get; set; } = approved;
    public double Amount { get; set; } = amount;
    public RecognitionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a recognition threshold in status requested.</summary>
    public static RecognitionThreshold NewRecognitionThreshold(string code, string title)
    {
        return new RecognitionThreshold(
            code,
            title,
            "",
            "",
            "",
            "",
            "",
            "",
            false,
            0.0,
            RecognitionThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionThresholdStatus next)
    {
        switch (Status)
        {
            case RecognitionThresholdStatus.Requested:
                switch (next)
                {
                    case RecognitionThresholdStatus.Revoked:
                        return true;
                }
                return false;
            case RecognitionThresholdStatus.Granted:
                switch (next)
                {
                    case RecognitionThresholdStatus.Revoked:
                    case RecognitionThresholdStatus.Expired:
                        return true;
                }
                return false;
            case RecognitionThresholdStatus.Revoked:
                return next == RecognitionThresholdStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RecognitionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"recognition threshold"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RecognitionThreshold Copy()
    {
        return new RecognitionThreshold(
            Code,
            Title,
            Source,
            Timezone,
            Department,
            Name,
            Label,
            Currency,
            Approved,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
