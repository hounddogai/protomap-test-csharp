namespace Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>A rule a default bonus pool reservation must pass. Check returns its problems, or none.</summary>
public interface IDefaultBonusPoolReservationRule
{
    List<string> Check(DefaultBonusPoolReservation item);
}

public sealed record DefaultBonusPoolReservationRequiredNameRule(string Message) : IDefaultBonusPoolReservationRule
{
    public List<string> Check(DefaultBonusPoolReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolReservationFloorValueRangeRule(double Min, double Max) : IDefaultBonusPoolReservationRule
{
    public List<string> Check(DefaultBonusPoolReservation item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolReservationRequiredCostCenterRule(string Message) : IDefaultBonusPoolReservationRule
{
    public List<string> Check(DefaultBonusPoolReservation item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolReservationRequiredCodeRule(string Message) : IDefaultBonusPoolReservationRule
{
    public List<string> Check(DefaultBonusPoolReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolReservationRequiredUnitRule(string Message) : IDefaultBonusPoolReservationRule
{
    public List<string> Check(DefaultBonusPoolReservation item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBonusPoolReservationCodeLengthRule(int MaxLength) : IDefaultBonusPoolReservationRule
{
    public List<string> Check(DefaultBonusPoolReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default bonus pool reservation.</summary>
public sealed record DefaultBonusPoolReservationValidator(List<IDefaultBonusPoolReservationRule> Rules)
{
    public List<string> Validate(DefaultBonusPoolReservation item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(DefaultBonusPoolReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBonusPoolReservation> items)
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

/// <summary>Functions of default bonus pool reservation rules.</summary>
public static class DefaultBonusPoolReservationRules
{
    public static DefaultBonusPoolReservationValidator DefaultDefaultBonusPoolReservationValidator()
    {
        return new DefaultBonusPoolReservationValidator(
            new List<IDefaultBonusPoolReservationRule>
            {
                new DefaultBonusPoolReservationRequiredNameRule("default bonus pool reservation needs a value"),
                new DefaultBonusPoolReservationFloorValueRangeRule(3.2, 604.0),
                new DefaultBonusPoolReservationRequiredCostCenterRule("default bonus pool reservation needs a value"),
                new DefaultBonusPoolReservationRequiredCodeRule("default bonus pool reservation needs an entry"),
                new DefaultBonusPoolReservationRequiredUnitRule("default bonus pool reservation needs this field"),
                new DefaultBonusPoolReservationCodeLengthRule(86400),
            });
    }
}
