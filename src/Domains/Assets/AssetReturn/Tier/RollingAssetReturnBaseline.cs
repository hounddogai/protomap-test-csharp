namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling asset return baseline is in its life cycle.</summary>
public enum RollingAssetReturnBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a rolling asset return baseline and its current status.</summary>
public sealed class RollingAssetReturnBaseline(string code, string label, bool archived, List<string> tags, double floorValue, int quantity, string timezone, double hours, double score, string name, RollingAssetReturnBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public List<string> Tags { get; set; } = tags;
    public double FloorValue { get; set; } = floorValue;
    public int Quantity { get; set; } = quantity;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public double Score { get; set; } = score;
    public string Name { get; set; } = name;
    public RollingAssetReturnBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a rolling asset return baseline in status idle.</summary>
    public static RollingAssetReturnBaseline NewRollingAssetReturnBaseline(string code, string label)
    {
        return new RollingAssetReturnBaseline(
            code,
            label,
            false,
            new List<string>(),
            0.0,
            0,
            "",
            0.0,
            0.0,
            "",
            RollingAssetReturnBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAssetReturnBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAssetReturnBaselineStatus next)
    {
        switch (Status)
        {
            case RollingAssetReturnBaselineStatus.Idle:
                switch (next)
                {
                    case RollingAssetReturnBaselineStatus.Ready:
                    case RollingAssetReturnBaselineStatus.Stopped:
                        return true;
                }
                return false;
            case RollingAssetReturnBaselineStatus.Warming:
                return next == RollingAssetReturnBaselineStatus.Ready;
            case RollingAssetReturnBaselineStatus.Ready:
                switch (next)
                {
                    case RollingAssetReturnBaselineStatus.Draining:
                        return true;
                }
                return false;
            case RollingAssetReturnBaselineStatus.Draining:
                return next == RollingAssetReturnBaselineStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingAssetReturnBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling asset return baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling asset return baseline"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round(Score + Hours, 2);
    }

    public RollingAssetReturnBaseline Copy()
    {
        return new RollingAssetReturnBaseline(
            Code,
            Label,
            Archived,
            Tags,
            FloorValue,
            Quantity,
            Timezone,
            Hours,
            Score,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
