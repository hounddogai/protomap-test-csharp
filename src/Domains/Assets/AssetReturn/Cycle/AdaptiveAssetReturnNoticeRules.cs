namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive asset return notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAssetReturnNoticeRule
{
    List<string> Check(AdaptiveAssetReturnNotice item);
}

public sealed record AdaptiveAssetReturnNoticeHoursRangeRule(double Min, double Max) : IAdaptiveAssetReturnNoticeRule
{
    public List<string> Check(AdaptiveAssetReturnNotice item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveAssetReturnNoticeRequiredTitleRule(string Message) : IAdaptiveAssetReturnNoticeRule
{
    public List<string> Check(AdaptiveAssetReturnNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAssetReturnNoticeAllowedCodeRule(List<string> Allowed) : IAdaptiveAssetReturnNoticeRule
{
    public List<string> Check(AdaptiveAssetReturnNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAssetReturnNoticeCodeLengthRule(int MaxLength) : IAdaptiveAssetReturnNoticeRule
{
    public List<string> Check(AdaptiveAssetReturnNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAssetReturnNoticeTitleLengthRule(int MaxLength) : IAdaptiveAssetReturnNoticeRule
{
    public List<string> Check(AdaptiveAssetReturnNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAssetReturnNoticeRequiredSkillRule(string Message) : IAdaptiveAssetReturnNoticeRule
{
    public List<string> Check(AdaptiveAssetReturnNotice item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive asset return notice.</summary>
public sealed record AdaptiveAssetReturnNoticeValidator(List<IAdaptiveAssetReturnNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveAssetReturnNotice item)
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

    public void RequireValid(AdaptiveAssetReturnNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAssetReturnNotice> items)
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

/// <summary>Functions of adaptive asset return notice rules.</summary>
public static class AdaptiveAssetReturnNoticeRules
{
    public static AdaptiveAssetReturnNoticeValidator DefaultAdaptiveAssetReturnNoticeValidator()
    {
        return new AdaptiveAssetReturnNoticeValidator(
            new List<IAdaptiveAssetReturnNoticeRule>
            {
                new AdaptiveAssetReturnNoticeHoursRangeRule(7.0, 2013.6),
                new AdaptiveAssetReturnNoticeRequiredTitleRule("adaptive asset return notice needs a value"),
                new AdaptiveAssetReturnNoticeAllowedCodeRule(new List<string> { "default" }),
                new AdaptiveAssetReturnNoticeCodeLengthRule(32),
                new AdaptiveAssetReturnNoticeTitleLengthRule(64),
                new AdaptiveAssetReturnNoticeRequiredSkillRule("adaptive asset return notice needs an entry"),
            });
    }
}
