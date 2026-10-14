namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal policy ack override is in its life cycle.</summary>
public enum SeasonalPolicyAckOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal policy ack override. Its code is unique within a tenant.</summary>
public sealed class SeasonalPolicyAckOverride(string code, string name, int capacity, double rating, double amount, SeasonalPolicyAckOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public SeasonalPolicyAckOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal policy ack override in status open.</summary>
    public static SeasonalPolicyAckOverride NewSeasonalPolicyAckOverride(string code, string name)
    {
        return new SeasonalPolicyAckOverride(code, name, 0, 0.0, 0.0, SeasonalPolicyAckOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPolicyAckOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPolicyAckOverrideStatus next)
    {
        switch (Status)
        {
            case SeasonalPolicyAckOverrideStatus.Open:
                return next == SeasonalPolicyAckOverrideStatus.Reopened;
            case SeasonalPolicyAckOverrideStatus.InReview:
                switch (next)
                {
                    case SeasonalPolicyAckOverrideStatus.Resolved:
                        return true;
                }
                return false;
            case SeasonalPolicyAckOverrideStatus.Resolved:
                switch (next)
                {
                    case SeasonalPolicyAckOverrideStatus.Reopened:
                    case SeasonalPolicyAckOverrideStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalPolicyAckOverrideStatus.Reopened:
                switch (next)
                {
                    case SeasonalPolicyAckOverrideStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPolicyAckOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal policy ack override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity + Rating, 2);
    }

    public SeasonalPolicyAckOverride Copy()
    {
        return new SeasonalPolicyAckOverride(Code, Name, Capacity, Rating, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
