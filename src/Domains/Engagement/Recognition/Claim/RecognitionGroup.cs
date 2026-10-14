namespace Acme.Workforce.Engagement.Recognition.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition group is in its life cycle.</summary>
public enum RecognitionGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a recognition group and its current status.</summary>
public sealed class RecognitionGroup(string code, string title, bool pinned, double cost, bool visible, RecognitionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public bool Visible { get; set; } = visible;
    public RecognitionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a recognition group in status idle.</summary>
    public static RecognitionGroup NewRecognitionGroup(string code, string title)
    {
        return new RecognitionGroup(code, title, false, 0.0, false, RecognitionGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionGroupStatus next)
    {
        switch (Status)
        {
            case RecognitionGroupStatus.Idle:
                switch (next)
                {
                    case RecognitionGroupStatus.Draining:
                        return true;
                }
                return false;
            case RecognitionGroupStatus.Warming:
                return next == RecognitionGroupStatus.Draining || next == RecognitionGroupStatus.Stopped;
            case RecognitionGroupStatus.Ready:
                switch (next)
                {
                    case RecognitionGroupStatus.Draining:
                        return true;
                }
                return false;
            case RecognitionGroupStatus.Draining:
                return next == RecognitionGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RecognitionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"recognition group"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public RecognitionGroup Copy()
    {
        return new RecognitionGroup(Code, Title, Pinned, Cost, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
