namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayProposalWorkflow(IRetroPayProposalStore Store, RetroPayProposalValidator Validator, IClock Clock, List<string> Journal)
{
    public static RetroPayProposalWorkflow NewRetroPayProposalWorkflow(IRetroPayProposalStore store, IClock clock)
    {
        return new RetroPayProposalWorkflow(
            store,
            RetroPayProposalRules.DefaultRetroPayProposalValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayProposalStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayProposalStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
