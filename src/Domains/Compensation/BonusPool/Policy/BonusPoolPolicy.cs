namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool policy is in its life cycle.</summary>
public enum BonusPoolPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A bonus pool policy. Its code is unique within a tenant.</summary>
public sealed class BonusPoolPolicy(string code, string name, double ratio, int leaveDays, string timezone, string reviewer, string description, int quantity, BonusPoolPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public int LeaveDays { get; set; } = leaveDays;
    public string Timezone { get; set; } = timezone;
    public string Reviewer { get; set; } = reviewer;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public BonusPoolPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool policy in status idle.</summary>
    public static BonusPoolPolicy NewBonusPoolPolicy(string code, string name)
    {
        return new BonusPoolPolicy(code, name, 0.0, 0, "", "", "", 0, BonusPoolPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolPolicyStatus next)
    {
        switch (Status)
        {
            case BonusPoolPolicyStatus.Idle:
                return next == BonusPoolPolicyStatus.Draining;
            case BonusPoolPolicyStatus.Warming:
                return next == BonusPoolPolicyStatus.Draining || next == BonusPoolPolicyStatus.Stopped;
            case BonusPoolPolicyStatus.Ready:
                return next == BonusPoolPolicyStatus.Draining || next == BonusPoolPolicyStatus.Stopped;
            case BonusPoolPolicyStatus.Draining:
                switch (next)
                {
                    case BonusPoolPolicyStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)LeaveDays * Ratio, 2);
    }

    public BonusPoolPolicy Copy()
    {
        return new BonusPoolPolicy(Code, Name, Ratio, LeaveDays, Timezone, Reviewer, Description, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
