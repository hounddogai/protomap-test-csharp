namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback garnishment exception is in its life cycle.</summary>
public enum FallbackGarnishmentExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One fallback garnishment exception, with what the planners need to act on it.</summary>
public sealed class FallbackGarnishmentException(string code, string name, string region, string unit, double amount, double minimum, bool visible, int limit, FallbackGarnishmentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public bool Visible { get; set; } = visible;
    public int Limit { get; set; } = limit;
    public FallbackGarnishmentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a fallback garnishment exception in status idle.</summary>
    public static FallbackGarnishmentException NewFallbackGarnishmentException(string code, string name)
    {
        return new FallbackGarnishmentException(
            code,
            name,
            "",
            "",
            0.0,
            0.0,
            false,
            0,
            FallbackGarnishmentExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackGarnishmentExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackGarnishmentExceptionStatus next)
    {
        switch (Status)
        {
            case FallbackGarnishmentExceptionStatus.Idle:
                switch (next)
                {
                    case FallbackGarnishmentExceptionStatus.Warming:
                    case FallbackGarnishmentExceptionStatus.Ready:
                        return true;
                }
                return false;
            case FallbackGarnishmentExceptionStatus.Warming:
                switch (next)
                {
                    case FallbackGarnishmentExceptionStatus.Ready:
                    case FallbackGarnishmentExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackGarnishmentExceptionStatus.Ready:
                return next == FallbackGarnishmentExceptionStatus.Stopped;
            case FallbackGarnishmentExceptionStatus.Draining:
                return next == FallbackGarnishmentExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackGarnishmentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback garnishment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Amount + (double)Limit, 2);
    }

    public FallbackGarnishmentException Copy()
    {
        return new FallbackGarnishmentException(Code, Name, Region, Unit, Amount, Minimum, Visible, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
