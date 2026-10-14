namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayPolicyService(IRetroPayPolicyStore Store, RetroPayPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayPolicyStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayPolicyStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of retro pay policy workflow.</summary>
public static class RetroPayPolicyWorkflow
{
    public static RetroPayPolicyService NewRetroPayPolicyService(IRetroPayPolicyStore store, IClock clock)
    {
        return new RetroPayPolicyService(
            store,
            RetroPayPolicyRules.DefaultRetroPayPolicyValidator(),
            clock,
            new List<string>());
    }
}
