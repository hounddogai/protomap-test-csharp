namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return threshold is in its life cycle.</summary>
public enum AssetReturnThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A asset return threshold. Its code is unique within a tenant.</summary>
public sealed class AssetReturnThreshold(string code, string label, string externalRef, List<string> tags, string skill, double hours, string source, int batchSize, double margin, int startsAt, AssetReturnThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public List<string> Tags { get; set; } = tags;
    public string Skill { get; set; } = skill;
    public double Hours { get; set; } = hours;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public double Margin { get; set; } = margin;
    public int StartsAt { get; set; } = startsAt;
    public AssetReturnThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a asset return threshold in status idle.</summary>
    public static AssetReturnThreshold NewAssetReturnThreshold(string code, string label)
    {
        return new AssetReturnThreshold(
            code,
            label,
            "",
            new List<string>(),
            "",
            0.0,
            "",
            0,
            0.0,
            0,
            AssetReturnThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnThresholdStatus next)
    {
        switch (Status)
        {
            case AssetReturnThresholdStatus.Idle:
                switch (next)
                {
                    case AssetReturnThresholdStatus.Ready:
                    case AssetReturnThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case AssetReturnThresholdStatus.Warming:
                switch (next)
                {
                    case AssetReturnThresholdStatus.Ready:
                        return true;
                }
                return false;
            case AssetReturnThresholdStatus.Ready:
                switch (next)
                {
                    case AssetReturnThresholdStatus.Draining:
                        return true;
                }
                return false;
            case AssetReturnThresholdStatus.Draining:
                switch (next)
                {
                    case AssetReturnThresholdStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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
        return Math.Round((double)BatchSize + Hours, 2);
    }

    public AssetReturnThreshold Copy()
    {
        return new AssetReturnThreshold(
            Code,
            Label,
            ExternalRef,
            Tags,
            Skill,
            Hours,
            Source,
            BatchSize,
            Margin,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
