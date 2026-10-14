namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy score is in its life cycle.</summary>
public enum PolicyScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A policy score as the policy team keeps it.</summary>
public sealed class PolicyScore(string code, string name, double hours, string description, string channel, double ceiling, int graceDays, PolicyScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public PolicyScoreStatus Status { get; set; } = status;

    /// <summary>Starts a policy score in status idle.</summary>
    public static PolicyScore NewPolicyScore(string code, string name)
    {
        return new PolicyScore(code, name, 0.0, "", "", 0.0, 0, PolicyScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyScoreStatus next)
    {
        switch (Status)
        {
            case PolicyScoreStatus.Idle:
                switch (next)
                {
                    case PolicyScoreStatus.Ready:
                    case PolicyScoreStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyScoreStatus.Warming:
                switch (next)
                {
                    case PolicyScoreStatus.Ready:
                    case PolicyScoreStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyScoreStatus.Ready:
                switch (next)
                {
                    case PolicyScoreStatus.Draining:
                    case PolicyScoreStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyScoreStatus.Draining:
                switch (next)
                {
                    case PolicyScoreStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)GraceDays + Hours, 2);
    }

    public PolicyScore Copy()
    {
        return new PolicyScore(Code, Name, Hours, Description, Channel, Ceiling, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
