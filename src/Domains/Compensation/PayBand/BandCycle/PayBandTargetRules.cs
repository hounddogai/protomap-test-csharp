namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pay band target must pass. Check returns its problems, or none.</summary>
public interface IPayBandTargetRule
{
    List<string> Check(PayBandTarget item);
}

public sealed record PayBandTargetRequiredJobCodeRule(string Message) : IPayBandTargetRule
{
    public List<string> Check(PayBandTarget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandTargetNameLengthRule(int MaxLength) : IPayBandTargetRule
{
    public List<string> Check(PayBandTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandTargetLimitRangeRule(int Min, int Max) : IPayBandTargetRule
{
    public List<string> Check(PayBandTarget item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandTargetRequiredCodeRule(string Message) : IPayBandTargetRule
{
    public List<string> Check(PayBandTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandTargetAllowedNameRule(List<string> Allowed) : IPayBandTargetRule
{
    public List<string> Check(PayBandTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PayBandTargetRequiredNameRule(string Message) : IPayBandTargetRule
{
    public List<string> Check(PayBandTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band target.</summary>
public sealed record PayBandTargetValidator(List<IPayBandTargetRule> Rules)
{
    public List<string> Validate(PayBandTarget item)
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

    public void RequireValid(PayBandTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandTarget> items)
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

/// <summary>Functions of pay band target rules.</summary>
public static class PayBandTargetRules
{
    public static PayBandTargetValidator DefaultPayBandTargetValidator()
    {
        return new PayBandTargetValidator(
            new List<IPayBandTargetRule>
            {
                new PayBandTargetRequiredJobCodeRule("pay band target needs an entry"),
                new PayBandTargetNameLengthRule(120),
                new PayBandTargetLimitRangeRule(32, 86400),
                new PayBandTargetRequiredCodeRule("pay band target needs an entry"),
                new PayBandTargetAllowedNameRule(new List<string> { "primary" }),
                new PayBandTargetRequiredNameRule("pay band target needs an entry"),
            });
    }
}
