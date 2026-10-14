namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay agreement must pass. Check returns its problems, or none.</summary>
public interface IRetroPayAgreementRule
{
    List<string> Check(RetroPayAgreement item);
}

public sealed record RetroPayAgreementEmployeeIdLengthRule(int MaxLength) : IRetroPayAgreementRule
{
    public List<string> Check(RetroPayAgreement item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAgreementVersionRangeRule(int Min, int Max) : IRetroPayAgreementRule
{
    public List<string> Check(RetroPayAgreement item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAgreementRequiredEmployeeIdRule(string Message) : IRetroPayAgreementRule
{
    public List<string> Check(RetroPayAgreement item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAgreementAllowedContactEmailRule(List<string> Allowed) : IRetroPayAgreementRule
{
    public List<string> Check(RetroPayAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay agreement.</summary>
public sealed record RetroPayAgreementValidator(List<IRetroPayAgreementRule> Rules)
{
    public List<string> Validate(RetroPayAgreement item)
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

    public void RequireValid(RetroPayAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayAgreement> items)
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

/// <summary>Functions of retro pay agreement rules.</summary>
public static class RetroPayAgreementRules
{
    public static RetroPayAgreementValidator DefaultRetroPayAgreementValidator()
    {
        return new RetroPayAgreementValidator(
            new List<IRetroPayAgreementRule>
            {
                new RetroPayAgreementEmployeeIdLengthRule(32),
                new RetroPayAgreementVersionRangeRule(16, 80),
                new RetroPayAgreementRequiredEmployeeIdRule("retro pay agreement needs an entry"),
                new RetroPayAgreementAllowedContactEmailRule(new List<string> { "weighted", "weighted", "default" }),
            });
    }
}
