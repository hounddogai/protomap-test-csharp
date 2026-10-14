namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path claim must pass. Check returns its problems, or none.</summary>
public interface ICareerPathClaimRule
{
    List<string> Check(CareerPathClaim item);
}

public sealed record CareerPathClaimAllowedNameRule(List<string> Allowed) : ICareerPathClaimRule
{
    public List<string> Check(CareerPathClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathClaimRequiredNameRule(string Message) : ICareerPathClaimRule
{
    public List<string> Check(CareerPathClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathClaimRequiredCodeRule(string Message) : ICareerPathClaimRule
{
    public List<string> Check(CareerPathClaim item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathClaimLocaleLengthRule(int MaxLength) : ICareerPathClaimRule
{
    public List<string> Check(CareerPathClaim item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathClaimTagsLimitRule(int MaxCount, List<string> Banned) : ICareerPathClaimRule
{
    public List<string> Check(CareerPathClaim item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a career path claim.</summary>
public sealed record CareerPathClaimValidator(List<ICareerPathClaimRule> Rules)
{
    public List<string> Validate(CareerPathClaim item)
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

    public void RequireValid(CareerPathClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathClaim> items)
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

/// <summary>Functions of career path claim rules.</summary>
public static class CareerPathClaimRules
{
    public static CareerPathClaimValidator DefaultCareerPathClaimValidator()
    {
        return new CareerPathClaimValidator(
            new List<ICareerPathClaimRule>
            {
                new CareerPathClaimAllowedNameRule(new List<string> { "rolling" }),
                new CareerPathClaimRequiredNameRule("career path claim needs an entry"),
                new CareerPathClaimRequiredCodeRule("career path claim needs this field"),
                new CareerPathClaimLocaleLengthRule(255),
                new CareerPathClaimTagsLimitRule(80, new List<string> { "global", "weighted", "batched" }),
            });
    }
}
