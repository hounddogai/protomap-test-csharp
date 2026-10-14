namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional position policy is in its life cycle.</summary>
public enum RegionalPositionPolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One regional position policy, with what the planners need to act on it.</summary>
public sealed class RegionalPositionPolicy(string code, string label, int batchSize, int createdAt, double fte, double discount, double rating, string checksum, RegionalPositionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int CreatedAt { get; set; } = createdAt;
    public double Fte { get; set; } = fte;
    public double Discount { get; set; } = discount;
    public double Rating { get; set; } = rating;
    public string Checksum { get; set; } = checksum;
    public RegionalPositionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a regional position policy in status proposed.</summary>
    public static RegionalPositionPolicy NewRegionalPositionPolicy(string code, string label)
    {
        return new RegionalPositionPolicy(
            code,
            label,
            0,
            0,
            0.0,
            0.0,
            0.0,
            "",
            RegionalPositionPolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPositionPolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPositionPolicyStatus next)
    {
        switch (Status)
        {
            case RegionalPositionPolicyStatus.Proposed:
                switch (next)
                {
                    case RegionalPositionPolicyStatus.Approved:
                        return true;
                }
                return false;
            case RegionalPositionPolicyStatus.Approved:
                return next == RegionalPositionPolicyStatus.Rejected || next == RegionalPositionPolicyStatus.Withdrawn;
            case RegionalPositionPolicyStatus.Rejected:
                return next == RegionalPositionPolicyStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalPositionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional position policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Rating * Fte, 2);
    }

    public RegionalPositionPolicy Copy()
    {
        return new RegionalPositionPolicy(
            Code,
            Label,
            BatchSize,
            CreatedAt,
            Fte,
            Discount,
            Rating,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
