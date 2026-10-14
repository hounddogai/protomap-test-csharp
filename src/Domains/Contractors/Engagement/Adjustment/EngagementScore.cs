namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement score is in its life cycle.</summary>
public enum EngagementScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a engagement score and its current status.</summary>
public sealed class EngagementScore(string code, string label, double minimum, double maximum, double threshold, string location, double weight, string badge, double ratio, EngagementScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double Maximum { get; set; } = maximum;
    public double Threshold { get; set; } = threshold;
    public string Location { get; set; } = location;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public double Ratio { get; set; } = ratio;
    public EngagementScoreStatus Status { get; set; } = status;

    /// <summary>Starts a engagement score in status proposed.</summary>
    public static EngagementScore NewEngagementScore(string code, string label)
    {
        return new EngagementScore(code, label, 0.0, 0.0, 0.0, "", 0.0, "", 0.0, EngagementScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementScoreStatus next)
    {
        switch (Status)
        {
            case EngagementScoreStatus.Proposed:
                return next == EngagementScoreStatus.Approved;
            case EngagementScoreStatus.Approved:
                return next == EngagementScoreStatus.Rejected || next == EngagementScoreStatus.Withdrawn;
            case EngagementScoreStatus.Rejected:
                switch (next)
                {
                    case EngagementScoreStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Ratio + Weight, 2);
    }

    public EngagementScore Copy()
    {
        return new EngagementScore(Code, Label, Minimum, Maximum, Threshold, Location, Weight, Badge, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
