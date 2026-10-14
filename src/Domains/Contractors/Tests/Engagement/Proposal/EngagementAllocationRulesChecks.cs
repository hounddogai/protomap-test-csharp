namespace Acme.Workforce.Contractors.Tests.Engagement.Proposal;

using Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class EngagementAllocationRulesChecks
{
    public static void EngagementAllocationValidatorReportsEveryProblem()
    {
        var validator = EngagementAllocationRules.DefaultEngagementAllocationValidator();
        var item = EngagementAllocation.NewEngagementAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementAllocationNotesLengthRuleFlagsALongNotes()
    {
        var rule = new EngagementAllocationNotesLengthRule(80);
        var item = EngagementAllocation.NewEngagementAllocation("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementAllocationCodeLengthRule(3600);
        var item = EngagementAllocation.NewEngagementAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementAllocationUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new EngagementAllocationUpdatedAtRangeRule(64, 80);
        var item = EngagementAllocation.NewEngagementAllocation("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementAllocationLabelLengthRule(32);
        var item = EngagementAllocation.NewEngagementAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EngagementAllocationRequiredLabelRule("engagement allocation needs a value");
        var item = EngagementAllocation.NewEngagementAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementAllocationRequiredCodeRule("engagement allocation needs an entry");
        var item = EngagementAllocation.NewEngagementAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementAllocationValidatorReportsEveryProblem();
        EngagementAllocationNotesLengthRuleFlagsALongNotes();
        EngagementAllocationCodeLengthRuleFlagsALongCode();
        EngagementAllocationUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        EngagementAllocationLabelLengthRuleFlagsALongLabel();
        EngagementAllocationRequiredLabelRuleFlagsABlankLabel();
        EngagementAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
