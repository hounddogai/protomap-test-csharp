namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PayBandGroupChecks
{
    public static void PayBandGroupStartsInOpen()
    {
        var item = PayBandGroup.NewPayBandGroup("code-674", "label-609");
        Check.Equal(item.Status, PayBandGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandGroupMovesToInReview()
    {
        var item = PayBandGroup.NewPayBandGroup("code-674", "label-609");
        try
        {
            item.MoveTo(PayBandGroupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandGroupStatus.InReview, "status");
    }

    public static void PayBandGroupRejectsMovesFromArchived()
    {
        var item = PayBandGroup.NewPayBandGroup("code-674", "label-609");
        item.Status = PayBandGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(PayBandGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        PayBandGroupStartsInOpen();
        PayBandGroupMovesToInReview();
        PayBandGroupRejectsMovesFromArchived();
    }
}
