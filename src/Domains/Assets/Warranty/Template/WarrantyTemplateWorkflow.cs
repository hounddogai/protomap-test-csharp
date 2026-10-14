namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyTemplateCoordinator(IWarrantyTemplateStore Store, WarrantyTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyTemplateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of warranty template workflow.</summary>
public static class WarrantyTemplateWorkflow
{
    public static WarrantyTemplateCoordinator NewWarrantyTemplateCoordinator(IWarrantyTemplateStore store, IClock clock)
    {
        return new WarrantyTemplateCoordinator(
            store,
            WarrantyTemplateRules.DefaultWarrantyTemplateValidator(),
            clock,
            new List<string>());
    }
}
