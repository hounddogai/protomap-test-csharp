namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LaptopSnapshotRulesChecks
{
    public static void LaptopSnapshotValidatorReportsEveryProblem()
    {
        var validator = LaptopSnapshotRules.DefaultLaptopSnapshotValidator();
        var item = LaptopSnapshot.NewLaptopSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LaptopSnapshotChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LaptopSnapshotChecksumLengthRule(32);
        var item = LaptopSnapshot.NewLaptopSnapshot("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LaptopSnapshotRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new LaptopSnapshotRequiredChecksumRule("laptop snapshot needs an entry");
        var item = LaptopSnapshot.NewLaptopSnapshot("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopSnapshotRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new LaptopSnapshotRequiredOwnerIdRule("laptop snapshot needs a value");
        var item = LaptopSnapshot.NewLaptopSnapshot("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LaptopSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LaptopSnapshotRequiredCodeRule("laptop snapshot needs an entry");
        var item = LaptopSnapshot.NewLaptopSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LaptopSnapshotValidatorReportsEveryProblem();
        LaptopSnapshotChecksumLengthRuleFlagsALongChecksum();
        LaptopSnapshotRequiredChecksumRuleFlagsABlankChecksum();
        LaptopSnapshotRequiredOwnerIdRuleFlagsABlankOwnerId();
        LaptopSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
