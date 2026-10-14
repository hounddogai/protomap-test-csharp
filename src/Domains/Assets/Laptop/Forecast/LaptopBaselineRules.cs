namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop baseline must pass. Check returns its problems, or none.</summary>
public interface ILaptopBaselineRule
{
    List<string> Check(LaptopBaseline item);
}

public sealed record LaptopBaselineAllowedContactEmailRule(List<string> Allowed) : ILaptopBaselineRule
{
    public List<string> Check(LaptopBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBaselineCodeLengthRule(int MaxLength) : ILaptopBaselineRule
{
    public List<string> Check(LaptopBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBaselineRequiredNameRule(string Message) : ILaptopBaselineRule
{
    public List<string> Check(LaptopBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopBaselineNameLengthRule(int MaxLength) : ILaptopBaselineRule
{
    public List<string> Check(LaptopBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBaselineCeilingRangeRule(double Min, double Max) : ILaptopBaselineRule
{
    public List<string> Check(LaptopBaseline item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop baseline.</summary>
public sealed record LaptopBaselineValidator(List<ILaptopBaselineRule> Rules)
{
    public List<string> Validate(LaptopBaseline item)
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

    public void RequireValid(LaptopBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopBaseline> items)
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

/// <summary>Functions of laptop baseline rules.</summary>
public static class LaptopBaselineRules
{
    public static LaptopBaselineValidator DefaultLaptopBaselineValidator()
    {
        return new LaptopBaselineValidator(
            new List<ILaptopBaselineRule>
            {
                new LaptopBaselineAllowedContactEmailRule(new List<string> { "adaptive", "global" }),
                new LaptopBaselineCodeLengthRule(3600),
                new LaptopBaselineRequiredNameRule("laptop baseline needs this field"),
                new LaptopBaselineNameLengthRule(1000),
                new LaptopBaselineCeilingRangeRule(9.9, 4749.8),
            });
    }
}
