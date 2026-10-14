namespace Acme.Workforce.Kit.Tests.Retry;

using Acme.Workforce.Kit.Retry;

/// <summary>Checks of retry code, run by the project's runner.</summary>
public static class RetryChecks
{
    public static void StopsAtTheFirstSuccess()
    {
        var policy = new Policy(4, new ConstantBackoff(10));
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt == 2);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(used, 3, "attempts");
        Check.Equal(policy.TotalDelay(), 40, "total delay");
    }

    public static void RunAll()
    {
        StopsAtTheFirstSuccess();
    }
}
