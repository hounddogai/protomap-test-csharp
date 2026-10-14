namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy span adjustment is in its life cycle.</summary>
public enum LegacySpanAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a legacy span adjustment and its current status.</summary>
public sealed class LegacySpanAdjustment(string code, string name, int endsAt, double weight, int sequence, double discount, string currency, string costCenter, string ownerId, LegacySpanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public double Discount { get; set; } = discount;
    public string Currency { get; set; } = currency;
    public string CostCenter { get; set; } = costCenter;
    public string OwnerId { get; set; } = ownerId;
    public LegacySpanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy span adjustment in status open.</summary>
    public static LegacySpanAdjustment NewLegacySpanAdjustment(string code, string name)
    {
        return new LegacySpanAdjustment(code, name, 0, 0.0, 0, 0.0, "", "", "", LegacySpanAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySpanAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySpanAdjustmentStatus next)
    {
        switch (Status)
        {
            case LegacySpanAdjustmentStatus.Open:
                switch (next)
                {
                    case LegacySpanAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case LegacySpanAdjustmentStatus.InReview:
                switch (next)
                {
                    case LegacySpanAdjustmentStatus.Resolved:
                        return true;
                }
                return false;
            case LegacySpanAdjustmentStatus.Resolved:
                return next == LegacySpanAdjustmentStatus.Reopened;
            case LegacySpanAdjustmentStatus.Reopened:
                return next == LegacySpanAdjustmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacySpanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy span adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Weight + (double)EndsAt, 2);
    }

    public LegacySpanAdjustment Copy()
    {
        return new LegacySpanAdjustment(
            Code,
            Name,
            EndsAt,
            Weight,
            Sequence,
            Discount,
            Currency,
            CostCenter,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
