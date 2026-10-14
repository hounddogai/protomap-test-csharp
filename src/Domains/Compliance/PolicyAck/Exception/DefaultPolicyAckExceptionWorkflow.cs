namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default policy ack exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultPolicyAckExceptionService(IDefaultPolicyAckExceptionStore Store, DefaultPolicyAckExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default policy ack exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultPolicyAckExceptionStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default policy ack exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultPolicyAckExceptionStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of default policy ack exception workflow.</summary>
public static class DefaultPolicyAckExceptionWorkflow
{
    public static DefaultPolicyAckExceptionService NewDefaultPolicyAckExceptionService(IDefaultPolicyAckExceptionStore store, IClock clock)
    {
        return new DefaultPolicyAckExceptionService(
            store,
            DefaultPolicyAckExceptionRules.DefaultDefaultPolicyAckExceptionValidator(),
            clock,
            new List<string>());
    }
}
