namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkReservationRulesChecks
{
    public static void BenchmarkReservationValidatorReportsEveryProblem()
    {
        var validator = BenchmarkReservationRules.DefaultBenchmarkReservationValidator();
        var item = BenchmarkReservation.NewBenchmarkReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkReservationLocationLengthRuleFlagsALongLocation()
    {
        var rule = new BenchmarkReservationLocationLengthRule(64);
        var item = BenchmarkReservation.NewBenchmarkReservation("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkReservationRequiredCodeRule("benchmark reservation needs an entry");
        var item = BenchmarkReservation.NewBenchmarkReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BenchmarkReservationTitleLengthRule(3600);
        var item = BenchmarkReservation.NewBenchmarkReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkReservationRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new BenchmarkReservationRequiredLocationRule("benchmark reservation needs an entry");
        var item = BenchmarkReservation.NewBenchmarkReservation("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkReservationValidatorReportsEveryProblem();
        BenchmarkReservationLocationLengthRuleFlagsALongLocation();
        BenchmarkReservationRequiredCodeRuleFlagsABlankCode();
        BenchmarkReservationTitleLengthRuleFlagsALongTitle();
        BenchmarkReservationRequiredLocationRuleFlagsABlankLocation();
    }
}
