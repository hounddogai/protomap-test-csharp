namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient pool draft must pass. Check returns its problems, or none.</summary>
public interface ILenientPoolDraftRule
{
    List<string> Check(LenientPoolDraft item);
}

public sealed record LenientPoolDraftAllowedManagerIdRule(List<string> Allowed) : ILenientPoolDraftRule
{
    public List<string> Check(LenientPoolDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is missing" };
        }
        return new List<string>();
    }
}

public sealed record LenientPoolDraftRequiredTitleRule(string Message) : ILenientPoolDraftRule
{
    public List<string> Check(LenientPoolDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPoolDraftRequiredCodeRule(string Message) : ILenientPoolDraftRule
{
    public List<string> Check(LenientPoolDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient pool draft.</summary>
public sealed record LenientPoolDraftValidator(List<ILenientPoolDraftRule> Rules)
{
    public List<string> Validate(LenientPoolDraft item)
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

    public void RequireValid(LenientPoolDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPoolDraft> items)
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

/// <summary>Functions of lenient pool draft rules.</summary>
public static class LenientPoolDraftRules
{
    public static LenientPoolDraftValidator DefaultLenientPoolDraftValidator()
    {
        return new LenientPoolDraftValidator(
            new List<ILenientPoolDraftRule>
            {
                new LenientPoolDraftAllowedManagerIdRule(new List<string> { "scheduled" }),
                new LenientPoolDraftRequiredTitleRule("lenient pool draft needs a value"),
                new LenientPoolDraftRequiredCodeRule("lenient pool draft needs an entry"),
            });
    }
}
