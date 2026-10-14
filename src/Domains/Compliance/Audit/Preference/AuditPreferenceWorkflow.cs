namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves audit preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditPreferenceWorkflow(IAuditPreferenceStore Store, AuditPreferenceValidator Validator, IClock Clock, List<string> Journal, AuditPreferenceEngine Engine)
{
    public static AuditPreferenceWorkflow NewAuditPreferenceWorkflow(IAuditPreferenceStore store, IClock clock)
    {
        return new AuditPreferenceWorkflow(
            store,
            AuditPreferenceRules.DefaultAuditPreferenceValidator(),
            clock,
            new List<string>(),
            AuditPreferenceEngine.DefaultAuditPreferenceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditPreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditPreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit preference {code} not found");
        }
        var result = Engine.Evaluate(item);
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
