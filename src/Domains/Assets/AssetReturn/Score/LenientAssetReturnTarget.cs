namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient asset return target is in its life cycle.</summary>
public enum LenientAssetReturnTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient asset return target as the asset return team keeps it.</summary>
public sealed class LenientAssetReturnTarget(string code, string name, int durationMinutes, string reviewer, string channel, LenientAssetReturnTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Reviewer { get; set; } = reviewer;
    public string Channel { get; set; } = channel;
    public LenientAssetReturnTargetStatus Status { get; set; } = status;

    /// <summary>Starts a lenient asset return target in status requested.</summary>
    public static LenientAssetReturnTarget NewLenientAssetReturnTarget(string code, string name)
    {
        return new LenientAssetReturnTarget(code, name, 0, "", "", LenientAssetReturnTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAssetReturnTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAssetReturnTargetStatus next)
    {
        switch (Status)
        {
            case LenientAssetReturnTargetStatus.Requested:
                switch (next)
                {
                    case LenientAssetReturnTargetStatus.Granted:
                    case LenientAssetReturnTargetStatus.Revoked:
                        return true;
                }
                return false;
            case LenientAssetReturnTargetStatus.Granted:
                switch (next)
                {
                    case LenientAssetReturnTargetStatus.Revoked:
                        return true;
                }
                return false;
            case LenientAssetReturnTargetStatus.Revoked:
                switch (next)
                {
                    case LenientAssetReturnTargetStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientAssetReturnTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient asset return target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public LenientAssetReturnTarget Copy()
    {
        return new LenientAssetReturnTarget(Code, Name, DurationMinutes, Reviewer, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
