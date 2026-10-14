namespace Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>A rule a regional asset return reservation must pass. Check returns its problems, or none.</summary>
public interface IRegionalAssetReturnReservationRule
{
    List<string> Check(RegionalAssetReturnReservation item);
}

public sealed record RegionalAssetReturnReservationRequiredNameRule(string Message) : IRegionalAssetReturnReservationRule
{
    public List<string> Check(RegionalAssetReturnReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnReservationCodeLengthRule(int MaxLength) : IRegionalAssetReturnReservationRule
{
    public List<string> Check(RegionalAssetReturnReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnReservationCreatedAtRangeRule(int Min, int Max) : IRegionalAssetReturnReservationRule
{
    public List<string> Check(RegionalAssetReturnReservation item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnReservationRequiredCodeRule(string Message) : IRegionalAssetReturnReservationRule
{
    public List<string> Check(RegionalAssetReturnReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnReservationTitleLengthRule(int MaxLength) : IRegionalAssetReturnReservationRule
{
    public List<string> Check(RegionalAssetReturnReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAssetReturnReservationThresholdRangeRule(double Min, double Max) : IRegionalAssetReturnReservationRule
{
    public List<string> Check(RegionalAssetReturnReservation item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional asset return reservation.</summary>
public sealed record RegionalAssetReturnReservationValidator(List<IRegionalAssetReturnReservationRule> Rules)
{
    public List<string> Validate(RegionalAssetReturnReservation item)
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

    public void RequireValid(RegionalAssetReturnReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAssetReturnReservation> items)
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

/// <summary>Functions of regional asset return reservation rules.</summary>
public static class RegionalAssetReturnReservationRules
{
    public static RegionalAssetReturnReservationValidator DefaultRegionalAssetReturnReservationValidator()
    {
        return new RegionalAssetReturnReservationValidator(
            new List<IRegionalAssetReturnReservationRule>
            {
                new RegionalAssetReturnReservationRequiredNameRule(
                    "regional asset return reservation needs an entry"),
                new RegionalAssetReturnReservationCodeLengthRule(16),
                new RegionalAssetReturnReservationCreatedAtRangeRule(80, 1000),
                new RegionalAssetReturnReservationRequiredCodeRule(
                    "regional asset return reservation needs an entry"),
                new RegionalAssetReturnReservationTitleLengthRule(86400),
                new RegionalAssetReturnReservationThresholdRangeRule(1.1, 4607.6),
            });
    }
}
