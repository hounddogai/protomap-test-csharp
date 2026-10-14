namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling cost model variance is in its life cycle.</summary>
public enum RollingCostModelVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a rolling cost model variance and its current status.</summary>
public sealed class RollingCostModelVariance(string code, string name, double threshold, double ratio, string skill, string region, int graceDays, bool pinned, double rate, double ceiling, RollingCostModelVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public double Ratio { get; set; } = ratio;
    public string Skill { get; set; } = skill;
    public string Region { get; set; } = region;
    public int GraceDays { get; set; } = graceDays;
    public bool Pinned { get; set; } = pinned;
    public double Rate { get; set; } = rate;
    public double Ceiling { get; set; } = ceiling;
    public RollingCostModelVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling cost model variance in status proposed.</summary>
    public static RollingCostModelVariance NewRollingCostModelVariance(string code, string name)
    {
        return new RollingCostModelVariance(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            0,
            false,
            0.0,
            0.0,
            RollingCostModelVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCostModelVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCostModelVarianceStatus next)
    {
        switch (Status)
        {
            case RollingCostModelVarianceStatus.Proposed:
                return next == RollingCostModelVarianceStatus.Approved || next == RollingCostModelVarianceStatus.Rejected;
            case RollingCostModelVarianceStatus.Approved:
                return next == RollingCostModelVarianceStatus.Rejected || next == RollingCostModelVarianceStatus.Withdrawn;
            case RollingCostModelVarianceStatus.Rejected:
                switch (next)
                {
                    case RollingCostModelVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingCostModelVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling cost model variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)GraceDays - Ratio, 2);
    }

    public RollingCostModelVariance Copy()
    {
        return new RollingCostModelVariance(
            Code,
            Name,
            Threshold,
            Ratio,
            Skill,
            Region,
            GraceDays,
            Pinned,
            Rate,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
