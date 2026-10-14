namespace Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>A rule a benchmark reservation must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkReservationRule
{
    List<string> Check(BenchmarkReservation item);
}

public sealed record BenchmarkReservationLocationLengthRule(int MaxLength) : IBenchmarkReservationRule
{
    public List<string> Check(BenchmarkReservation item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkReservationRequiredCodeRule(string Message) : IBenchmarkReservationRule
{
    public List<string> Check(BenchmarkReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkReservationTitleLengthRule(int MaxLength) : IBenchmarkReservationRule
{
    public List<string> Check(BenchmarkReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkReservationRequiredLocationRule(string Message) : IBenchmarkReservationRule
{
    public List<string> Check(BenchmarkReservation item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark reservation.</summary>
public sealed record BenchmarkReservationValidator(List<IBenchmarkReservationRule> Rules)
{
    public List<string> Validate(BenchmarkReservation item)
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

    public void RequireValid(BenchmarkReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkReservation> items)
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

/// <summary>Functions of benchmark reservation rules.</summary>
public static class BenchmarkReservationRules
{
    public static BenchmarkReservationValidator DefaultBenchmarkReservationValidator()
    {
        return new BenchmarkReservationValidator(
            new List<IBenchmarkReservationRule>
            {
                new BenchmarkReservationLocationLengthRule(255),
                new BenchmarkReservationRequiredCodeRule("benchmark reservation needs a value"),
                new BenchmarkReservationTitleLengthRule(64),
                new BenchmarkReservationRequiredLocationRule("benchmark reservation needs an entry"),
            });
    }
}
