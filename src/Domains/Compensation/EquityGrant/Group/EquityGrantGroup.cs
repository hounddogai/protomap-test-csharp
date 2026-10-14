namespace Acme.Workforce.Compensation.EquityGrant.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant group is in its life cycle.</summary>
public enum EquityGrantGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A equity grant group. Its code is unique within a tenant.</summary>
public sealed class EquityGrantGroup(string code, string label, double weight, string channel, int sequence, string externalRef, double floorValue, int updatedAt, int durationMinutes, int tenureMonths, int revision, EquityGrantGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string Channel { get; set; } = channel;
    public int Sequence { get; set; } = sequence;
    public string ExternalRef { get; set; } = externalRef;
    public double FloorValue { get; set; } = floorValue;
    public int UpdatedAt { get; set; } = updatedAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Revision { get; set; } = revision;
    public EquityGrantGroupStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant group in status new.</summary>
    public static EquityGrantGroup NewEquityGrantGroup(string code, string label)
    {
        return new EquityGrantGroup(code, label, 0.0, "", 0, "", 0.0, 0, 0, 0, 0, EquityGrantGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantGroupStatus next)
    {
        switch (Status)
        {
            case EquityGrantGroupStatus.New:
                switch (next)
                {
                    case EquityGrantGroupStatus.Running:
                    case EquityGrantGroupStatus.Failed:
                        return true;
                }
                return false;
            case EquityGrantGroupStatus.Queued:
                switch (next)
                {
                    case EquityGrantGroupStatus.Running:
                    case EquityGrantGroupStatus.Done:
                        return true;
                }
                return false;
            case EquityGrantGroupStatus.Running:
                switch (next)
                {
                    case EquityGrantGroupStatus.Done:
                    case EquityGrantGroupStatus.Failed:
                        return true;
                }
                return false;
            case EquityGrantGroupStatus.Done:
                return next == EquityGrantGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(EquityGrantGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Sequence - (double)DurationMinutes, 2);
    }

    public EquityGrantGroup Copy()
    {
        return new EquityGrantGroup(
            Code,
            Label,
            Weight,
            Channel,
            Sequence,
            ExternalRef,
            FloorValue,
            UpdatedAt,
            DurationMinutes,
            TenureMonths,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
