namespace Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>A rule a laptop manifest must pass. Check returns its problems, or none.</summary>
public interface ILaptopManifestRule
{
    List<string> Check(LaptopManifest item);
}

public sealed record LaptopManifestTeamLengthRule(int MaxLength) : ILaptopManifestRule
{
    public List<string> Check(LaptopManifest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopManifestExternalRefLengthRule(int MaxLength) : ILaptopManifestRule
{
    public List<string> Check(LaptopManifest item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopManifestRequiredTeamRule(string Message) : ILaptopManifestRule
{
    public List<string> Check(LaptopManifest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopManifestBudgetRangeRule(double Min, double Max) : ILaptopManifestRule
{
    public List<string> Check(LaptopManifest item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop manifest.</summary>
public sealed record LaptopManifestValidator(List<ILaptopManifestRule> Rules)
{
    public List<string> Validate(LaptopManifest item)
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

    public void RequireValid(LaptopManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopManifest> items)
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

/// <summary>Functions of laptop manifest rules.</summary>
public static class LaptopManifestRules
{
    public static LaptopManifestValidator DefaultLaptopManifestValidator()
    {
        return new LaptopManifestValidator(
            new List<ILaptopManifestRule>
            {
                new LaptopManifestTeamLengthRule(3600),
                new LaptopManifestExternalRefLengthRule(120),
                new LaptopManifestRequiredTeamRule("laptop manifest needs an entry"),
                new LaptopManifestBudgetRangeRule(4.3, 2123.6),
            });
    }
}
