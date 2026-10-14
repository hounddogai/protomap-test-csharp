namespace Acme.Workforce.Compensation.Tests.BonusPool.Exception;

using Acme.Workforce.Compensation.BonusPool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BonusPoolExceptionViewChecks
{
    public static void BonusPoolExceptionViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolException.NewBonusPoolException("code-71", "label-635");
        var view = BonusPoolExceptionView.ToBonusPoolExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolExceptionView.ToBonusPoolExceptionViews(new List<BonusPoolException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolExceptionViewShowsTheStatusInCapitals();
    }
}
