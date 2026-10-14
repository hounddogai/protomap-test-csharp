namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pool exception is in its life cycle.</summary>
public enum RollingPoolExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling pool exception. Its code is unique within a tenant.</summary>
public sealed class RollingPoolException(string code, string label, string externalRef, int leaveDays, double budget, int graceDays, RollingPoolExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public double Budget { get; set; } = budget;
    public int GraceDays { get; set; } = graceDays;
    public RollingPoolExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pool exception in status idle.</summary>
    public static RollingPoolException NewRollingPoolException(string code, string label)
    {
        return new RollingPoolException(code, label, "", 0, 0.0, 0, RollingPoolExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPoolExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPoolExceptionStatus next)
    {
        switch (Status)
        {
            case RollingPoolExceptionStatus.Idle:
                return next == RollingPoolExceptionStatus.Draining;
            case RollingPoolExceptionStatus.Warming:
                switch (next)
                {
                    case RollingPoolExceptionStatus.Ready:
                        return true;
                }
                return false;
            case RollingPoolExceptionStatus.Ready:
                return next == RollingPoolExceptionStatus.Draining;
            case RollingPoolExceptionStatus.Draining:
                switch (next)
                {
                    case RollingPoolExceptionStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPoolExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pool exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)GraceDays * Budget, 2);
    }

    public RollingPoolException Copy()
    {
        return new RollingPoolException(Code, Label, ExternalRef, LeaveDays, Budget, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
