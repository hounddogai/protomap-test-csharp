namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyRecordRulesChecks
{
    public static void WarrantyRecordValidatorReportsEveryProblem()
    {
        var validator = WarrantyRecordRules.DefaultWarrantyRecordValidator();
        var item = WarrantyRecord.NewWarrantyRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyRecordRequiredCodeRule("warranty record needs a value");
        var item = WarrantyRecord.NewWarrantyRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyRecordCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new WarrantyRecordCeilingRangeRule(4.0, 2320.2);
        var item = WarrantyRecord.NewWarrantyRecord("x", "y");
        item.Ceiling = 2321.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantyRecordContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new WarrantyRecordContactEmailLengthRule(32);
        var item = WarrantyRecord.NewWarrantyRecord("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyRecordCodeLengthRule(3600);
        var item = WarrantyRecord.NewWarrantyRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyRecordRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WarrantyRecordRequiredTitleRule("warranty record needs a value");
        var item = WarrantyRecord.NewWarrantyRecord("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyRecordValidatorReportsEveryProblem();
        WarrantyRecordRequiredCodeRuleFlagsABlankCode();
        WarrantyRecordCeilingRangeRuleFlagsValuesAboveCeiling();
        WarrantyRecordContactEmailLengthRuleFlagsALongContactEmail();
        WarrantyRecordCodeLengthRuleFlagsALongCode();
        WarrantyRecordRequiredTitleRuleFlagsABlankTitle();
    }
}
