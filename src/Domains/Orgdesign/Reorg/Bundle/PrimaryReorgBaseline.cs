namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary reorg baseline is in its life cycle.</summary>
public enum PrimaryReorgBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary reorg baseline and its current status.</summary>
public sealed class PrimaryReorgBaseline(string code, string label, bool approved, int graceDays, int revision, double fte, int retries, bool active, string skill, PrimaryReorgBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public int Revision { get; set; } = revision;
    public double Fte { get; set; } = fte;
    public int Retries { get; set; } = retries;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public PrimaryReorgBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a primary reorg baseline in status open.</summary>
    public static PrimaryReorgBaseline NewPrimaryReorgBaseline(string code, string label)
    {
        return new PrimaryReorgBaseline(
            code,
            label,
            false,
            0,
            0,
            0.0,
            0,
            false,
            "",
            PrimaryReorgBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryReorgBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryReorgBaselineStatus next)
    {
        switch (Status)
        {
            case PrimaryReorgBaselineStatus.Open:
                return next == PrimaryReorgBaselineStatus.Resolved;
            case PrimaryReorgBaselineStatus.InReview:
                switch (next)
                {
                    case PrimaryReorgBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryReorgBaselineStatus.Resolved:
                switch (next)
                {
                    case PrimaryReorgBaselineStatus.Reopened:
                    case PrimaryReorgBaselineStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryReorgBaselineStatus.Reopened:
                return next == PrimaryReorgBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryReorgBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary reorg baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)GraceDays * (double)Retries, 2);
    }

    public PrimaryReorgBaseline Copy()
    {
        return new PrimaryReorgBaseline(
            Code,
            Label,
            Approved,
            GraceDays,
            Revision,
            Fte,
            Retries,
            Active,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
