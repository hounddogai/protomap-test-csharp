namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive asset return allocation is in its life cycle.</summary>
public enum AdaptiveAssetReturnAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive asset return allocation, with what the planners need to act on it.</summary>
public sealed class AdaptiveAssetReturnAllocation(string code, string label, double rate, string jobCode, int priority, string description, string region, string checksum, string phone, int batchSize, AdaptiveAssetReturnAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string JobCode { get; set; } = jobCode;
    public int Priority { get; set; } = priority;
    public string Description { get; set; } = description;
    public string Region { get; set; } = region;
    public string Checksum { get; set; } = checksum;
    public string Phone { get; set; } = phone;
    public int BatchSize { get; set; } = batchSize;
    public AdaptiveAssetReturnAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive asset return allocation in status idle.</summary>
    public static AdaptiveAssetReturnAllocation NewAdaptiveAssetReturnAllocation(string code, string label)
    {
        return new AdaptiveAssetReturnAllocation(
            code,
            label,
            0.0,
            "",
            0,
            "",
            "",
            "",
            "",
            0,
            AdaptiveAssetReturnAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAssetReturnAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAssetReturnAllocationStatus next)
    {
        switch (Status)
        {
            case AdaptiveAssetReturnAllocationStatus.Idle:
                return next == AdaptiveAssetReturnAllocationStatus.Ready;
            case AdaptiveAssetReturnAllocationStatus.Warming:
                return next == AdaptiveAssetReturnAllocationStatus.Draining || next == AdaptiveAssetReturnAllocationStatus.Stopped;
            case AdaptiveAssetReturnAllocationStatus.Ready:
                switch (next)
                {
                    case AdaptiveAssetReturnAllocationStatus.Draining:
                    case AdaptiveAssetReturnAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveAssetReturnAllocationStatus.Draining:
                return next == AdaptiveAssetReturnAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveAssetReturnAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive asset return allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Rate * (double)BatchSize, 2);
    }

    public AdaptiveAssetReturnAllocation Copy()
    {
        return new AdaptiveAssetReturnAllocation(
            Code,
            Label,
            Rate,
            JobCode,
            Priority,
            Description,
            Region,
            Checksum,
            Phone,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
