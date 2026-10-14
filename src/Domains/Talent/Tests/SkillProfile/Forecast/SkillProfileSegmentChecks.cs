namespace Acme.Workforce.Talent.Tests.SkillProfile.Forecast;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SkillProfileSegmentChecks
{
    public static void SkillProfileSegmentStartsInCreated()
    {
        var item = SkillProfileSegment.NewSkillProfileSegment("code-501", "label-779");
        Check.Equal(item.Status, SkillProfileSegmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileSegmentMovesToPublished()
    {
        var item = SkillProfileSegment.NewSkillProfileSegment("code-501", "label-779");
        try
        {
            item.MoveTo(SkillProfileSegmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileSegmentStatus.Published, "status");
    }

    public static void SkillProfileSegmentRejectsMovesFromRetired()
    {
        var item = SkillProfileSegment.NewSkillProfileSegment("code-501", "label-779");
        item.Status = SkillProfileSegmentStatus.Retired;
        Check.Fails(() => item.MoveTo(SkillProfileSegmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        SkillProfileSegmentStartsInCreated();
        SkillProfileSegmentMovesToPublished();
        SkillProfileSegmentRejectsMovesFromRetired();
    }
}
