namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback town hall draft must pass. Check returns its problems, or none.</summary>
public interface IFallbackTownHallDraftRule
{
    List<string> Check(FallbackTownHallDraft item);
}

public sealed record FallbackTownHallDraftCodeLengthRule(int MaxLength) : IFallbackTownHallDraftRule
{
    public List<string> Check(FallbackTownHallDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTownHallDraftRequiredChecksumRule(string Message) : IFallbackTownHallDraftRule
{
    public List<string> Check(FallbackTownHallDraft item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTownHallDraftRequiredSalaryBandRule(string Message) : IFallbackTownHallDraftRule
{
    public List<string> Check(FallbackTownHallDraft item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackTownHallDraftAllowedChecksumRule(List<string> Allowed) : IFallbackTownHallDraftRule
{
    public List<string> Check(FallbackTownHallDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback town hall draft.</summary>
public sealed record FallbackTownHallDraftValidator(List<IFallbackTownHallDraftRule> Rules)
{
    public List<string> Validate(FallbackTownHallDraft item)
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

    public void RequireValid(FallbackTownHallDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTownHallDraft> items)
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

/// <summary>Functions of fallback town hall draft rules.</summary>
public static class FallbackTownHallDraftRules
{
    public static FallbackTownHallDraftValidator DefaultFallbackTownHallDraftValidator()
    {
        return new FallbackTownHallDraftValidator(
            new List<IFallbackTownHallDraftRule>
            {
                new FallbackTownHallDraftCodeLengthRule(64),
                new FallbackTownHallDraftRequiredChecksumRule("fallback town hall draft needs this field"),
                new FallbackTownHallDraftRequiredSalaryBandRule("fallback town hall draft needs this field"),
                new FallbackTownHallDraftAllowedChecksumRule(new List<string>()),
            });
    }
}
