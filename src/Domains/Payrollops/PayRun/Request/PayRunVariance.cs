namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run variance is in its life cycle.</summary>
public enum PayRunVarianceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pay run variance and its current status.</summary>
public sealed class PayRunVariance(string code, string title, int capacity, int quantity, int durationMinutes, double budget, PayRunVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public int Quantity { get; set; } = quantity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Budget { get; set; } = budget;
    public PayRunVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a pay run variance in status idle.</summary>
    public static PayRunVariance NewPayRunVariance(string code, string title)
    {
        return new PayRunVariance(code, title, 0, 0, 0, 0.0, PayRunVarianceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunVarianceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunVarianceStatus next)
    {
        switch (Status)
        {
            case PayRunVarianceStatus.Idle:
                return next == PayRunVarianceStatus.Ready || next == PayRunVarianceStatus.Draining;
            case PayRunVarianceStatus.Warming:
                return next == PayRunVarianceStatus.Ready || next == PayRunVarianceStatus.Draining;
            case PayRunVarianceStatus.Ready:
                return next == PayRunVarianceStatus.Draining || next == PayRunVarianceStatus.Stopped;
            case PayRunVarianceStatus.Draining:
                return next == PayRunVarianceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayRunVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)DurationMinutes - (double)Capacity, 2);
    }

    public PayRunVariance Copy()
    {
        return new PayRunVariance(Code, Title, Capacity, Quantity, DurationMinutes, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
