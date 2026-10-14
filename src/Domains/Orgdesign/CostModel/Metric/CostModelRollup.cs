namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model rollup is in its life cycle.</summary>
public enum CostModelRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A cost model rollup as the cost model team keeps it.</summary>
public sealed class CostModelRollup(string code, string label, string channel, string unit, int startsAt, string contactEmail, string ownerId, double maximum, CostModelRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Unit { get; set; } = unit;
    public int StartsAt { get; set; } = startsAt;
    public string ContactEmail { get; set; } = contactEmail;
    public string OwnerId { get; set; } = ownerId;
    public double Maximum { get; set; } = maximum;
    public CostModelRollupStatus Status { get; set; } = status;

    /// <summary>Starts a cost model rollup in status planned.</summary>
    public static CostModelRollup NewCostModelRollup(string code, string label)
    {
        return new CostModelRollup(code, label, "", "", 0, "", "", 0.0, CostModelRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelRollupStatus next)
    {
        switch (Status)
        {
            case CostModelRollupStatus.Planned:
                switch (next)
                {
                    case CostModelRollupStatus.InProgress:
                    case CostModelRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case CostModelRollupStatus.Scheduled:
                switch (next)
                {
                    case CostModelRollupStatus.InProgress:
                    case CostModelRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case CostModelRollupStatus.InProgress:
                return next == CostModelRollupStatus.Completed || next == CostModelRollupStatus.Cancelled;
            case CostModelRollupStatus.Completed:
                switch (next)
                {
                    case CostModelRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Maximum + (double)StartsAt, 2);
    }

    public CostModelRollup Copy()
    {
        return new CostModelRollup(Code, Label, Channel, Unit, StartsAt, ContactEmail, OwnerId, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
