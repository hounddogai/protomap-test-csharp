namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient statement of work bundle must pass. Check returns its problems, or none.</summary>
public interface ILenientStatementOfWorkBundleRule
{
    List<string> Check(LenientStatementOfWorkBundle item);
}

public sealed record LenientStatementOfWorkBundleNameLengthRule(int MaxLength) : ILenientStatementOfWorkBundleRule
{
    public List<string> Check(LenientStatementOfWorkBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkBundleCodeLengthRule(int MaxLength) : ILenientStatementOfWorkBundleRule
{
    public List<string> Check(LenientStatementOfWorkBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkBundleRequiredCodeRule(string Message) : ILenientStatementOfWorkBundleRule
{
    public List<string> Check(LenientStatementOfWorkBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkBundleAllowedDescriptionRule(List<string> Allowed) : ILenientStatementOfWorkBundleRule
{
    public List<string> Check(LenientStatementOfWorkBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkBundleRequiredNameRule(string Message) : ILenientStatementOfWorkBundleRule
{
    public List<string> Check(LenientStatementOfWorkBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkBundleLabelLengthRule(int MaxLength) : ILenientStatementOfWorkBundleRule
{
    public List<string> Check(LenientStatementOfWorkBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient statement of work bundle.</summary>
public sealed record LenientStatementOfWorkBundleValidator(List<ILenientStatementOfWorkBundleRule> Rules)
{
    public List<string> Validate(LenientStatementOfWorkBundle item)
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

    public void RequireValid(LenientStatementOfWorkBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientStatementOfWorkBundle> items)
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

/// <summary>Functions of lenient statement of work bundle rules.</summary>
public static class LenientStatementOfWorkBundleRules
{
    public static LenientStatementOfWorkBundleValidator DefaultLenientStatementOfWorkBundleValidator()
    {
        return new LenientStatementOfWorkBundleValidator(
            new List<ILenientStatementOfWorkBundleRule>
            {
                new LenientStatementOfWorkBundleNameLengthRule(3600),
                new LenientStatementOfWorkBundleCodeLengthRule(16),
                new LenientStatementOfWorkBundleRequiredCodeRule("lenient statement of work bundle needs an entry"),
                new LenientStatementOfWorkBundleAllowedDescriptionRule(new List<string>()),
                new LenientStatementOfWorkBundleRequiredNameRule("lenient statement of work bundle needs a value"),
                new LenientStatementOfWorkBundleLabelLengthRule(16),
            });
    }
}
