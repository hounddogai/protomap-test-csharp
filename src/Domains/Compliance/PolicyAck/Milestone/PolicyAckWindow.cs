namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack window is in its life cycle.</summary>
public enum PolicyAckWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One policy ack window, with what the planners need to act on it.</summary>
public sealed class PolicyAckWindow(string code, string name, List<string> tags, string country, double ceiling, int windowDays, PolicyAckWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public double Ceiling { get; set; } = ceiling;
    public int WindowDays { get; set; } = windowDays;
    public PolicyAckWindowStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack window in status created.</summary>
    public static PolicyAckWindow NewPolicyAckWindow(string code, string name)
    {
        return new PolicyAckWindow(code, name, new List<string>(), "", 0.0, 0, PolicyAckWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckWindowStatus next)
    {
        switch (Status)
        {
            case PolicyAckWindowStatus.Created:
                switch (next)
                {
                    case PolicyAckWindowStatus.Validated:
                        return true;
                }
                return false;
            case PolicyAckWindowStatus.Validated:
                return next == PolicyAckWindowStatus.Retired;
            case PolicyAckWindowStatus.Published:
                return next == PolicyAckWindowStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PolicyAckWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy ack window"} {Code}";
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

    public double WeightedLoad()
    {
        return Math.Round((double)WindowDays + Ceiling, 2);
    }

    public PolicyAckWindow Copy()
    {
        return new PolicyAckWindow(Code, Name, Tags, Country, Ceiling, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
