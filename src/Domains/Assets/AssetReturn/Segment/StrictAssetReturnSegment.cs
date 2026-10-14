namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict asset return segment is in its life cycle.</summary>
public enum StrictAssetReturnSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict asset return segment. Its code is unique within a tenant.</summary>
public sealed class StrictAssetReturnSegment(string code, string label, bool archived, string source, int retries, string phone, int leaveDays, string badge, string locale, int quantity, int priority, StrictAssetReturnSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public int Retries { get; set; } = retries;
    public string Phone { get; set; } = phone;
    public int LeaveDays { get; set; } = leaveDays;
    public string Badge { get; set; } = badge;
    public string Locale { get; set; } = locale;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public StrictAssetReturnSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict asset return segment in status proposed.</summary>
    public static StrictAssetReturnSegment NewStrictAssetReturnSegment(string code, string label)
    {
        return new StrictAssetReturnSegment(
            code,
            label,
            false,
            "",
            0,
            "",
            0,
            "",
            "",
            0,
            0,
            StrictAssetReturnSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssetReturnSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssetReturnSegmentStatus next)
    {
        switch (Status)
        {
            case StrictAssetReturnSegmentStatus.Proposed:
                return next == StrictAssetReturnSegmentStatus.Rejected || next == StrictAssetReturnSegmentStatus.Withdrawn;
            case StrictAssetReturnSegmentStatus.Approved:
                switch (next)
                {
                    case StrictAssetReturnSegmentStatus.Rejected:
                    case StrictAssetReturnSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictAssetReturnSegmentStatus.Rejected:
                switch (next)
                {
                    case StrictAssetReturnSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAssetReturnSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict asset return segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Quantity + (double)LeaveDays, 2);
    }

    public StrictAssetReturnSegment Copy()
    {
        return new StrictAssetReturnSegment(
            Code,
            Label,
            Archived,
            Source,
            Retries,
            Phone,
            LeaveDays,
            Badge,
            Locale,
            Quantity,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
