namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling visitor window is in its life cycle.</summary>
public enum RollingVisitorWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One rolling visitor window, with what the planners need to act on it.</summary>
public sealed class RollingVisitorWindow(string code, string name, int revision, double score, bool archived, double minimum, double rate, double threshold, Dictionary<string, string> attributes, double fte, string channel, RollingVisitorWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public double Rate { get; set; } = rate;
    public double Threshold { get; set; } = threshold;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public string Channel { get; set; } = channel;
    public RollingVisitorWindowStatus Status { get; set; } = status;

    /// <summary>Starts a rolling visitor window in status idle.</summary>
    public static RollingVisitorWindow NewRollingVisitorWindow(string code, string name)
    {
        return new RollingVisitorWindow(
            code,
            name,
            0,
            0.0,
            false,
            0.0,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            RollingVisitorWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingVisitorWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingVisitorWindowStatus next)
    {
        switch (Status)
        {
            case RollingVisitorWindowStatus.Idle:
                return next == RollingVisitorWindowStatus.Stopped;
            case RollingVisitorWindowStatus.Warming:
                return next == RollingVisitorWindowStatus.Draining || next == RollingVisitorWindowStatus.Stopped;
            case RollingVisitorWindowStatus.Ready:
                switch (next)
                {
                    case RollingVisitorWindowStatus.Draining:
                    case RollingVisitorWindowStatus.Stopped:
                        return true;
                }
                return false;
            case RollingVisitorWindowStatus.Draining:
                return next == RollingVisitorWindowStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingVisitorWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling visitor window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetExposure()
    {
        return Math.Round(Rate - Fte, 2);
    }

    public RollingVisitorWindow Copy()
    {
        return new RollingVisitorWindow(
            Code,
            Name,
            Revision,
            Score,
            Archived,
            Minimum,
            Rate,
            Threshold,
            Attributes,
            Fte,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
