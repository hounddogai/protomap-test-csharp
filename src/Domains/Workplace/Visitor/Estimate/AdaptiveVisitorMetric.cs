namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive visitor metric is in its life cycle.</summary>
public enum AdaptiveVisitorMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive visitor metric and its current status.</summary>
public sealed class AdaptiveVisitorMetric(string code, string title, double fte, string label, string ownerId, string notes, bool archived, int version, AdaptiveVisitorMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Notes { get; set; } = notes;
    public bool Archived { get; set; } = archived;
    public int Version { get; set; } = version;
    public AdaptiveVisitorMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive visitor metric in status requested.</summary>
    public static AdaptiveVisitorMetric NewAdaptiveVisitorMetric(string code, string title)
    {
        return new AdaptiveVisitorMetric(
            code,
            title,
            0.0,
            "",
            "",
            "",
            false,
            0,
            AdaptiveVisitorMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveVisitorMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveVisitorMetricStatus next)
    {
        switch (Status)
        {
            case AdaptiveVisitorMetricStatus.Requested:
                return next == AdaptiveVisitorMetricStatus.Revoked || next == AdaptiveVisitorMetricStatus.Expired;
            case AdaptiveVisitorMetricStatus.Granted:
                switch (next)
                {
                    case AdaptiveVisitorMetricStatus.Revoked:
                        return true;
                }
                return false;
            case AdaptiveVisitorMetricStatus.Revoked:
                return next == AdaptiveVisitorMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveVisitorMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive visitor metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Fte + (double)Version, 2);
    }

    public AdaptiveVisitorMetric Copy()
    {
        return new AdaptiveVisitorMetric(Code, Title, Fte, Label, OwnerId, Notes, Archived, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
