namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global floor plan estimate is in its life cycle.</summary>
public enum GlobalFloorPlanEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a global floor plan estimate and its current status.</summary>
public sealed class GlobalFloorPlanEstimate(string code, string label, string title, int endsAt, string source, Dictionary<string, string> attributes, string badge, GlobalFloorPlanEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string Source { get; set; } = source;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public GlobalFloorPlanEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a global floor plan estimate in status draft.</summary>
    public static GlobalFloorPlanEstimate NewGlobalFloorPlanEstimate(string code, string label)
    {
        return new GlobalFloorPlanEstimate(
            code,
            label,
            "",
            0,
            "",
            new Dictionary<string, string>(),
            "",
            GlobalFloorPlanEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalFloorPlanEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalFloorPlanEstimateStatus next)
    {
        switch (Status)
        {
            case GlobalFloorPlanEstimateStatus.Draft:
                return next == GlobalFloorPlanEstimateStatus.Closed;
            case GlobalFloorPlanEstimateStatus.Pending:
                switch (next)
                {
                    case GlobalFloorPlanEstimateStatus.Closed:
                        return true;
                }
                return false;
            case GlobalFloorPlanEstimateStatus.Active:
                switch (next)
                {
                    case GlobalFloorPlanEstimateStatus.Suspended:
                    case GlobalFloorPlanEstimateStatus.Closed:
                        return true;
                }
                return false;
            case GlobalFloorPlanEstimateStatus.Suspended:
                switch (next)
                {
                    case GlobalFloorPlanEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalFloorPlanEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global floor plan estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public GlobalFloorPlanEstimate Copy()
    {
        return new GlobalFloorPlanEstimate(Code, Label, Title, EndsAt, Source, Attributes, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
