namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive laptop checkpoint is in its life cycle.</summary>
public enum AdaptiveLaptopCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a adaptive laptop checkpoint and its current status.</summary>
public sealed class AdaptiveLaptopCheckpoint(string code, string label, double budget, int retries, string department, double cost, double ceiling, Dictionary<string, string> attributes, string managerId, string ownerId, double ratio, AdaptiveLaptopCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public string Department { get; set; } = department;
    public double Cost { get; set; } = cost;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ManagerId { get; set; } = managerId;
    public string OwnerId { get; set; } = ownerId;
    public double Ratio { get; set; } = ratio;
    public AdaptiveLaptopCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive laptop checkpoint in status created.</summary>
    public static AdaptiveLaptopCheckpoint NewAdaptiveLaptopCheckpoint(string code, string label)
    {
        return new AdaptiveLaptopCheckpoint(
            code,
            label,
            0.0,
            0,
            "",
            0.0,
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            AdaptiveLaptopCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveLaptopCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveLaptopCheckpointStatus next)
    {
        switch (Status)
        {
            case AdaptiveLaptopCheckpointStatus.Created:
                return next == AdaptiveLaptopCheckpointStatus.Validated;
            case AdaptiveLaptopCheckpointStatus.Validated:
                switch (next)
                {
                    case AdaptiveLaptopCheckpointStatus.Published:
                    case AdaptiveLaptopCheckpointStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveLaptopCheckpointStatus.Published:
                return next == AdaptiveLaptopCheckpointStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveLaptopCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive laptop checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)Retries * Ratio, 2);
    }

    public AdaptiveLaptopCheckpoint Copy()
    {
        return new AdaptiveLaptopCheckpoint(
            Code,
            Label,
            Budget,
            Retries,
            Department,
            Cost,
            Ceiling,
            Attributes,
            ManagerId,
            OwnerId,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
