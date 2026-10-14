namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive warranty mapping is in its life cycle.</summary>
public enum AdaptiveWarrantyMappingStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One adaptive warranty mapping, with what the planners need to act on it.</summary>
public sealed class AdaptiveWarrantyMapping(string code, string label, int quantity, double ratio, int durationMinutes, int expiresAt, int endsAt, int leaveDays, double score, AdaptiveWarrantyMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int ExpiresAt { get; set; } = expiresAt;
    public int EndsAt { get; set; } = endsAt;
    public int LeaveDays { get; set; } = leaveDays;
    public double Score { get; set; } = score;
    public AdaptiveWarrantyMappingStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive warranty mapping in status draft.</summary>
    public static AdaptiveWarrantyMapping NewAdaptiveWarrantyMapping(string code, string label)
    {
        return new AdaptiveWarrantyMapping(
            code,
            label,
            0,
            0.0,
            0,
            0,
            0,
            0,
            0.0,
            AdaptiveWarrantyMappingStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveWarrantyMappingStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveWarrantyMappingStatus next)
    {
        switch (Status)
        {
            case AdaptiveWarrantyMappingStatus.Draft:
                return next == AdaptiveWarrantyMappingStatus.Pending;
            case AdaptiveWarrantyMappingStatus.Pending:
                switch (next)
                {
                    case AdaptiveWarrantyMappingStatus.Suspended:
                        return true;
                }
                return false;
            case AdaptiveWarrantyMappingStatus.Active:
                return next == AdaptiveWarrantyMappingStatus.Suspended;
            case AdaptiveWarrantyMappingStatus.Suspended:
                switch (next)
                {
                    case AdaptiveWarrantyMappingStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveWarrantyMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive warranty mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"adaptive warranty mapping"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)DurationMinutes + (double)EndsAt, 2);
    }

    public AdaptiveWarrantyMapping Copy()
    {
        return new AdaptiveWarrantyMapping(
            Code,
            Label,
            Quantity,
            Ratio,
            DurationMinutes,
            ExpiresAt,
            EndsAt,
            LeaveDays,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
