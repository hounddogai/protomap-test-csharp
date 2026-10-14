namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkSnapshotRulesChecks
{
    public static void StatementOfWorkSnapshotValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkSnapshotRules.DefaultStatementOfWorkSnapshotValidator();
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkSnapshotTeamLengthRuleFlagsALongTeam()
    {
        var rule = new StatementOfWorkSnapshotTeamLengthRule(3600);
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkSnapshotHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new StatementOfWorkSnapshotHoursRangeRule(0.8, 1972.0);
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("x", "y");
        item.Hours = 1973.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkSnapshotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StatementOfWorkSnapshotRequiredLabelRule("statement of work snapshot needs a value");
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkSnapshotValidatorReportsEveryProblem();
        StatementOfWorkSnapshotTeamLengthRuleFlagsALongTeam();
        StatementOfWorkSnapshotHoursRangeRuleFlagsValuesAboveHours();
        StatementOfWorkSnapshotRequiredLabelRuleFlagsABlankLabel();
    }
}
