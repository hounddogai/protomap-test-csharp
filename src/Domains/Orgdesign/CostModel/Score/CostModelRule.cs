namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model rule is in its life cycle.</summary>
public enum CostModelRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A cost model rule. Its code is unique within a tenant.</summary>
public sealed class CostModelRule(string code, string label, bool active, double ceiling, int expiresAt, string location, string source, List<string> tags, string description, string team, double maximum, CostModelRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Location { get; set; } = location;
    public string Source { get; set; } = source;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public string Team { get; set; } = team;
    public double Maximum { get; set; } = maximum;
    public CostModelRuleStatus Status { get; set; } = status;

    /// <summary>Starts a cost model rule in status requested.</summary>
    public static CostModelRule NewCostModelRule(string code, string label)
    {
        return new CostModelRule(
            code,
            label,
            false,
            0.0,
            0,
            "",
            "",
            new List<string>(),
            "",
            "",
            0.0,
            CostModelRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelRuleStatus next)
    {
        switch (Status)
        {
            case CostModelRuleStatus.Requested:
                switch (next)
                {
                    case CostModelRuleStatus.Granted:
                    case CostModelRuleStatus.Revoked:
                        return true;
                }
                return false;
            case CostModelRuleStatus.Granted:
                return next == CostModelRuleStatus.Expired;
            case CostModelRuleStatus.Revoked:
                return next == CostModelRuleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CostModelRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"cost model rule"} {Code}";
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

    public double NetValue()
    {
        return Math.Round((double)ExpiresAt + Maximum, 2);
    }

    public CostModelRule Copy()
    {
        return new CostModelRule(
            Code,
            Label,
            Active,
            Ceiling,
            ExpiresAt,
            Location,
            Source,
            Tags,
            Description,
            Team,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
