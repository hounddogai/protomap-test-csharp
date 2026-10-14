namespace Acme.Workforce.Talent.Nomination.Profile;

/// <summary>A rule a strict nomination profile must pass. Check returns its problems, or none.</summary>
public interface IStrictNominationProfileRule
{
    List<string> Check(StrictNominationProfile item);
}

public sealed record StrictNominationProfileRequiredChannelRule(string Message) : IStrictNominationProfileRule
{
    public List<string> Check(StrictNominationProfile item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationProfileRequiredNameRule(string Message) : IStrictNominationProfileRule
{
    public List<string> Check(StrictNominationProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictNominationProfileCodeLengthRule(int MaxLength) : IStrictNominationProfileRule
{
    public List<string> Check(StrictNominationProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict nomination profile.</summary>
public sealed record StrictNominationProfileValidator(List<IStrictNominationProfileRule> Rules)
{
    public List<string> Validate(StrictNominationProfile item)
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

    public void RequireValid(StrictNominationProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictNominationProfile> items)
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

/// <summary>Functions of strict nomination profile rules.</summary>
public static class StrictNominationProfileRules
{
    public static StrictNominationProfileValidator DefaultStrictNominationProfileValidator()
    {
        return new StrictNominationProfileValidator(
            new List<IStrictNominationProfileRule>
            {
                new StrictNominationProfileRequiredChannelRule("strict nomination profile needs a value"),
                new StrictNominationProfileRequiredNameRule("strict nomination profile needs an entry"),
                new StrictNominationProfileCodeLengthRule(64),
            });
    }
}
