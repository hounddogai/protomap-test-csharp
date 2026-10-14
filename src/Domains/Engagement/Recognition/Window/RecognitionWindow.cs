namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition window is in its life cycle.</summary>
public enum RecognitionWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A recognition window as the recognition team keeps it.</summary>
public sealed class RecognitionWindow(string code, string label, int quantity, string description, int windowDays, string managerId, double maximum, RecognitionWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public string ManagerId { get; set; } = managerId;
    public double Maximum { get; set; } = maximum;
    public RecognitionWindowStatus Status { get; set; } = status;

    /// <summary>Starts a recognition window in status planned.</summary>
    public static RecognitionWindow NewRecognitionWindow(string code, string label)
    {
        return new RecognitionWindow(code, label, 0, "", 0, "", 0.0, RecognitionWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionWindowStatus next)
    {
        switch (Status)
        {
            case RecognitionWindowStatus.Planned:
                return next == RecognitionWindowStatus.InProgress;
            case RecognitionWindowStatus.Scheduled:
                return next == RecognitionWindowStatus.Completed;
            case RecognitionWindowStatus.InProgress:
                switch (next)
                {
                    case RecognitionWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case RecognitionWindowStatus.Completed:
                switch (next)
                {
                    case RecognitionWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)WindowDays - (double)Quantity, 2);
    }

    public RecognitionWindow Copy()
    {
        return new RecognitionWindow(Code, Label, Quantity, Description, WindowDays, ManagerId, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
