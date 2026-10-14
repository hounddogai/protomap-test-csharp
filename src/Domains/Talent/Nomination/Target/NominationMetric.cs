namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination metric is in its life cycle.</summary>
public enum NominationMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A nomination metric as the nomination team keeps it.</summary>
public sealed class NominationMetric(string code, string name, int batchSize, bool approved, int startsAt, double cost, int expiresAt, int sequence, int tenureMonths, bool archived, NominationMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Sequence { get; set; } = sequence;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Archived { get; set; } = archived;
    public NominationMetricStatus Status { get; set; } = status;

    /// <summary>Starts a nomination metric in status created.</summary>
    public static NominationMetric NewNominationMetric(string code, string name)
    {
        return new NominationMetric(code, name, 0, false, 0, 0.0, 0, 0, 0, false, NominationMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationMetricStatus next)
    {
        switch (Status)
        {
            case NominationMetricStatus.Created:
                return next == NominationMetricStatus.Retired;
            case NominationMetricStatus.Validated:
                switch (next)
                {
                    case NominationMetricStatus.Retired:
                        return true;
                }
                return false;
            case NominationMetricStatus.Published:
                switch (next)
                {
                    case NominationMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence - Cost, 2);
    }

    public NominationMetric Copy()
    {
        return new NominationMetric(
            Code,
            Name,
            BatchSize,
            Approved,
            StartsAt,
            Cost,
            ExpiresAt,
            Sequence,
            TenureMonths,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
