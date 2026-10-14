namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return digest is in its life cycle.</summary>
public enum AssetReturnDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One asset return digest, with what the planners need to act on it.</summary>
public sealed class AssetReturnDigest(string code, string label, string notes, int graceDays, int priority, double maximum, AssetReturnDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public int Priority { get; set; } = priority;
    public double Maximum { get; set; } = maximum;
    public AssetReturnDigestStatus Status { get; set; } = status;

    /// <summary>Starts a asset return digest in status planned.</summary>
    public static AssetReturnDigest NewAssetReturnDigest(string code, string label)
    {
        return new AssetReturnDigest(code, label, "", 0, 0, 0.0, AssetReturnDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnDigestStatus next)
    {
        switch (Status)
        {
            case AssetReturnDigestStatus.Planned:
                return next == AssetReturnDigestStatus.Completed;
            case AssetReturnDigestStatus.Scheduled:
                return next == AssetReturnDigestStatus.InProgress || next == AssetReturnDigestStatus.Completed;
            case AssetReturnDigestStatus.InProgress:
                return next == AssetReturnDigestStatus.Completed || next == AssetReturnDigestStatus.Cancelled;
            case AssetReturnDigestStatus.Completed:
                switch (next)
                {
                    case AssetReturnDigestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Maximum * (double)GraceDays, 2);
    }

    public AssetReturnDigest Copy()
    {
        return new AssetReturnDigest(Code, Label, Notes, GraceDays, Priority, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
