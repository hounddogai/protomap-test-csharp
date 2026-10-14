namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshPolicyService(IRefreshPolicyStore Store, RefreshPolicyValidator Validator, IClock Clock, List<string> Journal, RefreshPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshPolicyStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshPolicyStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh policy {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of refresh policy workflow.</summary>
public static class RefreshPolicyWorkflow
{
    public static RefreshPolicyService NewRefreshPolicyService(IRefreshPolicyStore store, IClock clock)
    {
        return new RefreshPolicyService(
            store,
            RefreshPolicyRules.DefaultRefreshPolicyValidator(),
            clock,
            new List<string>(),
            RefreshPolicyEngine.DefaultRefreshPolicyEngine());
    }
}
