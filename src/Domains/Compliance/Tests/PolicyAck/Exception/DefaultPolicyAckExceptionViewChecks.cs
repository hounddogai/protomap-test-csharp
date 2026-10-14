namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DefaultPolicyAckExceptionViewChecks
{
    public static void DefaultPolicyAckExceptionViewShowsTheStatusInCapitals()
    {
        var item = DefaultPolicyAckException.NewDefaultPolicyAckException("code-778", "name-943");
        var view = DefaultPolicyAckExceptionView.ToDefaultPolicyAckExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPolicyAckExceptionView.ToDefaultPolicyAckExceptionViews(new List<DefaultPolicyAckException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPolicyAckExceptionViewShowsTheStatusInCapitals();
    }
}
