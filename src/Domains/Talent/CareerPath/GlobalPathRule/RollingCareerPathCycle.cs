namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling career path cycle is in its life cycle.</summary>
public enum RollingCareerPathCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a rolling career path cycle and its current status.</summary>
public sealed class RollingCareerPathCycle(string code, string name, string externalRef, string title, double discount, string badge, double rate, int startsAt, string salaryBand, string label, RollingCareerPathCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string Label { get; set; } = label;
    public RollingCareerPathCycleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling career path cycle in status idle.</summary>
    public static RollingCareerPathCycle NewRollingCareerPathCycle(string code, string name)
    {
        return new RollingCareerPathCycle(
            code,
            name,
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            "",
            "",
            RollingCareerPathCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCareerPathCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCareerPathCycleStatus next)
    {
        switch (Status)
        {
            case RollingCareerPathCycleStatus.Idle:
                return next == RollingCareerPathCycleStatus.Ready;
            case RollingCareerPathCycleStatus.Warming:
                switch (next)
                {
                    case RollingCareerPathCycleStatus.Draining:
                    case RollingCareerPathCycleStatus.Stopped:
                        return true;
                }
                return false;
            case RollingCareerPathCycleStatus.Ready:
                return next == RollingCareerPathCycleStatus.Draining;
            case RollingCareerPathCycleStatus.Draining:
                return next == RollingCareerPathCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingCareerPathCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling career path cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)StartsAt * Discount, 2);
    }

    public RollingCareerPathCycle Copy()
    {
        return new RollingCareerPathCycle(
            Code,
            Name,
            ExternalRef,
            Title,
            Discount,
            Badge,
            Rate,
            StartsAt,
            SalaryBand,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
