namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default floor plan baseline is in its life cycle.</summary>
public enum DefaultFloorPlanBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default floor plan baseline. Its code is unique within a tenant.</summary>
public sealed class DefaultFloorPlanBaseline(string code, string label, int retries, string notes, double amount, double ratio, string location, bool archived, Dictionary<string, string> attributes, DefaultFloorPlanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string Notes { get; set; } = notes;
    public double Amount { get; set; } = amount;
    public double Ratio { get; set; } = ratio;
    public string Location { get; set; } = location;
    public bool Archived { get; set; } = archived;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public DefaultFloorPlanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a default floor plan baseline in status draft.</summary>
    public static DefaultFloorPlanBaseline NewDefaultFloorPlanBaseline(string code, string label)
    {
        return new DefaultFloorPlanBaseline(
            code,
            label,
            0,
            "",
            0.0,
            0.0,
            "",
            false,
            new Dictionary<string, string>(),
            DefaultFloorPlanBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultFloorPlanBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultFloorPlanBaselineStatus next)
    {
        switch (Status)
        {
            case DefaultFloorPlanBaselineStatus.Draft:
                return next == DefaultFloorPlanBaselineStatus.Pending || next == DefaultFloorPlanBaselineStatus.Closed;
            case DefaultFloorPlanBaselineStatus.Pending:
                switch (next)
                {
                    case DefaultFloorPlanBaselineStatus.Closed:
                        return true;
                }
                return false;
            case DefaultFloorPlanBaselineStatus.Active:
                switch (next)
                {
                    case DefaultFloorPlanBaselineStatus.Closed:
                        return true;
                }
                return false;
            case DefaultFloorPlanBaselineStatus.Suspended:
                return next == DefaultFloorPlanBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultFloorPlanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default floor plan baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default floor plan baseline"} {Code}";
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

    public double AdjustedLoad()
    {
        return Math.Round(Amount + Ratio, 2);
    }

    public DefaultFloorPlanBaseline Copy()
    {
        return new DefaultFloorPlanBaseline(
            Code,
            Label,
            Retries,
            Notes,
            Amount,
            Ratio,
            Location,
            Archived,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
