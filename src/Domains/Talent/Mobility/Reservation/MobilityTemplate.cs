namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility template is in its life cycle.</summary>
public enum MobilityTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A mobility template as the mobility team keeps it.</summary>
public sealed class MobilityTemplate(string code, string label, double cost, bool archived, string currency, int expiresAt, int createdAt, double weight, List<string> tags, MobilityTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public bool Archived { get; set; } = archived;
    public string Currency { get; set; } = currency;
    public int ExpiresAt { get; set; } = expiresAt;
    public int CreatedAt { get; set; } = createdAt;
    public double Weight { get; set; } = weight;
    public List<string> Tags { get; set; } = tags;
    public MobilityTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a mobility template in status proposed.</summary>
    public static MobilityTemplate NewMobilityTemplate(string code, string label)
    {
        return new MobilityTemplate(
            code,
            label,
            0.0,
            false,
            "",
            0,
            0,
            0.0,
            new List<string>(),
            MobilityTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityTemplateStatus next)
    {
        switch (Status)
        {
            case MobilityTemplateStatus.Proposed:
                return next == MobilityTemplateStatus.Approved || next == MobilityTemplateStatus.Withdrawn;
            case MobilityTemplateStatus.Approved:
                return next == MobilityTemplateStatus.Rejected;
            case MobilityTemplateStatus.Rejected:
                return next == MobilityTemplateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(MobilityTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedExposure()
    {
        return Math.Round(Weight - Cost, 2);
    }

    public MobilityTemplate Copy()
    {
        return new MobilityTemplate(
            Code,
            Label,
            Cost,
            Archived,
            Currency,
            ExpiresAt,
            CreatedAt,
            Weight,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
