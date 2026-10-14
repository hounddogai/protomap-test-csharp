namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted visitor baseline is in its life cycle.</summary>
public enum WeightedVisitorBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a weighted visitor baseline and its current status.</summary>
public sealed class WeightedVisitorBaseline(string code, string label, string ownerId, string segment, string skill, int version, int revision, double minimum, string description, double discount, string region, WeightedVisitorBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Segment { get; set; } = segment;
    public string Skill { get; set; } = skill;
    public int Version { get; set; } = version;
    public int Revision { get; set; } = revision;
    public double Minimum { get; set; } = minimum;
    public string Description { get; set; } = description;
    public double Discount { get; set; } = discount;
    public string Region { get; set; } = region;
    public WeightedVisitorBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a weighted visitor baseline in status open.</summary>
    public static WeightedVisitorBaseline NewWeightedVisitorBaseline(string code, string label)
    {
        return new WeightedVisitorBaseline(
            code,
            label,
            "",
            "",
            "",
            0,
            0,
            0.0,
            "",
            0.0,
            "",
            WeightedVisitorBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedVisitorBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedVisitorBaselineStatus next)
    {
        switch (Status)
        {
            case WeightedVisitorBaselineStatus.Open:
                return next == WeightedVisitorBaselineStatus.Resolved;
            case WeightedVisitorBaselineStatus.InReview:
                switch (next)
                {
                    case WeightedVisitorBaselineStatus.Resolved:
                        return true;
                }
                return false;
            case WeightedVisitorBaselineStatus.Resolved:
                switch (next)
                {
                    case WeightedVisitorBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case WeightedVisitorBaselineStatus.Reopened:
                switch (next)
                {
                    case WeightedVisitorBaselineStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedVisitorBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted visitor baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Version * (double)Revision, 2);
    }

    public WeightedVisitorBaseline Copy()
    {
        return new WeightedVisitorBaseline(
            Code,
            Label,
            OwnerId,
            Segment,
            Skill,
            Version,
            Revision,
            Minimum,
            Description,
            Discount,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
