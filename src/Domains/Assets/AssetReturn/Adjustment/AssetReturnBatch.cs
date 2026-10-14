namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return batch is in its life cycle.</summary>
public enum AssetReturnBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a asset return batch and its current status.</summary>
public sealed class AssetReturnBatch(string code, string title, int leaveDays, string externalRef, int capacity, string name, int retries, string summary, AssetReturnBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string ExternalRef { get; set; } = externalRef;
    public int Capacity { get; set; } = capacity;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Summary { get; set; } = summary;
    public AssetReturnBatchStatus Status { get; set; } = status;

    /// <summary>Starts a asset return batch in status idle.</summary>
    public static AssetReturnBatch NewAssetReturnBatch(string code, string title)
    {
        return new AssetReturnBatch(code, title, 0, "", 0, "", 0, "", AssetReturnBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnBatchStatus next)
    {
        switch (Status)
        {
            case AssetReturnBatchStatus.Idle:
                switch (next)
                {
                    case AssetReturnBatchStatus.Stopped:
                        return true;
                }
                return false;
            case AssetReturnBatchStatus.Warming:
                switch (next)
                {
                    case AssetReturnBatchStatus.Ready:
                    case AssetReturnBatchStatus.Stopped:
                        return true;
                }
                return false;
            case AssetReturnBatchStatus.Ready:
                return next == AssetReturnBatchStatus.Draining || next == AssetReturnBatchStatus.Stopped;
            case AssetReturnBatchStatus.Draining:
                return next == AssetReturnBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AssetReturnBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"asset return batch"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Capacity * (double)Retries, 2);
    }

    public AssetReturnBatch Copy()
    {
        return new AssetReturnBatch(Code, Title, LeaveDays, ExternalRef, Capacity, Name, Retries, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
