namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount policy is in its life cycle.</summary>
public enum HeadcountPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A headcount policy. Its code is unique within a tenant.</summary>
public sealed class HeadcountPolicy(string code, string label, int expiresAt, string channel, double cost, HeadcountPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public double Cost { get; set; } = cost;
    public HeadcountPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a headcount policy in status new.</summary>
    public static HeadcountPolicy NewHeadcountPolicy(string code, string label)
    {
        return new HeadcountPolicy(code, label, 0, "", 0.0, HeadcountPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountPolicyStatus next)
    {
        switch (Status)
        {
            case HeadcountPolicyStatus.New:
                return next == HeadcountPolicyStatus.Failed;
            case HeadcountPolicyStatus.Queued:
                switch (next)
                {
                    case HeadcountPolicyStatus.Running:
                    case HeadcountPolicyStatus.Done:
                        return true;
                }
                return false;
            case HeadcountPolicyStatus.Running:
                return next == HeadcountPolicyStatus.Done || next == HeadcountPolicyStatus.Failed;
            case HeadcountPolicyStatus.Done:
                switch (next)
                {
                    case HeadcountPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round(Cost + (double)ExpiresAt, 2);
    }

    public HeadcountPolicy Copy()
    {
        return new HeadcountPolicy(Code, Label, ExpiresAt, Channel, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
