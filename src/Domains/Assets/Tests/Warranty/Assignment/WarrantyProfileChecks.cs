namespace Acme.Workforce.Assets.Tests.Warranty.Assignment;

using Acme.Workforce.Assets.Warranty.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WarrantyProfileChecks
{
    public static void WarrantyProfileStartsInCreated()
    {
        var item = WarrantyProfile.NewWarrantyProfile("code-139", "title-247");
        Check.Equal(item.Status, WarrantyProfileStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyProfileMovesToPublished()
    {
        var item = WarrantyProfile.NewWarrantyProfile("code-139", "title-247");
        try
        {
            item.MoveTo(WarrantyProfileStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyProfileStatus.Published, "status");
    }

    public static void WarrantyProfileRejectsMovesFromRetired()
    {
        var item = WarrantyProfile.NewWarrantyProfile("code-139", "title-247");
        item.Status = WarrantyProfileStatus.Retired;
        Check.Fails(() => item.MoveTo(WarrantyProfileStatus.Created), "move");
    }

    public static void RunAll()
    {
        WarrantyProfileStartsInCreated();
        WarrantyProfileMovesToPublished();
        WarrantyProfileRejectsMovesFromRetired();
    }
}
