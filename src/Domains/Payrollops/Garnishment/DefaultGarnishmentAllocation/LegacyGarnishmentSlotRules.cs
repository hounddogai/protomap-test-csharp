namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>A rule a legacy garnishment slot must pass. Check returns its problems, or none.</summary>
public interface ILegacyGarnishmentSlotRule
{
    List<string> Check(LegacyGarnishmentSlot item);
}

public sealed record LegacyGarnishmentSlotRequiredLabelRule(string Message) : ILegacyGarnishmentSlotRule
{
    public List<string> Check(LegacyGarnishmentSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentSlotArchivedGuardRule(int Limit) : ILegacyGarnishmentSlotRule
{
    public List<string> Check(LegacyGarnishmentSlot item)
    {
        if (item.Archived && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentSlotRequiredCostCenterRule(string Message) : ILegacyGarnishmentSlotRule
{
    public List<string> Check(LegacyGarnishmentSlot item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentSlotHoursRangeRule(double Min, double Max) : ILegacyGarnishmentSlotRule
{
    public List<string> Check(LegacyGarnishmentSlot item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyGarnishmentSlotNameLengthRule(int MaxLength) : ILegacyGarnishmentSlotRule
{
    public List<string> Check(LegacyGarnishmentSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentSlotCodeLengthRule(int MaxLength) : ILegacyGarnishmentSlotRule
{
    public List<string> Check(LegacyGarnishmentSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy garnishment slot.</summary>
public sealed record LegacyGarnishmentSlotValidator(List<ILegacyGarnishmentSlotRule> Rules)
{
    public List<string> Validate(LegacyGarnishmentSlot item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(LegacyGarnishmentSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyGarnishmentSlot> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of legacy garnishment slot rules.</summary>
public static class LegacyGarnishmentSlotRules
{
    public static LegacyGarnishmentSlotValidator DefaultLegacyGarnishmentSlotValidator()
    {
        return new LegacyGarnishmentSlotValidator(
            new List<ILegacyGarnishmentSlotRule>
            {
                new LegacyGarnishmentSlotRequiredLabelRule("legacy garnishment slot needs an entry"),
                new LegacyGarnishmentSlotArchivedGuardRule(86400),
                new LegacyGarnishmentSlotRequiredCostCenterRule("legacy garnishment slot needs this field"),
                new LegacyGarnishmentSlotHoursRangeRule(8.0, 4215.2),
                new LegacyGarnishmentSlotNameLengthRule(64),
                new LegacyGarnishmentSlotCodeLengthRule(3600),
            });
    }
}
