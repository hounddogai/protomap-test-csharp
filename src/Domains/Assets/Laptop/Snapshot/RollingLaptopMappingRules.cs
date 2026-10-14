namespace Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>A rule a rolling laptop mapping must pass. Check returns its problems, or none.</summary>
public interface IRollingLaptopMappingRule
{
    List<string> Check(RollingLaptopMapping item);
}

public sealed record RollingLaptopMappingSkillLengthRule(int MaxLength) : IRollingLaptopMappingRule
{
    public List<string> Check(RollingLaptopMapping item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopMappingRequiredNameRule(string Message) : IRollingLaptopMappingRule
{
    public List<string> Check(RollingLaptopMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopMappingRequiredCodeRule(string Message) : IRollingLaptopMappingRule
{
    public List<string> Check(RollingLaptopMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopMappingCodeLengthRule(int MaxLength) : IRollingLaptopMappingRule
{
    public List<string> Check(RollingLaptopMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopMappingRequiredOwnerIdRule(string Message) : IRollingLaptopMappingRule
{
    public List<string> Check(RollingLaptopMapping item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling laptop mapping.</summary>
public sealed record RollingLaptopMappingValidator(List<IRollingLaptopMappingRule> Rules)
{
    public List<string> Validate(RollingLaptopMapping item)
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

    public void RequireValid(RollingLaptopMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLaptopMapping> items)
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

/// <summary>Functions of rolling laptop mapping rules.</summary>
public static class RollingLaptopMappingRules
{
    public static RollingLaptopMappingValidator DefaultRollingLaptopMappingValidator()
    {
        return new RollingLaptopMappingValidator(
            new List<IRollingLaptopMappingRule>
            {
                new RollingLaptopMappingSkillLengthRule(255),
                new RollingLaptopMappingRequiredNameRule("rolling laptop mapping needs this field"),
                new RollingLaptopMappingRequiredCodeRule("rolling laptop mapping needs an entry"),
                new RollingLaptopMappingCodeLengthRule(120),
                new RollingLaptopMappingRequiredOwnerIdRule("rolling laptop mapping needs this field"),
            });
    }
}
