namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return estimate is in its life cycle.</summary>
public enum AssetReturnEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One asset return estimate, with what the planners need to act on it.</summary>
public sealed class AssetReturnEstimate(string code, string title, double budget, double fte, string badge, bool visible, string channel, string externalRef, string description, AssetReturnEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Fte { get; set; } = fte;
    public string Badge { get; set; } = badge;
    public bool Visible { get; set; } = visible;
    public string Channel { get; set; } = channel;
    public string ExternalRef { get; set; } = externalRef;
    public string Description { get; set; } = description;
    public AssetReturnEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a asset return estimate in status idle.</summary>
    public static AssetReturnEstimate NewAssetReturnEstimate(string code, string title)
    {
        return new AssetReturnEstimate(code, title, 0.0, 0.0, "", false, "", "", "", AssetReturnEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnEstimateStatus next)
    {
        switch (Status)
        {
            case AssetReturnEstimateStatus.Idle:
                return next == AssetReturnEstimateStatus.Warming || next == AssetReturnEstimateStatus.Draining;
            case AssetReturnEstimateStatus.Warming:
                return next == AssetReturnEstimateStatus.Ready;
            case AssetReturnEstimateStatus.Ready:
                switch (next)
                {
                    case AssetReturnEstimateStatus.Draining:
                    case AssetReturnEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case AssetReturnEstimateStatus.Draining:
                switch (next)
                {
                    case AssetReturnEstimateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"asset return estimate"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Budget + Fte, 2);
    }

    public AssetReturnEstimate Copy()
    {
        return new AssetReturnEstimate(
            Code,
            Title,
            Budget,
            Fte,
            Badge,
            Visible,
            Channel,
            ExternalRef,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
