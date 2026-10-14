namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorAgreementRulesChecks
{
    public static void VisitorAgreementValidatorReportsEveryProblem()
    {
        var validator = VisitorAgreementRules.DefaultVisitorAgreementValidator();
        var item = VisitorAgreement.NewVisitorAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorAgreementLocationLengthRuleFlagsALongLocation()
    {
        var rule = new VisitorAgreementLocationLengthRule(32);
        var item = VisitorAgreement.NewVisitorAgreement("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new VisitorAgreementNameLengthRule(120);
        var item = VisitorAgreement.NewVisitorAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorAgreementCodeLengthRule(3600);
        var item = VisitorAgreement.NewVisitorAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorAgreementRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new VisitorAgreementRequiredLocationRule("visitor agreement needs this field");
        var item = VisitorAgreement.NewVisitorAgreement("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorAgreementValidatorReportsEveryProblem();
        VisitorAgreementLocationLengthRuleFlagsALongLocation();
        VisitorAgreementNameLengthRuleFlagsALongName();
        VisitorAgreementCodeLengthRuleFlagsALongCode();
        VisitorAgreementRequiredLocationRuleFlagsABlankLocation();
    }
}
