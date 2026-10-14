namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled equity grant variance is in its life cycle.</summary>
public enum ScheduledEquityGrantVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled equity grant variance as the equity grant team keeps it.</summary>
public sealed class ScheduledEquityGrantVariance(string code, string name, string notes, string region, bool visible, double discount, int batchSize, ScheduledEquityGrantVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string Region { get; set; } = region;
    public bool Visible { get; set; } = visible;
    public double Discount { get; set; } = discount;
    public int BatchSize { get; set; } = batchSize;
    public ScheduledEquityGrantVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled equity grant variance in status requested.</summary>
    public static ScheduledEquityGrantVariance NewScheduledEquityGrantVariance(string code, string name)
    {
        return new ScheduledEquityGrantVariance(
            code,
            name,
            "",
            "",
            false,
            0.0,
            0,
            ScheduledEquityGrantVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEquityGrantVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEquityGrantVarianceStatus next)
    {
        switch (Status)
        {
            case ScheduledEquityGrantVarianceStatus.Requested:
                return next == ScheduledEquityGrantVarianceStatus.Revoked;
            case ScheduledEquityGrantVarianceStatus.Granted:
                switch (next)
                {
                    case ScheduledEquityGrantVarianceStatus.Revoked:
                        return true;
                }
                return false;
            case ScheduledEquityGrantVarianceStatus.Revoked:
                switch (next)
                {
                    case ScheduledEquityGrantVarianceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledEquityGrantVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled equity grant variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Discount - (double)BatchSize, 2);
    }

    public ScheduledEquityGrantVariance Copy()
    {
        return new ScheduledEquityGrantVariance(Code, Name, Notes, Region, Visible, Discount, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
