namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class GlobalAccessBadgeRollupChecks
{
    public static void GlobalAccessBadgeRollupStartsInCreated()
    {
        var item = GlobalAccessBadgeRollup.NewGlobalAccessBadgeRollup("code-637", "name-552");
        Check.Equal(item.Status, GlobalAccessBadgeRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAccessBadgeRollupMovesToPublished()
    {
        var item = GlobalAccessBadgeRollup.NewGlobalAccessBadgeRollup("code-637", "name-552");
        try
        {
            item.MoveTo(GlobalAccessBadgeRollupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAccessBadgeRollupStatus.Published, "status");
    }

    public static void GlobalAccessBadgeRollupRejectsMovesFromRetired()
    {
        var item = GlobalAccessBadgeRollup.NewGlobalAccessBadgeRollup("code-637", "name-552");
        item.Status = GlobalAccessBadgeRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalAccessBadgeRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalAccessBadgeRollupStartsInCreated();
        GlobalAccessBadgeRollupMovesToPublished();
        GlobalAccessBadgeRollupRejectsMovesFromRetired();
    }
}
