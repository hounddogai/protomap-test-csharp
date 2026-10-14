namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount rule is in its life cycle.</summary>
public enum HeadcountRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a headcount rule and its current status.</summary>
public sealed class HeadcountRule(string code, string name, Dictionary<string, string> attributes, string team, string channel, string costCenter, double floorValue, string phone, string employeeId, List<string> tags, HeadcountRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public string Channel { get; set; } = channel;
    public string CostCenter { get; set; } = costCenter;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public HeadcountRuleStatus Status { get; set; } = status;

    /// <summary>Starts a headcount rule in status planned.</summary>
    public static HeadcountRule NewHeadcountRule(string code, string name)
    {
        return new HeadcountRule(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            "",
            "",
            new List<string>(),
            HeadcountRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountRuleStatus next)
    {
        switch (Status)
        {
            case HeadcountRuleStatus.Planned:
                switch (next)
                {
                    case HeadcountRuleStatus.Completed:
                    case HeadcountRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountRuleStatus.Scheduled:
                switch (next)
                {
                    case HeadcountRuleStatus.Completed:
                    case HeadcountRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountRuleStatus.InProgress:
                return next == HeadcountRuleStatus.Completed;
            case HeadcountRuleStatus.Completed:
                switch (next)
                {
                    case HeadcountRuleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"headcount rule"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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
        return FloorValue;
    }

    public HeadcountRule Copy()
    {
        return new HeadcountRule(
            Code,
            Name,
            Attributes,
            Team,
            Channel,
            CostCenter,
            FloorValue,
            Phone,
            EmployeeId,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
