namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global access badge estimate is in its life cycle.</summary>
public enum GlobalAccessBadgeEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global access badge estimate and its current status.</summary>
public sealed class GlobalAccessBadgeEstimate(string code, string name, double maximum, int endsAt, int capacity, GlobalAccessBadgeEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public GlobalAccessBadgeEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a global access badge estimate in status idle.</summary>
    public static GlobalAccessBadgeEstimate NewGlobalAccessBadgeEstimate(string code, string name)
    {
        return new GlobalAccessBadgeEstimate(code, name, 0.0, 0, 0, GlobalAccessBadgeEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAccessBadgeEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAccessBadgeEstimateStatus next)
    {
        switch (Status)
        {
            case GlobalAccessBadgeEstimateStatus.Idle:
                switch (next)
                {
                    case GlobalAccessBadgeEstimateStatus.Ready:
                        return true;
                }
                return false;
            case GlobalAccessBadgeEstimateStatus.Warming:
                switch (next)
                {
                    case GlobalAccessBadgeEstimateStatus.Ready:
                        return true;
                }
                return false;
            case GlobalAccessBadgeEstimateStatus.Ready:
                return next == GlobalAccessBadgeEstimateStatus.Stopped;
            case GlobalAccessBadgeEstimateStatus.Draining:
                switch (next)
                {
                    case GlobalAccessBadgeEstimateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAccessBadgeEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global access badge estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)EndsAt - (double)Capacity, 2);
    }

    public GlobalAccessBadgeEstimate Copy()
    {
        return new GlobalAccessBadgeEstimate(Code, Name, Maximum, EndsAt, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
