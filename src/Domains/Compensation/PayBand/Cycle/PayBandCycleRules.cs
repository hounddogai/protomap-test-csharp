namespace Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>A rule a pay band cycle must pass. Check returns its problems, or none.</summary>
public interface IPayBandCycleRule
{
    List<string> Check(PayBandCycle item);
}

public sealed record PayBandCycleQuantityRangeRule(int Min, int Max) : IPayBandCycleRule
{
    public List<string> Check(PayBandCycle item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandCycleCodeLengthRule(int MaxLength) : IPayBandCycleRule
{
    public List<string> Check(PayBandCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandCycleRequiredManagerIdRule(string Message) : IPayBandCycleRule
{
    public List<string> Check(PayBandCycle item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandCycleSkillLengthRule(int MaxLength) : IPayBandCycleRule
{
    public List<string> Check(PayBandCycle item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandCycleRequiredNameRule(string Message) : IPayBandCycleRule
{
    public List<string> Check(PayBandCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandCycleCostRangeRule(double Min, double Max) : IPayBandCycleRule
{
    public List<string> Check(PayBandCycle item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band cycle.</summary>
public sealed record PayBandCycleValidator(List<IPayBandCycleRule> Rules)
{
    public List<string> Validate(PayBandCycle item)
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

    public void RequireValid(PayBandCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandCycle> items)
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

/// <summary>Functions of pay band cycle rules.</summary>
public static class PayBandCycleRules
{
    public static PayBandCycleValidator DefaultPayBandCycleValidator()
    {
        return new PayBandCycleValidator(
            new List<IPayBandCycleRule>
            {
                new PayBandCycleQuantityRangeRule(120, 86400),
                new PayBandCycleCodeLengthRule(1000),
                new PayBandCycleRequiredManagerIdRule("pay band cycle needs a value"),
                new PayBandCycleSkillLengthRule(86400),
                new PayBandCycleRequiredNameRule("pay band cycle needs this field"),
                new PayBandCycleCostRangeRule(0.4, 3351.6),
            });
    }
}
