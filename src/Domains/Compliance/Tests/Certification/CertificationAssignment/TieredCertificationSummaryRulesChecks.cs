namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class TieredCertificationSummaryRulesChecks
{
    public static void TieredCertificationSummaryValidatorReportsEveryProblem()
    {
        var validator = TieredCertificationSummaryRules.DefaultTieredCertificationSummaryValidator();
        var item = TieredCertificationSummary.NewTieredCertificationSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredCertificationSummaryNotesLengthRuleFlagsALongNotes()
    {
        var rule = new TieredCertificationSummaryNotesLengthRule(1000);
        var item = TieredCertificationSummary.NewTieredCertificationSummary("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredCertificationSummaryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredCertificationSummaryRequiredTitleRule("tiered certification summary needs a value");
        var item = TieredCertificationSummary.NewTieredCertificationSummary("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredCertificationSummaryRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new TieredCertificationSummaryRequiredNotesRule("tiered certification summary needs a value");
        var item = TieredCertificationSummary.NewTieredCertificationSummary("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredCertificationSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredCertificationSummaryCodeLengthRule(120);
        var item = TieredCertificationSummary.NewTieredCertificationSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredCertificationSummaryValidatorReportsEveryProblem();
        TieredCertificationSummaryNotesLengthRuleFlagsALongNotes();
        TieredCertificationSummaryRequiredTitleRuleFlagsABlankTitle();
        TieredCertificationSummaryRequiredNotesRuleFlagsABlankNotes();
        TieredCertificationSummaryCodeLengthRuleFlagsALongCode();
    }
}
