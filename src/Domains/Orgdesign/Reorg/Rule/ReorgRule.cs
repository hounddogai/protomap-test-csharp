namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg rule is in its life cycle.</summary>
public enum ReorgRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a reorg rule and its current status.</summary>
public sealed class ReorgRule(string code, string name, double cost, int createdAt, string skill, ReorgRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int CreatedAt { get; set; } = createdAt;
    public string Skill { get; set; } = skill;
    public ReorgRuleStatus Status { get; set; } = status;

    /// <summary>Starts a reorg rule in status created.</summary>
    public static ReorgRule NewReorgRule(string code, string name)
    {
        return new ReorgRule(code, name, 0.0, 0, "", ReorgRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgRuleStatus next)
    {
        switch (Status)
        {
            case ReorgRuleStatus.Created:
                return next == ReorgRuleStatus.Validated || next == ReorgRuleStatus.Retired;
            case ReorgRuleStatus.Validated:
                switch (next)
                {
                    case ReorgRuleStatus.Published:
                    case ReorgRuleStatus.Retired:
                        return true;
                }
                return false;
            case ReorgRuleStatus.Published:
                switch (next)
                {
                    case ReorgRuleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"reorg rule"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Cost * (double)CreatedAt, 2);
    }

    public ReorgRule Copy()
    {
        return new ReorgRule(Code, Name, Cost, CreatedAt, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
