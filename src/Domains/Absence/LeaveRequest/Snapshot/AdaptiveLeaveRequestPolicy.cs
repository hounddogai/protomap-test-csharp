namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive leave request policy is in its life cycle.</summary>
public enum AdaptiveLeaveRequestPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive leave request policy, with what the planners need to act on it.</summary>
public sealed class AdaptiveLeaveRequestPolicy(string code, string name, double ratio, int retries, double margin, double minimum, string category, string jobCode, AdaptiveLeaveRequestPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public int Retries { get; set; } = retries;
    public double Margin { get; set; } = margin;
    public double Minimum { get; set; } = minimum;
    public string Category { get; set; } = category;
    public string JobCode { get; set; } = jobCode;
    public AdaptiveLeaveRequestPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive leave request policy in status idle.</summary>
    public static AdaptiveLeaveRequestPolicy NewAdaptiveLeaveRequestPolicy(string code, string name)
    {
        return new AdaptiveLeaveRequestPolicy(
            code,
            name,
            0.0,
            0,
            0.0,
            0.0,
            "",
            "",
            AdaptiveLeaveRequestPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveLeaveRequestPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveLeaveRequestPolicyStatus next)
    {
        switch (Status)
        {
            case AdaptiveLeaveRequestPolicyStatus.Idle:
                switch (next)
                {
                    case AdaptiveLeaveRequestPolicyStatus.Ready:
                        return true;
                }
                return false;
            case AdaptiveLeaveRequestPolicyStatus.Warming:
                switch (next)
                {
                    case AdaptiveLeaveRequestPolicyStatus.Ready:
                    case AdaptiveLeaveRequestPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveLeaveRequestPolicyStatus.Ready:
                return next == AdaptiveLeaveRequestPolicyStatus.Draining || next == AdaptiveLeaveRequestPolicyStatus.Stopped;
            case AdaptiveLeaveRequestPolicyStatus.Draining:
                return next == AdaptiveLeaveRequestPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveLeaveRequestPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive leave request policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Margin + Ratio, 2);
    }

    public AdaptiveLeaveRequestPolicy Copy()
    {
        return new AdaptiveLeaveRequestPolicy(
            Code,
            Name,
            Ratio,
            Retries,
            Margin,
            Minimum,
            Category,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
