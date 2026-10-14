namespace Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>A rule a refresh template must pass. Check returns its problems, or none.</summary>
public interface IRefreshTemplateRule
{
    List<string> Check(RefreshTemplate item);
}

public sealed record RefreshTemplateRequiredTitleRule(string Message) : IRefreshTemplateRule
{
    public List<string> Check(RefreshTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshTemplateRequiredChecksumRule(string Message) : IRefreshTemplateRule
{
    public List<string> Check(RefreshTemplate item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshTemplateTitleLengthRule(int MaxLength) : IRefreshTemplateRule
{
    public List<string> Check(RefreshTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshTemplateRequiredContactEmailRule(string Message) : IRefreshTemplateRule
{
    public List<string> Check(RefreshTemplate item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshTemplateChecksumLengthRule(int MaxLength) : IRefreshTemplateRule
{
    public List<string> Check(RefreshTemplate item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh template.</summary>
public sealed record RefreshTemplateValidator(List<IRefreshTemplateRule> Rules)
{
    public List<string> Validate(RefreshTemplate item)
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

    public void RequireValid(RefreshTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshTemplate> items)
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

/// <summary>Functions of refresh template rules.</summary>
public static class RefreshTemplateRules
{
    public static RefreshTemplateValidator DefaultRefreshTemplateValidator()
    {
        return new RefreshTemplateValidator(
            new List<IRefreshTemplateRule>
            {
                new RefreshTemplateRequiredTitleRule("refresh template needs an entry"),
                new RefreshTemplateRequiredChecksumRule("refresh template needs a value"),
                new RefreshTemplateTitleLengthRule(64),
                new RefreshTemplateRequiredContactEmailRule("refresh template needs a value"),
                new RefreshTemplateChecksumLengthRule(80),
            });
    }
}
