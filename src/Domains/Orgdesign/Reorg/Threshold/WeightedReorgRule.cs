namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted reorg rule is in its life cycle.</summary>
public enum WeightedReorgRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a weighted reorg rule and its current status.</summary>
public sealed class WeightedReorgRule(string code, string name, int version, Dictionary<string, string> attributes, string phone, WeightedReorgRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Phone { get; set; } = phone;
    public WeightedReorgRuleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted reorg rule in status proposed.</summary>
    public static WeightedReorgRule NewWeightedReorgRule(string code, string name)
    {
        return new WeightedReorgRule(
            code,
            name,
            0,
            new Dictionary<string, string>(),
            "",
            WeightedReorgRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedReorgRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedReorgRuleStatus next)
    {
        switch (Status)
        {
            case WeightedReorgRuleStatus.Proposed:
                switch (next)
                {
                    case WeightedReorgRuleStatus.Rejected:
                        return true;
                }
                return false;
            case WeightedReorgRuleStatus.Approved:
                return next == WeightedReorgRuleStatus.Rejected || next == WeightedReorgRuleStatus.Withdrawn;
            case WeightedReorgRuleStatus.Rejected:
                switch (next)
                {
                    case WeightedReorgRuleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedReorgRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted reorg rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
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

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public WeightedReorgRule Copy()
    {
        return new WeightedReorgRule(Code, Name, Version, Attributes, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
