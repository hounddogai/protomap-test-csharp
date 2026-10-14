namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>A rule a retro pay record must pass. Check returns its problems, or none.</summary>
public interface IRetroPayRecordRule
{
    List<string> Check(RetroPayRecord item);
}

public sealed record RetroPayRecordRequiredCodeRule(string Message) : IRetroPayRecordRule
{
    public List<string> Check(RetroPayRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRecordRequiredSalaryBandRule(string Message) : IRetroPayRecordRule
{
    public List<string> Check(RetroPayRecord item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRecordRequiredNameRule(string Message) : IRetroPayRecordRule
{
    public List<string> Check(RetroPayRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay record.</summary>
public sealed record RetroPayRecordValidator(List<IRetroPayRecordRule> Rules)
{
    public List<string> Validate(RetroPayRecord item)
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

    public void RequireValid(RetroPayRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayRecord> items)
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

/// <summary>Functions of retro pay record rules.</summary>
public static class RetroPayRecordRules
{
    public static RetroPayRecordValidator DefaultRetroPayRecordValidator()
    {
        return new RetroPayRecordValidator(
            new List<IRetroPayRecordRule>
            {
                new RetroPayRecordRequiredCodeRule("retro pay record needs a value"),
                new RetroPayRecordRequiredSalaryBandRule("retro pay record needs this field"),
                new RetroPayRecordRequiredNameRule("retro pay record needs an entry"),
            });
    }
}
