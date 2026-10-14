namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool draft is in its life cycle.</summary>
public enum BonusPoolDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A bonus pool draft. Its code is unique within a tenant.</summary>
public sealed class BonusPoolDraft(string code, string label, bool pinned, Dictionary<string, string> attributes, string team, int batchSize, string department, string phone, int tenureMonths, string badge, BonusPoolDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public int BatchSize { get; set; } = batchSize;
    public string Department { get; set; } = department;
    public string Phone { get; set; } = phone;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public BonusPoolDraftStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool draft in status proposed.</summary>
    public static BonusPoolDraft NewBonusPoolDraft(string code, string label)
    {
        return new BonusPoolDraft(
            code,
            label,
            false,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            "",
            0,
            "",
            BonusPoolDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolDraftStatus next)
    {
        switch (Status)
        {
            case BonusPoolDraftStatus.Proposed:
                switch (next)
                {
                    case BonusPoolDraftStatus.Withdrawn:
                        return true;
                }
                return false;
            case BonusPoolDraftStatus.Approved:
                return next == BonusPoolDraftStatus.Withdrawn;
            case BonusPoolDraftStatus.Rejected:
                return next == BonusPoolDraftStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BonusPoolDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)TenureMonths + (double)BatchSize, 2);
    }

    public BonusPoolDraft Copy()
    {
        return new BonusPoolDraft(
            Code,
            Label,
            Pinned,
            Attributes,
            Team,
            BatchSize,
            Department,
            Phone,
            TenureMonths,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
