namespace Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>A rule a default refresh slot must pass. Check returns its problems, or none.</summary>
public interface IDefaultRefreshSlotRule
{
    List<string> Check(DefaultRefreshSlot item);
}

public sealed record DefaultRefreshSlotRevisionRangeRule(int Min, int Max) : IDefaultRefreshSlotRule
{
    public List<string> Check(DefaultRefreshSlot item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRefreshSlotActiveGuardRule(double Limit) : IDefaultRefreshSlotRule
{
    public List<string> Check(DefaultRefreshSlot item)
    {
        if (item.Active && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRefreshSlotRequiredCodeRule(string Message) : IDefaultRefreshSlotRule
{
    public List<string> Check(DefaultRefreshSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultRefreshSlotSalaryBandLengthRule(int MaxLength) : IDefaultRefreshSlotRule
{
    public List<string> Check(DefaultRefreshSlot item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultRefreshSlotRequiredSalaryBandRule(string Message) : IDefaultRefreshSlotRule
{
    public List<string> Check(DefaultRefreshSlot item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default refresh slot.</summary>
public sealed record DefaultRefreshSlotValidator(List<IDefaultRefreshSlotRule> Rules)
{
    public List<string> Validate(DefaultRefreshSlot item)
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

    public void RequireValid(DefaultRefreshSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultRefreshSlot> items)
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

/// <summary>Functions of default refresh slot rules.</summary>
public static class DefaultRefreshSlotRules
{
    public static DefaultRefreshSlotValidator DefaultDefaultRefreshSlotValidator()
    {
        return new DefaultRefreshSlotValidator(
            new List<IDefaultRefreshSlotRule>
            {
                new DefaultRefreshSlotRevisionRangeRule(64, 86400),
                new DefaultRefreshSlotActiveGuardRule(3.6),
                new DefaultRefreshSlotRequiredCodeRule("default refresh slot needs a value"),
                new DefaultRefreshSlotSalaryBandLengthRule(3600),
                new DefaultRefreshSlotRequiredSalaryBandRule("default refresh slot needs this field"),
            });
    }
}
