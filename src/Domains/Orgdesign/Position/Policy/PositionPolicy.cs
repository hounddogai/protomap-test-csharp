namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position policy is in its life cycle.</summary>
public enum PositionPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a position policy and its current status.</summary>
public sealed class PositionPolicy(string code, string label, string region, int createdAt, int priority, double ratio, PositionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public int CreatedAt { get; set; } = createdAt;
    public int Priority { get; set; } = priority;
    public double Ratio { get; set; } = ratio;
    public PositionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a position policy in status idle.</summary>
    public static PositionPolicy NewPositionPolicy(string code, string label)
    {
        return new PositionPolicy(code, label, "", 0, 0, 0.0, PositionPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionPolicyStatus next)
    {
        switch (Status)
        {
            case PositionPolicyStatus.Idle:
                switch (next)
                {
                    case PositionPolicyStatus.Ready:
                        return true;
                }
                return false;
            case PositionPolicyStatus.Warming:
                switch (next)
                {
                    case PositionPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case PositionPolicyStatus.Ready:
                switch (next)
                {
                    case PositionPolicyStatus.Draining:
                    case PositionPolicyStatus.Stopped:
                        return true;
                }
                return false;
            case PositionPolicyStatus.Draining:
                return next == PositionPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PositionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"position policy"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Ratio - (double)Priority, 2);
    }

    public PositionPolicy Copy()
    {
        return new PositionPolicy(Code, Label, Region, CreatedAt, Priority, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
