namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered policy ack override is in its life cycle.</summary>
public enum TieredPolicyAckOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tiered policy ack override and its current status.</summary>
public sealed class TieredPolicyAckOverride(string code, string label, double hours, int durationMinutes, string costCenter, int attempts, string phone, string source, TieredPolicyAckOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public string Source { get; set; } = source;
    public TieredPolicyAckOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a tiered policy ack override in status created.</summary>
    public static TieredPolicyAckOverride NewTieredPolicyAckOverride(string code, string label)
    {
        return new TieredPolicyAckOverride(
            code,
            label,
            0.0,
            0,
            "",
            0,
            "",
            "",
            TieredPolicyAckOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPolicyAckOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPolicyAckOverrideStatus next)
    {
        switch (Status)
        {
            case TieredPolicyAckOverrideStatus.Created:
                switch (next)
                {
                    case TieredPolicyAckOverrideStatus.Published:
                        return true;
                }
                return false;
            case TieredPolicyAckOverrideStatus.Validated:
                switch (next)
                {
                    case TieredPolicyAckOverrideStatus.Published:
                    case TieredPolicyAckOverrideStatus.Retired:
                        return true;
                }
                return false;
            case TieredPolicyAckOverrideStatus.Published:
                return next == TieredPolicyAckOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredPolicyAckOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered policy ack override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"tiered policy ack override"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Hours + (double)DurationMinutes, 2);
    }

    public TieredPolicyAckOverride Copy()
    {
        return new TieredPolicyAckOverride(
            Code,
            Label,
            Hours,
            DurationMinutes,
            CostCenter,
            Attempts,
            Phone,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
