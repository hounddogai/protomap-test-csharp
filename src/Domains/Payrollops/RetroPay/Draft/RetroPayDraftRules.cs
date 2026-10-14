namespace Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>A rule a retro pay draft must pass. Check returns its problems, or none.</summary>
public interface IRetroPayDraftRule
{
    List<string> Check(RetroPayDraft item);
}

public sealed record RetroPayDraftTitleLengthRule(int MaxLength) : IRetroPayDraftRule
{
    public List<string> Check(RetroPayDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDraftRequiredCodeRule(string Message) : IRetroPayDraftRule
{
    public List<string> Check(RetroPayDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDraftNameLengthRule(int MaxLength) : IRetroPayDraftRule
{
    public List<string> Check(RetroPayDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDraftRequiredPhoneRule(string Message) : IRetroPayDraftRule
{
    public List<string> Check(RetroPayDraft item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayDraftWeightRangeRule(double Min, double Max) : IRetroPayDraftRule
{
    public List<string> Check(RetroPayDraft item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay draft.</summary>
public sealed record RetroPayDraftValidator(List<IRetroPayDraftRule> Rules)
{
    public List<string> Validate(RetroPayDraft item)
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

    public void RequireValid(RetroPayDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayDraft> items)
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

/// <summary>Functions of retro pay draft rules.</summary>
public static class RetroPayDraftRules
{
    public static RetroPayDraftValidator DefaultRetroPayDraftValidator()
    {
        return new RetroPayDraftValidator(
            new List<IRetroPayDraftRule>
            {
                new RetroPayDraftTitleLengthRule(255),
                new RetroPayDraftRequiredCodeRule("retro pay draft needs an entry"),
                new RetroPayDraftNameLengthRule(64),
                new RetroPayDraftRequiredPhoneRule("retro pay draft needs an entry"),
                new RetroPayDraftWeightRangeRule(0.3, 3050.7),
            });
    }
}
