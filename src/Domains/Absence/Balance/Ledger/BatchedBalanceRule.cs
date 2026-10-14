namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched balance rule is in its life cycle.</summary>
public enum BatchedBalanceRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched balance rule as the balance team keeps it.</summary>
public sealed class BatchedBalanceRule(string code, string name, string skill, int revision, List<string> tags, string costCenter, BatchedBalanceRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public int Revision { get; set; } = revision;
    public List<string> Tags { get; set; } = tags;
    public string CostCenter { get; set; } = costCenter;
    public BatchedBalanceRuleStatus Status { get; set; } = status;

    /// <summary>Starts a batched balance rule in status proposed.</summary>
    public static BatchedBalanceRule NewBatchedBalanceRule(string code, string name)
    {
        return new BatchedBalanceRule(code, name, "", 0, new List<string>(), "", BatchedBalanceRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBalanceRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBalanceRuleStatus next)
    {
        switch (Status)
        {
            case BatchedBalanceRuleStatus.Proposed:
                return next == BatchedBalanceRuleStatus.Rejected;
            case BatchedBalanceRuleStatus.Approved:
                switch (next)
                {
                    case BatchedBalanceRuleStatus.Rejected:
                    case BatchedBalanceRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedBalanceRuleStatus.Rejected:
                return next == BatchedBalanceRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedBalanceRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched balance rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public BatchedBalanceRule Copy()
    {
        return new BatchedBalanceRule(Code, Name, Skill, Revision, Tags, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
