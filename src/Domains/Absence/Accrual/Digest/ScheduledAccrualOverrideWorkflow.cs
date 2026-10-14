namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled accrual overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAccrualOverrideWorkflow(IScheduledAccrualOverrideStore Store, ScheduledAccrualOverrideValidator Validator, IClock Clock, List<string> Journal, ScheduledAccrualOverrideEngine Engine)
{
    public static ScheduledAccrualOverrideWorkflow NewScheduledAccrualOverrideWorkflow(IScheduledAccrualOverrideStore store, IClock clock)
    {
        return new ScheduledAccrualOverrideWorkflow(
            store,
            ScheduledAccrualOverrideRules.DefaultScheduledAccrualOverrideValidator(),
            clock,
            new List<string>(),
            ScheduledAccrualOverrideEngine.DefaultScheduledAccrualOverrideEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled accrual override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAccrualOverrideStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled accrual override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAccrualOverrideStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled accrual override {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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
