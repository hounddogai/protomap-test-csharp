namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay exception is in its life cycle.</summary>
public enum RetroPayExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a retro pay exception and its current status.</summary>
public sealed class RetroPayException(string code, string label, string country, string grade, double ceiling, int capacity, int quantity, List<string> tags, double maximum, double score, RetroPayExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public int Capacity { get; set; } = capacity;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public RetroPayExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay exception in status idle.</summary>
    public static RetroPayException NewRetroPayException(string code, string label)
    {
        return new RetroPayException(
            code,
            label,
            "",
            "",
            0.0,
            0,
            0,
            new List<string>(),
            0.0,
            0.0,
            RetroPayExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayExceptionStatus next)
    {
        switch (Status)
        {
            case RetroPayExceptionStatus.Idle:
                return next == RetroPayExceptionStatus.Ready;
            case RetroPayExceptionStatus.Warming:
                return next == RetroPayExceptionStatus.Stopped;
            case RetroPayExceptionStatus.Ready:
                return next == RetroPayExceptionStatus.Draining || next == RetroPayExceptionStatus.Stopped;
            case RetroPayExceptionStatus.Draining:
                switch (next)
                {
                    case RetroPayExceptionStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"retro pay exception"} {Code}";
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

    public double ProjectedValue()
    {
        return Math.Round(Ceiling - (double)Capacity, 2);
    }

    public RetroPayException Copy()
    {
        return new RetroPayException(
            Code,
            Label,
            Country,
            Grade,
            Ceiling,
            Capacity,
            Quantity,
            Tags,
            Maximum,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
