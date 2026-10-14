namespace Acme.Workforce.Engagement.TownHall.Target;

/// <summary>A rule a lenient town hall target must pass. Check returns its problems, or none.</summary>
public interface ILenientTownHallTargetRule
{
    List<string> Check(LenientTownHallTarget item);
}

public sealed record LenientTownHallTargetApprovedGuardRule(double Limit) : ILenientTownHallTargetRule
{
    public List<string> Check(LenientTownHallTarget item)
    {
        if (item.Approved && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record LenientTownHallTargetCodeLengthRule(int MaxLength) : ILenientTownHallTargetRule
{
    public List<string> Check(LenientTownHallTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTownHallTargetRequiredNameRule(string Message) : ILenientTownHallTargetRule
{
    public List<string> Check(LenientTownHallTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient town hall target.</summary>
public sealed record LenientTownHallTargetValidator(List<ILenientTownHallTargetRule> Rules)
{
    public List<string> Validate(LenientTownHallTarget item)
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

    public void RequireValid(LenientTownHallTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTownHallTarget> items)
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

/// <summary>Functions of lenient town hall target rules.</summary>
public static class LenientTownHallTargetRules
{
    public static LenientTownHallTargetValidator DefaultLenientTownHallTargetValidator()
    {
        return new LenientTownHallTargetValidator(
            new List<ILenientTownHallTargetRule>
            {
                new LenientTownHallTargetApprovedGuardRule(7.5),
                new LenientTownHallTargetCodeLengthRule(32),
                new LenientTownHallTargetRequiredNameRule("lenient town hall target needs an entry"),
            });
    }
}
