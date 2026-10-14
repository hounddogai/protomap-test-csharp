namespace Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>A rule a return plan draft must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanDraftRule
{
    List<string> Check(ReturnPlanDraft item);
}

public sealed record ReturnPlanDraftRequiredLabelRule(string Message) : IReturnPlanDraftRule
{
    public List<string> Check(ReturnPlanDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDraftRequiredCodeRule(string Message) : IReturnPlanDraftRule
{
    public List<string> Check(ReturnPlanDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDraftRequiredCurrencyRule(string Message) : IReturnPlanDraftRule
{
    public List<string> Check(ReturnPlanDraft item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDraftLabelLengthRule(int MaxLength) : IReturnPlanDraftRule
{
    public List<string> Check(ReturnPlanDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDraftCurrencyLengthRule(int MaxLength) : IReturnPlanDraftRule
{
    public List<string> Check(ReturnPlanDraft item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan draft.</summary>
public sealed record ReturnPlanDraftValidator(List<IReturnPlanDraftRule> Rules)
{
    public List<string> Validate(ReturnPlanDraft item)
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

    public void RequireValid(ReturnPlanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanDraft> items)
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

/// <summary>Functions of return plan draft rules.</summary>
public static class ReturnPlanDraftRules
{
    public static ReturnPlanDraftValidator DefaultReturnPlanDraftValidator()
    {
        return new ReturnPlanDraftValidator(
            new List<IReturnPlanDraftRule>
            {
                new ReturnPlanDraftRequiredLabelRule("return plan draft needs an entry"),
                new ReturnPlanDraftRequiredCodeRule("return plan draft needs an entry"),
                new ReturnPlanDraftRequiredCurrencyRule("return plan draft needs an entry"),
                new ReturnPlanDraftLabelLengthRule(255),
                new ReturnPlanDraftCurrencyLengthRule(86400),
            });
    }
}
