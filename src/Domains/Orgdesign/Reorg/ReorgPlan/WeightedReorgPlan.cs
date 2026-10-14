namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted reorg plan is in its life cycle.</summary>
public enum WeightedReorgPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A weighted reorg plan as the reorg team keeps it.</summary>
public sealed class WeightedReorgPlan(string code, string title, double ceiling, string segment, string externalRef, string notes, int startsAt, string salaryBand, WeightedReorgPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string Segment { get; set; } = segment;
    public string ExternalRef { get; set; } = externalRef;
    public string Notes { get; set; } = notes;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public WeightedReorgPlanStatus Status { get; set; } = status;

    /// <summary>Starts a weighted reorg plan in status requested.</summary>
    public static WeightedReorgPlan NewWeightedReorgPlan(string code, string title)
    {
        return new WeightedReorgPlan(code, title, 0.0, "", "", "", 0, "", WeightedReorgPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedReorgPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedReorgPlanStatus next)
    {
        switch (Status)
        {
            case WeightedReorgPlanStatus.Requested:
                switch (next)
                {
                    case WeightedReorgPlanStatus.Granted:
                        return true;
                }
                return false;
            case WeightedReorgPlanStatus.Granted:
                return next == WeightedReorgPlanStatus.Revoked || next == WeightedReorgPlanStatus.Expired;
            case WeightedReorgPlanStatus.Revoked:
                return next == WeightedReorgPlanStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WeightedReorgPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted reorg plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Ceiling + (double)StartsAt, 2);
    }

    public WeightedReorgPlan Copy()
    {
        return new WeightedReorgPlan(
            Code,
            Title,
            Ceiling,
            Segment,
            ExternalRef,
            Notes,
            StartsAt,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
