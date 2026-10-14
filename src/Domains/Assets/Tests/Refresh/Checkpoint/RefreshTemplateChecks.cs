namespace Acme.Workforce.Assets.Tests.Refresh.Checkpoint;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RefreshTemplateChecks
{
    public static void RefreshTemplateStartsInCreated()
    {
        var item = RefreshTemplate.NewRefreshTemplate("code-406", "title-636");
        Check.Equal(item.Status, RefreshTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshTemplateMovesToValidated()
    {
        var item = RefreshTemplate.NewRefreshTemplate("code-406", "title-636");
        try
        {
            item.MoveTo(RefreshTemplateStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshTemplateStatus.Validated, "status");
    }

    public static void RefreshTemplateRejectsMovesFromRetired()
    {
        var item = RefreshTemplate.NewRefreshTemplate("code-406", "title-636");
        item.Status = RefreshTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(RefreshTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        RefreshTemplateStartsInCreated();
        RefreshTemplateMovesToValidated();
        RefreshTemplateRejectsMovesFromRetired();
    }
}
