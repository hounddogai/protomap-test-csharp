namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path profile must pass. Check returns its problems, or none.</summary>
public interface ICareerPathProfileRule
{
    List<string> Check(CareerPathProfile item);
}

public sealed record CareerPathProfileCodePrefixRule(string Prefix) : ICareerPathProfileRule
{
    public List<string> Check(CareerPathProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathProfileApprovedGuardRule(double Limit) : ICareerPathProfileRule
{
    public List<string> Check(CareerPathProfile item)
    {
        if (item.Approved && item.Margin > Limit)
        {
            return new List<string> { $"margin {item.Margin} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathProfileNameLengthRule(int MaxLength) : ICareerPathProfileRule
{
    public List<string> Check(CareerPathProfile item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathProfileRequiredCodeRule(string Message) : ICareerPathProfileRule
{
    public List<string> Check(CareerPathProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathProfilePhoneLengthRule(int MaxLength) : ICareerPathProfileRule
{
    public List<string> Check(CareerPathProfile item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathProfileAllowedNameRule(List<string> Allowed) : ICareerPathProfileRule
{
    public List<string> Check(CareerPathProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path profile.</summary>
public sealed record CareerPathProfileValidator(List<ICareerPathProfileRule> Rules)
{
    public List<string> Validate(CareerPathProfile item)
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

    public void RequireValid(CareerPathProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathProfile> items)
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

/// <summary>Functions of career path profile rules.</summary>
public static class CareerPathProfileRules
{
    public static CareerPathProfileValidator DefaultCareerPathProfileValidator()
    {
        return new CareerPathProfileValidator(
            new List<ICareerPathProfileRule>
            {
                new CareerPathProfileCodePrefixRule("REF"),
                new CareerPathProfileApprovedGuardRule(8.4),
                new CareerPathProfileNameLengthRule(80),
                new CareerPathProfileRequiredCodeRule("career path profile needs a value"),
                new CareerPathProfilePhoneLengthRule(80),
                new CareerPathProfileAllowedNameRule(new List<string> { "batched", "primary" }),
            });
    }
}
