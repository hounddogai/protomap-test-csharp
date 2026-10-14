namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient asset return target must pass. Check returns its problems, or none.</summary>
public interface ILenientAssetReturnTargetRule
{
    List<string> Check(LenientAssetReturnTarget item);
}

public sealed record LenientAssetReturnTargetRequiredCodeRule(string Message) : ILenientAssetReturnTargetRule
{
    public List<string> Check(LenientAssetReturnTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientAssetReturnTargetReviewerLengthRule(int MaxLength) : ILenientAssetReturnTargetRule
{
    public List<string> Check(LenientAssetReturnTarget item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssetReturnTargetAllowedReviewerRule(List<string> Allowed) : ILenientAssetReturnTargetRule
{
    public List<string> Check(LenientAssetReturnTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssetReturnTargetCodeLengthRule(int MaxLength) : ILenientAssetReturnTargetRule
{
    public List<string> Check(LenientAssetReturnTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAssetReturnTargetRequiredNameRule(string Message) : ILenientAssetReturnTargetRule
{
    public List<string> Check(LenientAssetReturnTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient asset return target.</summary>
public sealed record LenientAssetReturnTargetValidator(List<ILenientAssetReturnTargetRule> Rules)
{
    public List<string> Validate(LenientAssetReturnTarget item)
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

    public void RequireValid(LenientAssetReturnTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAssetReturnTarget> items)
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

/// <summary>Functions of lenient asset return target rules.</summary>
public static class LenientAssetReturnTargetRules
{
    public static LenientAssetReturnTargetValidator DefaultLenientAssetReturnTargetValidator()
    {
        return new LenientAssetReturnTargetValidator(
            new List<ILenientAssetReturnTargetRule>
            {
                new LenientAssetReturnTargetRequiredCodeRule("lenient asset return target needs an entry"),
                new LenientAssetReturnTargetReviewerLengthRule(1000),
                new LenientAssetReturnTargetAllowedReviewerRule(new List<string> { "strict", "adaptive" }),
                new LenientAssetReturnTargetCodeLengthRule(86400),
                new LenientAssetReturnTargetRequiredNameRule("lenient asset return target needs an entry"),
            });
    }
}
