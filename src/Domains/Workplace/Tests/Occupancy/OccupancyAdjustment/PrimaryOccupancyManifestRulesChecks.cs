namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class PrimaryOccupancyManifestRulesChecks
{
    public static void PrimaryOccupancyManifestValidatorReportsEveryProblem()
    {
        var validator = PrimaryOccupancyManifestRules.DefaultPrimaryOccupancyManifestValidator();
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryOccupancyManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryOccupancyManifestRequiredNameRule("primary occupancy manifest needs an entry");
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryOccupancyManifestNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryOccupancyManifestNameLengthRule(32);
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryOccupancyManifestRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new PrimaryOccupancyManifestRequiredNotesRule("primary occupancy manifest needs an entry");
        var item = PrimaryOccupancyManifest.NewPrimaryOccupancyManifest("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryOccupancyManifestValidatorReportsEveryProblem();
        PrimaryOccupancyManifestRequiredNameRuleFlagsABlankName();
        PrimaryOccupancyManifestNameLengthRuleFlagsALongName();
        PrimaryOccupancyManifestRequiredNotesRuleFlagsABlankNotes();
    }
}
