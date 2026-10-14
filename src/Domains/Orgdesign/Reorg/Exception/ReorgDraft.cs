namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg draft is in its life cycle.</summary>
public enum ReorgDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One reorg draft, with what the planners need to act on it.</summary>
public sealed class ReorgDraft(string code, string name, double weight, int limit, string region, string label, string skill, ReorgDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int Limit { get; set; } = limit;
    public string Region { get; set; } = region;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public ReorgDraftStatus Status { get; set; } = status;

    /// <summary>Starts a reorg draft in status draft.</summary>
    public static ReorgDraft NewReorgDraft(string code, string name)
    {
        return new ReorgDraft(code, name, 0.0, 0, "", "", "", ReorgDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgDraftStatus next)
    {
        switch (Status)
        {
            case ReorgDraftStatus.Draft:
                return next == ReorgDraftStatus.Pending;
            case ReorgDraftStatus.Pending:
                return next == ReorgDraftStatus.Active;
            case ReorgDraftStatus.Active:
                switch (next)
                {
                    case ReorgDraftStatus.Closed:
                        return true;
                }
                return false;
            case ReorgDraftStatus.Suspended:
                return next == ReorgDraftStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ReorgDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Weight * (double)Limit, 2);
    }

    public ReorgDraft Copy()
    {
        return new ReorgDraft(Code, Name, Weight, Limit, Region, Label, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
