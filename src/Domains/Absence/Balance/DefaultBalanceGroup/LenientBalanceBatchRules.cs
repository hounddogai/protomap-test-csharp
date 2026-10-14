namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient balance batch must pass. Check returns its problems, or none.</summary>
public interface ILenientBalanceBatchRule
{
    List<string> Check(LenientBalanceBatch item);
}

public sealed record LenientBalanceBatchRequiredTitleRule(string Message) : ILenientBalanceBatchRule
{
    public List<string> Check(LenientBalanceBatch item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceBatchAllowedCodeRule(List<string> Allowed) : ILenientBalanceBatchRule
{
    public List<string> Check(LenientBalanceBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceBatchTitleLengthRule(int MaxLength) : ILenientBalanceBatchRule
{
    public List<string> Check(LenientBalanceBatch item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient balance batch.</summary>
public sealed record LenientBalanceBatchValidator(List<ILenientBalanceBatchRule> Rules)
{
    public List<string> Validate(LenientBalanceBatch item)
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

    public void RequireValid(LenientBalanceBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBalanceBatch> items)
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

/// <summary>Functions of lenient balance batch rules.</summary>
public static class LenientBalanceBatchRules
{
    public static LenientBalanceBatchValidator DefaultLenientBalanceBatchValidator()
    {
        return new LenientBalanceBatchValidator(
            new List<ILenientBalanceBatchRule>
            {
                new LenientBalanceBatchRequiredTitleRule("lenient balance batch needs this field"),
                new LenientBalanceBatchAllowedCodeRule(new List<string> { "rolling" }),
                new LenientBalanceBatchTitleLengthRule(120),
            });
    }
}
