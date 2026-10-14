namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pay band estimate is in its life cycle.</summary>
public enum TieredPayBandEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A tiered pay band estimate. Its code is unique within a tenant.</summary>
public sealed class TieredPayBandEstimate(string code, string label, int capacity, int expiresAt, int graceDays, int quantity, double floorValue, Dictionary<string, string> attributes, double margin, string skill, double ratio, TieredPayBandEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public int ExpiresAt { get; set; } = expiresAt;
    public int GraceDays { get; set; } = graceDays;
    public int Quantity { get; set; } = quantity;
    public double FloorValue { get; set; } = floorValue;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Margin { get; set; } = margin;
    public string Skill { get; set; } = skill;
    public double Ratio { get; set; } = ratio;
    public TieredPayBandEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pay band estimate in status idle.</summary>
    public static TieredPayBandEstimate NewTieredPayBandEstimate(string code, string label)
    {
        return new TieredPayBandEstimate(
            code,
            label,
            0,
            0,
            0,
            0,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            0.0,
            TieredPayBandEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPayBandEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPayBandEstimateStatus next)
    {
        switch (Status)
        {
            case TieredPayBandEstimateStatus.Idle:
                return next == TieredPayBandEstimateStatus.Ready || next == TieredPayBandEstimateStatus.Draining;
            case TieredPayBandEstimateStatus.Warming:
                switch (next)
                {
                    case TieredPayBandEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case TieredPayBandEstimateStatus.Ready:
                switch (next)
                {
                    case TieredPayBandEstimateStatus.Draining:
                    case TieredPayBandEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case TieredPayBandEstimateStatus.Draining:
                return next == TieredPayBandEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TieredPayBandEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pay band estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetLoad()
    {
        return Math.Round((double)ExpiresAt + Ratio, 2);
    }

    public TieredPayBandEstimate Copy()
    {
        return new TieredPayBandEstimate(
            Code,
            Label,
            Capacity,
            ExpiresAt,
            GraceDays,
            Quantity,
            FloorValue,
            Attributes,
            Margin,
            Skill,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
