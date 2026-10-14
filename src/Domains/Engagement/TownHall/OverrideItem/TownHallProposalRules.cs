namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall proposal must pass. Check returns its problems, or none.</summary>
public interface ITownHallProposalRule
{
    List<string> Check(TownHallProposal item);
}

public sealed record TownHallProposalAllowedCodeRule(List<string> Allowed) : ITownHallProposalRule
{
    public List<string> Check(TownHallProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TownHallProposalCodePrefixRule(string Prefix) : ITownHallProposalRule
{
    public List<string> Check(TownHallProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallProposalNameLengthRule(int MaxLength) : ITownHallProposalRule
{
    public List<string> Check(TownHallProposal item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallProposalVersionRangeRule(int Min, int Max) : ITownHallProposalRule
{
    public List<string> Check(TownHallProposal item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallProposalRequiredSkillRule(string Message) : ITownHallProposalRule
{
    public List<string> Check(TownHallProposal item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall proposal.</summary>
public sealed record TownHallProposalValidator(List<ITownHallProposalRule> Rules)
{
    public List<string> Validate(TownHallProposal item)
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

    public void RequireValid(TownHallProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallProposal> items)
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

/// <summary>Functions of town hall proposal rules.</summary>
public static class TownHallProposalRules
{
    public static TownHallProposalValidator DefaultTownHallProposalValidator()
    {
        return new TownHallProposalValidator(
            new List<ITownHallProposalRule>
            {
                new TownHallProposalAllowedCodeRule(new List<string>()),
                new TownHallProposalCodePrefixRule("Q"),
                new TownHallProposalNameLengthRule(86400),
                new TownHallProposalVersionRangeRule(16, 32),
                new TownHallProposalRequiredSkillRule("town hall proposal needs a value"),
            });
    }
}
