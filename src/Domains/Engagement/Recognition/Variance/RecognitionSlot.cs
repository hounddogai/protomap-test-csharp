namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition slot is in its life cycle.</summary>
public enum RecognitionSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a recognition slot and its current status.</summary>
public sealed class RecognitionSlot(string code, string name, string reviewer, string channel, string grade, int attempts, RecognitionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public int Attempts { get; set; } = attempts;
    public RecognitionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a recognition slot in status draft.</summary>
    public static RecognitionSlot NewRecognitionSlot(string code, string name)
    {
        return new RecognitionSlot(code, name, "", "", "", 0, RecognitionSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionSlotStatus next)
    {
        switch (Status)
        {
            case RecognitionSlotStatus.Draft:
                switch (next)
                {
                    case RecognitionSlotStatus.Suspended:
                    case RecognitionSlotStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionSlotStatus.Pending:
                return next == RecognitionSlotStatus.Closed;
            case RecognitionSlotStatus.Active:
                switch (next)
                {
                    case RecognitionSlotStatus.Suspended:
                    case RecognitionSlotStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionSlotStatus.Suspended:
                return next == RecognitionSlotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RecognitionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public RecognitionSlot Copy()
    {
        return new RecognitionSlot(Code, Name, Reviewer, Channel, Grade, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
