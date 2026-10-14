namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling desk mapping is in its life cycle.</summary>
public enum RollingDeskMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A rolling desk mapping. Its code is unique within a tenant.</summary>
public sealed class RollingDeskMapping(string code, string label, int priority, string notes, int leaveDays, Dictionary<string, string> attributes, string description, string name, double weight, string segment, RollingDeskMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Notes { get; set; } = notes;
    public int LeaveDays { get; set; } = leaveDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Description { get; set; } = description;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Segment { get; set; } = segment;
    public RollingDeskMappingStatus Status { get; set; } = status;

    /// <summary>Starts a rolling desk mapping in status requested.</summary>
    public static RollingDeskMapping NewRollingDeskMapping(string code, string label)
    {
        return new RollingDeskMapping(
            code,
            label,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            "",
            RollingDeskMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingDeskMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingDeskMappingStatus next)
    {
        switch (Status)
        {
            case RollingDeskMappingStatus.Requested:
                return next == RollingDeskMappingStatus.Revoked;
            case RollingDeskMappingStatus.Granted:
                switch (next)
                {
                    case RollingDeskMappingStatus.Revoked:
                    case RollingDeskMappingStatus.Expired:
                        return true;
                }
                return false;
            case RollingDeskMappingStatus.Revoked:
                switch (next)
                {
                    case RollingDeskMappingStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingDeskMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling desk mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double NetTotal()
    {
        return Math.Round((double)Priority + (double)LeaveDays, 2);
    }

    public RollingDeskMapping Copy()
    {
        return new RollingDeskMapping(
            Code,
            Label,
            Priority,
            Notes,
            LeaveDays,
            Attributes,
            Description,
            Name,
            Weight,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
