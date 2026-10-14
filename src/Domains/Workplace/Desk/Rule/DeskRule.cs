namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk rule is in its life cycle.</summary>
public enum DeskRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A desk rule. Its code is unique within a tenant.</summary>
public sealed class DeskRule(string code, string name, string grade, int endsAt, string skill, string source, int graceDays, double weight, Dictionary<string, string> attributes, string unit, string description, DeskRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public string Source { get; set; } = source;
    public int GraceDays { get; set; } = graceDays;
    public double Weight { get; set; } = weight;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public DeskRuleStatus Status { get; set; } = status;

    /// <summary>Starts a desk rule in status proposed.</summary>
    public static DeskRule NewDeskRule(string code, string name)
    {
        return new DeskRule(
            code,
            name,
            "",
            0,
            "",
            "",
            0,
            0.0,
            new Dictionary<string, string>(),
            "",
            "",
            DeskRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskRuleStatus next)
    {
        switch (Status)
        {
            case DeskRuleStatus.Proposed:
                switch (next)
                {
                    case DeskRuleStatus.Approved:
                    case DeskRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeskRuleStatus.Approved:
                switch (next)
                {
                    case DeskRuleStatus.Rejected:
                    case DeskRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeskRuleStatus.Rejected:
                return next == DeskRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeskRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt * (double)GraceDays, 2);
    }

    public DeskRule Copy()
    {
        return new DeskRule(
            Code,
            Name,
            Grade,
            EndsAt,
            Skill,
            Source,
            GraceDays,
            Weight,
            Attributes,
            Unit,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
