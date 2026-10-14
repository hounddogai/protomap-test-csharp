namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run digest is in its life cycle.</summary>
public enum PayRunDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pay run digest and its current status.</summary>
public sealed class PayRunDigest(string code, string title, int graceDays, double fte, int windowDays, int capacity, string label, double amount, PayRunDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public double Fte { get; set; } = fte;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public PayRunDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pay run digest in status idle.</summary>
    public static PayRunDigest NewPayRunDigest(string code, string title)
    {
        return new PayRunDigest(code, title, 0, 0.0, 0, 0, "", 0.0, PayRunDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunDigestStatus next)
    {
        switch (Status)
        {
            case PayRunDigestStatus.Idle:
                switch (next)
                {
                    case PayRunDigestStatus.Ready:
                    case PayRunDigestStatus.Stopped:
                        return true;
                }
                return false;
            case PayRunDigestStatus.Warming:
                switch (next)
                {
                    case PayRunDigestStatus.Ready:
                        return true;
                }
                return false;
            case PayRunDigestStatus.Ready:
                return next == PayRunDigestStatus.Draining;
            case PayRunDigestStatus.Draining:
                return next == PayRunDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayRunDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Capacity + (double)GraceDays, 2);
    }

    public PayRunDigest Copy()
    {
        return new PayRunDigest(Code, Title, GraceDays, Fte, WindowDays, Capacity, Label, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
