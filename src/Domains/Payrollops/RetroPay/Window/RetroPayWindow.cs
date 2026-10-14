namespace Acme.Workforce.Payrollops.RetroPay.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay window is in its life cycle.</summary>
public enum RetroPayWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a retro pay window and its current status.</summary>
public sealed class RetroPayWindow(string code, string label, string salaryBand, int expiresAt, double fte, string source, RetroPayWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Fte { get; set; } = fte;
    public string Source { get; set; } = source;
    public RetroPayWindowStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay window in status idle.</summary>
    public static RetroPayWindow NewRetroPayWindow(string code, string label)
    {
        return new RetroPayWindow(code, label, "", 0, 0.0, "", RetroPayWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayWindowStatus next)
    {
        switch (Status)
        {
            case RetroPayWindowStatus.Idle:
                return next == RetroPayWindowStatus.Warming || next == RetroPayWindowStatus.Stopped;
            case RetroPayWindowStatus.Warming:
                return next == RetroPayWindowStatus.Ready || next == RetroPayWindowStatus.Draining;
            case RetroPayWindowStatus.Ready:
                return next == RetroPayWindowStatus.Draining;
            case RetroPayWindowStatus.Draining:
                return next == RetroPayWindowStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RetroPayWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"retro pay window"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)ExpiresAt - Fte, 2);
    }

    public RetroPayWindow Copy()
    {
        return new RetroPayWindow(Code, Label, SalaryBand, ExpiresAt, Fte, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
