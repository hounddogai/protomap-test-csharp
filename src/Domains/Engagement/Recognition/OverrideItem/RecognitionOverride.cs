namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition override is in its life cycle.</summary>
public enum RecognitionOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A recognition override. Its code is unique within a tenant.</summary>
public sealed class RecognitionOverride(string code, string label, string source, string title, string notes, double amount, RecognitionOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public double Amount { get; set; } = amount;
    public RecognitionOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a recognition override in status planned.</summary>
    public static RecognitionOverride NewRecognitionOverride(string code, string label)
    {
        return new RecognitionOverride(code, label, "", "", "", 0.0, RecognitionOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionOverrideStatus next)
    {
        switch (Status)
        {
            case RecognitionOverrideStatus.Planned:
                switch (next)
                {
                    case RecognitionOverrideStatus.InProgress:
                    case RecognitionOverrideStatus.Completed:
                        return true;
                }
                return false;
            case RecognitionOverrideStatus.Scheduled:
                switch (next)
                {
                    case RecognitionOverrideStatus.InProgress:
                    case RecognitionOverrideStatus.Completed:
                        return true;
                }
                return false;
            case RecognitionOverrideStatus.InProgress:
                return next == RecognitionOverrideStatus.Completed || next == RecognitionOverrideStatus.Cancelled;
            case RecognitionOverrideStatus.Completed:
                switch (next)
                {
                    case RecognitionOverrideStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"recognition override"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RecognitionOverride Copy()
    {
        return new RecognitionOverride(Code, Label, Source, Title, Notes, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
