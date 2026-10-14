namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionRequestViewChecks
{
    public static void ExceptionRequestViewShowsTheStatusInCapitals()
    {
        var item = ExceptionRequest.NewExceptionRequest("code-466", "label-110");
        var view = ExceptionRequestView.ToExceptionRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionRequestView.ToExceptionRequestViews(new List<ExceptionRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionRequestViewShowsTheStatusInCapitals();
    }
}
