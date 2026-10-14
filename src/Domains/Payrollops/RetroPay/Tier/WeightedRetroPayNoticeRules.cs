namespace Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>A rule a weighted retro pay notice must pass. Check returns its problems, or none.</summary>
public interface IWeightedRetroPayNoticeRule
{
    List<string> Check(WeightedRetroPayNotice item);
}

public sealed record WeightedRetroPayNoticeLabelLengthRule(int MaxLength) : IWeightedRetroPayNoticeRule
{
    public List<string> Check(WeightedRetroPayNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRetroPayNoticeRequiredCodeRule(string Message) : IWeightedRetroPayNoticeRule
{
    public List<string> Check(WeightedRetroPayNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRetroPayNoticeRequiredUnitRule(string Message) : IWeightedRetroPayNoticeRule
{
    public List<string> Check(WeightedRetroPayNotice item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted retro pay notice.</summary>
public sealed record WeightedRetroPayNoticeValidator(List<IWeightedRetroPayNoticeRule> Rules)
{
    public List<string> Validate(WeightedRetroPayNotice item)
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

    public void RequireValid(WeightedRetroPayNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRetroPayNotice> items)
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

/// <summary>Functions of weighted retro pay notice rules.</summary>
public static class WeightedRetroPayNoticeRules
{
    public static WeightedRetroPayNoticeValidator DefaultWeightedRetroPayNoticeValidator()
    {
        return new WeightedRetroPayNoticeValidator(
            new List<IWeightedRetroPayNoticeRule>
            {
                new WeightedRetroPayNoticeLabelLengthRule(86400),
                new WeightedRetroPayNoticeRequiredCodeRule("weighted retro pay notice needs a value"),
                new WeightedRetroPayNoticeRequiredUnitRule("weighted retro pay notice needs a value"),
            });
    }
}
