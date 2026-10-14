namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition request is in its life cycle.</summary>
public enum RecognitionRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A recognition request as the recognition team keeps it.</summary>
public sealed class RecognitionRequest(string code, string title, string grade, double rate, double amount, int endsAt, int quantity, string notes, string label, RecognitionRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Rate { get; set; } = rate;
    public double Amount { get; set; } = amount;
    public int EndsAt { get; set; } = endsAt;
    public int Quantity { get; set; } = quantity;
    public string Notes { get; set; } = notes;
    public string Label { get; set; } = label;
    public RecognitionRequestStatus Status { get; set; } = status;

    /// <summary>Starts a recognition request in status idle.</summary>
    public static RecognitionRequest NewRecognitionRequest(string code, string title)
    {
        return new RecognitionRequest(code, title, "", 0.0, 0.0, 0, 0, "", "", RecognitionRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionRequestStatus next)
    {
        switch (Status)
        {
            case RecognitionRequestStatus.Idle:
                switch (next)
                {
                    case RecognitionRequestStatus.Stopped:
                        return true;
                }
                return false;
            case RecognitionRequestStatus.Warming:
                return next == RecognitionRequestStatus.Ready || next == RecognitionRequestStatus.Stopped;
            case RecognitionRequestStatus.Ready:
                return next == RecognitionRequestStatus.Stopped;
            case RecognitionRequestStatus.Draining:
                return next == RecognitionRequestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RecognitionRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"recognition request"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt - Amount, 2);
    }

    public RecognitionRequest Copy()
    {
        return new RecognitionRequest(Code, Title, Grade, Rate, Amount, EndsAt, Quantity, Notes, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
