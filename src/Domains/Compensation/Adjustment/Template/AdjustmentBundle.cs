namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment bundle is in its life cycle.</summary>
public enum AdjustmentBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adjustment bundle and its current status.</summary>
public sealed class AdjustmentBundle(string code, string title, string summary, double minimum, int startsAt, AdjustmentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double Minimum { get; set; } = minimum;
    public int StartsAt { get; set; } = startsAt;
    public AdjustmentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment bundle in status idle.</summary>
    public static AdjustmentBundle NewAdjustmentBundle(string code, string title)
    {
        return new AdjustmentBundle(code, title, "", 0.0, 0, AdjustmentBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentBundleStatus next)
    {
        switch (Status)
        {
            case AdjustmentBundleStatus.Idle:
                return next == AdjustmentBundleStatus.Warming || next == AdjustmentBundleStatus.Stopped;
            case AdjustmentBundleStatus.Warming:
                return next == AdjustmentBundleStatus.Draining || next == AdjustmentBundleStatus.Stopped;
            case AdjustmentBundleStatus.Ready:
                return next == AdjustmentBundleStatus.Stopped;
            case AdjustmentBundleStatus.Draining:
                switch (next)
                {
                    case AdjustmentBundleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adjustment bundle"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Minimum + (double)StartsAt, 2);
    }

    public AdjustmentBundle Copy()
    {
        return new AdjustmentBundle(Code, Title, Summary, Minimum, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
