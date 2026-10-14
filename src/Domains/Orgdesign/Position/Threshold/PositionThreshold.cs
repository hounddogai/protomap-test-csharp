namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position threshold is in its life cycle.</summary>
public enum PositionThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A position threshold. Its code is unique within a tenant.</summary>
public sealed class PositionThreshold(string code, string label, string externalRef, double discount, string location, int limit, double rating, int attempts, double amount, PositionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Discount { get; set; } = discount;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public double Rating { get; set; } = rating;
    public int Attempts { get; set; } = attempts;
    public double Amount { get; set; } = amount;
    public PositionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a position threshold in status requested.</summary>
    public static PositionThreshold NewPositionThreshold(string code, string label)
    {
        return new PositionThreshold(code, label, "", 0.0, "", 0, 0.0, 0, 0.0, PositionThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionThresholdStatus next)
    {
        switch (Status)
        {
            case PositionThresholdStatus.Requested:
                switch (next)
                {
                    case PositionThresholdStatus.Expired:
                        return true;
                }
                return false;
            case PositionThresholdStatus.Granted:
                return next == PositionThresholdStatus.Revoked;
            case PositionThresholdStatus.Revoked:
                switch (next)
                {
                    case PositionThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"position threshold"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Attempts * Rating, 2);
    }

    public PositionThreshold Copy()
    {
        return new PositionThreshold(
            Code,
            Label,
            ExternalRef,
            Discount,
            Location,
            Limit,
            Rating,
            Attempts,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
