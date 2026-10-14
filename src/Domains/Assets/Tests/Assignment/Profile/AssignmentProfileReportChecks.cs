namespace Acme.Workforce.Assets.Tests.Assignment.Profile;

using Acme.Workforce.Assets.Assignment.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssignmentProfileReportChecks
{
    public static void AssignmentProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708558678);
        var items = new List<AssignmentProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AssignmentProfile.NewAssignmentProfile($"assignment-profile-{index}", "sample"));
        }
        var report = AssignmentProfileReport.BuildAssignmentProfileReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AssignmentProfileReportCountsEachRecordOnce();
    }
}
