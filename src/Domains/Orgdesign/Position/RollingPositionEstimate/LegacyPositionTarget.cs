namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position target is in its life cycle.</summary>
public enum LegacyPositionTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy position target as the position team keeps it.</summary>
public sealed class LegacyPositionTarget(string code, string title, int quantity, Dictionary<string, string> attributes, string location, string unit, double threshold, LegacyPositionTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Location { get; set; } = location;
    public string Unit { get; set; } = unit;
    public double Threshold { get; set; } = threshold;
    public LegacyPositionTargetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position target in status open.</summary>
    public static LegacyPositionTarget NewLegacyPositionTarget(string code, string title)
    {
        return new LegacyPositionTarget(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            LegacyPositionTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionTargetStatus next)
    {
        switch (Status)
        {
            case LegacyPositionTargetStatus.Open:
                switch (next)
                {
                    case LegacyPositionTargetStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyPositionTargetStatus.InReview:
                switch (next)
                {
                    case LegacyPositionTargetStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyPositionTargetStatus.Resolved:
                return next == LegacyPositionTargetStatus.Archived;
            case LegacyPositionTargetStatus.Reopened:
                switch (next)
                {
                    case LegacyPositionTargetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPositionTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Quantity - Threshold, 2);
    }

    public LegacyPositionTarget Copy()
    {
        return new LegacyPositionTarget(Code, Title, Quantity, Attributes, Location, Unit, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
