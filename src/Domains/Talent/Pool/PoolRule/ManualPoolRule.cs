namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pool rule is in its life cycle.</summary>
public enum ManualPoolRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One manual pool rule, with what the planners need to act on it.</summary>
public sealed class ManualPoolRule(string code, string name, double fte, int quantity, int limit, string grade, double weight, string title, string label, double amount, int durationMinutes, ManualPoolRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public int Limit { get; set; } = limit;
    public string Grade { get; set; } = grade;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ManualPoolRuleStatus Status { get; set; } = status;

    /// <summary>Starts a manual pool rule in status created.</summary>
    public static ManualPoolRule NewManualPoolRule(string code, string name)
    {
        return new ManualPoolRule(code, name, 0.0, 0, 0, "", 0.0, "", "", 0.0, 0, ManualPoolRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPoolRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPoolRuleStatus next)
    {
        switch (Status)
        {
            case ManualPoolRuleStatus.Created:
                return next == ManualPoolRuleStatus.Validated || next == ManualPoolRuleStatus.Retired;
            case ManualPoolRuleStatus.Validated:
                switch (next)
                {
                    case ManualPoolRuleStatus.Published:
                    case ManualPoolRuleStatus.Retired:
                        return true;
                }
                return false;
            case ManualPoolRuleStatus.Published:
                return next == ManualPoolRuleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualPoolRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pool rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Quantity - Fte, 2);
    }

    public ManualPoolRule Copy()
    {
        return new ManualPoolRule(
            Code,
            Name,
            Fte,
            Quantity,
            Limit,
            Grade,
            Weight,
            Title,
            Label,
            Amount,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
