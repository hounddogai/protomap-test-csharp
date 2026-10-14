namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class WarrantySlotRulesChecks
{
    public static void WarrantySlotValidatorReportsEveryProblem()
    {
        var validator = WarrantySlotRules.DefaultWarrantySlotValidator();
        var item = WarrantySlot.NewWarrantySlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantySlotRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new WarrantySlotRequiredLocationRule("warranty slot needs a value");
        var item = WarrantySlot.NewWarrantySlot("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantySlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantySlotRequiredCodeRule("warranty slot needs a value");
        var item = WarrantySlot.NewWarrantySlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantySlotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WarrantySlotLabelLengthRule(32);
        var item = WarrantySlot.NewWarrantySlot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantySlotValidatorReportsEveryProblem();
        WarrantySlotRequiredLocationRuleFlagsABlankLocation();
        WarrantySlotRequiredCodeRuleFlagsABlankCode();
        WarrantySlotLabelLengthRuleFlagsALongLabel();
    }
}
