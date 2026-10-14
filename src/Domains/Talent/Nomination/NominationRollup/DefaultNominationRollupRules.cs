namespace Acme.Workforce.Talent.Nomination.NominationRollup;

/// <summary>A rule a default nomination rollup must pass. Check returns its problems, or none.</summary>
public interface IDefaultNominationRollupRule
{
    List<string> Check(DefaultNominationRollup item);
}

public sealed record DefaultNominationRollupCodeLengthRule(int MaxLength) : IDefaultNominationRollupRule
{
    public List<string> Check(DefaultNominationRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultNominationRollupDescriptionLengthRule(int MaxLength) : IDefaultNominationRollupRule
{
    public List<string> Check(DefaultNominationRollup item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultNominationRollupRequiredContactEmailRule(string Message) : IDefaultNominationRollupRule
{
    public List<string> Check(DefaultNominationRollup item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultNominationRollupRequiredLabelRule(string Message) : IDefaultNominationRollupRule
{
    public List<string> Check(DefaultNominationRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultNominationRollupRequiredDescriptionRule(string Message) : IDefaultNominationRollupRule
{
    public List<string> Check(DefaultNominationRollup item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default nomination rollup.</summary>
public sealed record DefaultNominationRollupValidator(List<IDefaultNominationRollupRule> Rules)
{
    public List<string> Validate(DefaultNominationRollup item)
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

    public void RequireValid(DefaultNominationRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultNominationRollup> items)
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

/// <summary>Functions of default nomination rollup rules.</summary>
public static class DefaultNominationRollupRules
{
    public static DefaultNominationRollupValidator DefaultDefaultNominationRollupValidator()
    {
        return new DefaultNominationRollupValidator(
            new List<IDefaultNominationRollupRule>
            {
                new DefaultNominationRollupCodeLengthRule(86400),
                new DefaultNominationRollupDescriptionLengthRule(3600),
                new DefaultNominationRollupRequiredContactEmailRule("default nomination rollup needs a value"),
                new DefaultNominationRollupRequiredLabelRule("default nomination rollup needs a value"),
                new DefaultNominationRollupRequiredDescriptionRule("default nomination rollup needs a value"),
            });
    }
}
