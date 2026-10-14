namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientSpanRollupChecks
{
    public static void LenientSpanRollupStartsInCreated()
    {
        var item = LenientSpanRollup.NewLenientSpanRollup("code-512", "name-422");
        Check.Equal(item.Status, LenientSpanRollupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientSpanRollupMovesToValidated()
    {
        var item = LenientSpanRollup.NewLenientSpanRollup("code-512", "name-422");
        try
        {
            item.MoveTo(LenientSpanRollupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientSpanRollupStatus.Validated, "status");
    }

    public static void LenientSpanRollupRejectsMovesFromRetired()
    {
        var item = LenientSpanRollup.NewLenientSpanRollup("code-512", "name-422");
        item.Status = LenientSpanRollupStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientSpanRollupStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientSpanRollupStartsInCreated();
        LenientSpanRollupMovesToValidated();
        LenientSpanRollupRejectsMovesFromRetired();
    }
}
