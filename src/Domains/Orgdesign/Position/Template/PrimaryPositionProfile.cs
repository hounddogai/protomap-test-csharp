namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary position profile is in its life cycle.</summary>
public enum PrimaryPositionProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary position profile. Its code is unique within a tenant.</summary>
public sealed class PrimaryPositionProfile(string code, string title, string managerId, double minimum, string summary, int priority, string jobCode, PrimaryPositionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public double Minimum { get; set; } = minimum;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public string JobCode { get; set; } = jobCode;
    public PrimaryPositionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a primary position profile in status idle.</summary>
    public static PrimaryPositionProfile NewPrimaryPositionProfile(string code, string title)
    {
        return new PrimaryPositionProfile(code, title, "", 0.0, "", 0, "", PrimaryPositionProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPositionProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPositionProfileStatus next)
    {
        switch (Status)
        {
            case PrimaryPositionProfileStatus.Idle:
                switch (next)
                {
                    case PrimaryPositionProfileStatus.Warming:
                    case PrimaryPositionProfileStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryPositionProfileStatus.Warming:
                return next == PrimaryPositionProfileStatus.Draining || next == PrimaryPositionProfileStatus.Stopped;
            case PrimaryPositionProfileStatus.Ready:
                switch (next)
                {
                    case PrimaryPositionProfileStatus.Draining:
                    case PrimaryPositionProfileStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryPositionProfileStatus.Draining:
                return next == PrimaryPositionProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PrimaryPositionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary position profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary position profile"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Minimum * (double)Priority, 2);
    }

    public PrimaryPositionProfile Copy()
    {
        return new PrimaryPositionProfile(Code, Title, ManagerId, Minimum, Summary, Priority, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
