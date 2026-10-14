namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return metric is in its life cycle.</summary>
public enum AssetReturnMetricStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One asset return metric, with what the planners need to act on it.</summary>
public sealed class AssetReturnMetric(string code, string title, int durationMinutes, string reviewer, double cost, AssetReturnMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Reviewer { get; set; } = reviewer;
    public double Cost { get; set; } = cost;
    public AssetReturnMetricStatus Status { get; set; } = status;

    /// <summary>Starts a asset return metric in status planned.</summary>
    public static AssetReturnMetric NewAssetReturnMetric(string code, string title)
    {
        return new AssetReturnMetric(code, title, 0, "", 0.0, AssetReturnMetricStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnMetricStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnMetricStatus next)
    {
        switch (Status)
        {
            case AssetReturnMetricStatus.Planned:
                return next == AssetReturnMetricStatus.Cancelled;
            case AssetReturnMetricStatus.Scheduled:
                return next == AssetReturnMetricStatus.Completed;
            case AssetReturnMetricStatus.InProgress:
                return next == AssetReturnMetricStatus.Completed || next == AssetReturnMetricStatus.Cancelled;
            case AssetReturnMetricStatus.Completed:
                switch (next)
                {
                    case AssetReturnMetricStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)DurationMinutes * Cost, 2);
    }

    public AssetReturnMetric Copy()
    {
        return new AssetReturnMetric(Code, Title, DurationMinutes, Reviewer, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
