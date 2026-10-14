namespace Acme.Workforce.Assets.Laptop.Audit;

/// <summary>A rule a rolling laptop claim must pass. Check returns its problems, or none.</summary>
public interface IRollingLaptopClaimRule
{
    List<string> Check(RollingLaptopClaim item);
}

public sealed record RollingLaptopClaimJobCodeLengthRule(int MaxLength) : IRollingLaptopClaimRule
{
    public List<string> Check(RollingLaptopClaim item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopClaimNameLengthRule(int MaxLength) : IRollingLaptopClaimRule
{
    public List<string> Check(RollingLaptopClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopClaimRequiredJobCodeRule(string Message) : IRollingLaptopClaimRule
{
    public List<string> Check(RollingLaptopClaim item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopClaimWindowDaysRangeRule(int Min, int Max) : IRollingLaptopClaimRule
{
    public List<string> Check(RollingLaptopClaim item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling laptop claim.</summary>
public sealed record RollingLaptopClaimValidator(List<IRollingLaptopClaimRule> Rules)
{
    public List<string> Validate(RollingLaptopClaim item)
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

    public void RequireValid(RollingLaptopClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLaptopClaim> items)
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

/// <summary>Functions of rolling laptop claim rules.</summary>
public static class RollingLaptopClaimRules
{
    public static RollingLaptopClaimValidator DefaultRollingLaptopClaimValidator()
    {
        return new RollingLaptopClaimValidator(
            new List<IRollingLaptopClaimRule>
            {
                new RollingLaptopClaimJobCodeLengthRule(64),
                new RollingLaptopClaimNameLengthRule(255),
                new RollingLaptopClaimRequiredJobCodeRule("rolling laptop claim needs an entry"),
                new RollingLaptopClaimWindowDaysRangeRule(64, 120),
            });
    }
}
