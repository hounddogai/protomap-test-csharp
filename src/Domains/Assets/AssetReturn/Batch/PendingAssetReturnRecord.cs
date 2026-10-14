namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending asset return record is in its life cycle.</summary>
public enum PendingAssetReturnRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pending asset return record as the asset return team keeps it.</summary>
public sealed class PendingAssetReturnRecord(string code, string title, string costCenter, string externalRef, List<string> tags, bool active, double score, PendingAssetReturnRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public List<string> Tags { get; set; } = tags;
    public bool Active { get; set; } = active;
    public double Score { get; set; } = score;
    public PendingAssetReturnRecordStatus Status { get; set; } = status;

    /// <summary>Starts a pending asset return record in status idle.</summary>
    public static PendingAssetReturnRecord NewPendingAssetReturnRecord(string code, string title)
    {
        return new PendingAssetReturnRecord(
            code,
            title,
            "",
            "",
            new List<string>(),
            false,
            0.0,
            PendingAssetReturnRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAssetReturnRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAssetReturnRecordStatus next)
    {
        switch (Status)
        {
            case PendingAssetReturnRecordStatus.Idle:
                return next == PendingAssetReturnRecordStatus.Ready;
            case PendingAssetReturnRecordStatus.Warming:
                switch (next)
                {
                    case PendingAssetReturnRecordStatus.Stopped:
                        return true;
                }
                return false;
            case PendingAssetReturnRecordStatus.Ready:
                return next == PendingAssetReturnRecordStatus.Draining || next == PendingAssetReturnRecordStatus.Stopped;
            case PendingAssetReturnRecordStatus.Draining:
                return next == PendingAssetReturnRecordStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingAssetReturnRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending asset return record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double MeasuredValue()
    {
        return Score;
    }

    public PendingAssetReturnRecord Copy()
    {
        return new PendingAssetReturnRecord(Code, Title, CostCenter, ExternalRef, Tags, Active, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
